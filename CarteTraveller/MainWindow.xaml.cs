using CarteTraveller.Models;
using CarteTraveller.Services;
using System.Windows;

using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarteTraveller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ICampaignContext _campaignContext;
        private readonly IGalaxyProvider _galaxyManager;
        private AppState _currentAppState;

        public MainWindow(IGalaxyProvider galaxyManager, ICampaignContext campaignContext)
        {
            InitializeComponent();
            _galaxyManager = galaxyManager;
            _campaignContext = campaignContext;

            // 1. Chargement de l'état au démarrage
            _currentAppState = AppStateService.LoadState();

            // 2. On injecte le chemin sauvegardé dans notre contexte Singleton
            if (_currentAppState.LastCampaignPath == string.Empty) 
            {
                _currentAppState.LastCampaignPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves", "Defaut");
            }
            _campaignContext.CurrentCampaignPath = _currentAppState.LastCampaignPath;

            // On branche le délégué. 
            // Chaque fois que le contrôle a besoin de dessiner un hexagone d'un secteur, 
            // il appellera GetSector du GalaxyManager.
            MapControl.SectorProvider = coord => _galaxyManager.GetOrLoadSector(coord);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 3. Sauvegarde de l'état exact avant de tuer le processus
            // Assure-toi que _currentAppState.LastActiveSector est mis à jour 
            // pendant ta navigation sur la carte.
            AppStateService.SaveState(_currentAppState);

            base.OnClosing(e);
        }

        private void OnCalculateRouteClick(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text = string.Empty;

            // Parsing sommaire des coordonnées hexagones
            if (!TryParseHex(TxtOrigin.Text, out var start) || !TryParseHex(TxtTarget.Text, out var target))
            {
                TxtStatus.Foreground = Brushes.Orange;
                TxtStatus.Text = "Format d'hexagone invalide. Utilisez le format CCLL (ex: 0205).";
                return;
            }

            int maxJump = CboJump.SelectedIndex + 1; // Index 0 = Jump-1

            // LIMITATION TEMPORAIRE : On s'assure que le départ et l'arrivée 
            // sont dans le secteur actuellement affiché.
            var secteurActif = _currentAppState.LastActiveSector; // ou la coordonnée active (ex: 0,0)

            if (start.SectorX != secteurActif.X || start.SectorY != secteurActif.Y ||
                target.SectorX != secteurActif.X || target.SectorY != secteurActif.Y)
            {
                MessageBox.Show("Calcul de route temporairement limité au secteur actif.", "Limitation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Exécution de l'algorithme A*
            var route = RouteCalculator.FindRoute(_galaxyManager, start, target, maxJump);

            if (route != null)
            {
                MapControl.CurrentRoute = route;
                TxtStatus.Foreground = Brushes.LimeGreen;
                TxtStatus.Text = $"Route établie ! {route.Count - 1} saut(s) requis.";
            }
            else
            {
                MapControl.CurrentRoute = null; // Efface le tracé précédent s'il existe
                TxtStatus.Foreground = Brushes.Coral;
                TxtStatus.Text = $"Impossible de tracer la route. Vous avez besoin d'un plus gros moteur (Moteur de saut insuffisant ou système inaccessible).";
            }
        }

        private bool TryParseHex(string input, out GlobalHexCoord coord)
        {
            coord = new GlobalHexCoord(0, 0, 0, 0);
            if (string.IsNullOrWhiteSpace(input) || input.Length != 8) return false;

            if (int.TryParse(input.Substring(0, 2), out int secCol) && 
                int.TryParse(input.Substring(2, 2), out int secRow) &&
                int.TryParse(input.Substring(2, 2), out int col) &&
                int.TryParse(input.Substring(2, 2), out int row))
            {
                coord = new GlobalHexCoord(secCol, secRow, col, row);
                return true;
            }
            return false;
        }

    }
}