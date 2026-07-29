using CarteTraveller.Models;
using CarteTraveller.Services;
using System.Text;
using System.Windows;
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
        private AppState _currentAppState;
        private GalaxyManager _galaxyManager;

        public MainWindow()
        {
            InitializeComponent();

            // 1. Chargement de l'état au démarrage
            _currentAppState = AppStateService.LoadState();
            _galaxyManager = new GalaxyManager(@"C:\Traveller\Saves\MaCampagne");

            Sector mySector = new Sector();

            // Chargement
            var monSecteurCharge = SectorPersistenceService.LoadSector("mon_secteur.json");

            if (monSecteurCharge != null)
            {
                mySector = monSecteurCharge;
            }
            else
            {
                // Utilise le défaut de 50%
                SectorGeneratorService générateur = new SectorGeneratorService();
                mySector = générateur.GenerateSector("Alpha",-2);
                
                // Sauvegarde
                SectorPersistenceService.SaveSector("mon_secteur.json", mySector);
            }

            MapControl.SectorData = mySector;

        }

        private void LoadMapAt(SectorCoordinate coord)
        {
            // Logique pour centrer ton SectorMapControl
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
            //TxtStatus.Text = string.Empty;

            //if (MapControl.SectorData == null)
            //{
            //    TxtStatus.Foreground = Brushes.Orange;
            //    TxtStatus.Text = "Erreur: Aucun secteur chargé.";
            //    return;
            //}

            //// Parsing sommaire des coordonnées hexagones
            //if (!TryParseHex(TxtOrigin.Text, out var start) || !TryParseHex(TxtTarget.Text, out var target))
            //{
            //    TxtStatus.Foreground = Brushes.Orange;
            //    TxtStatus.Text = "Format d'hexagone invalide. Utilisez le format CCLL (ex: 0205).";
            //    return;
            //}

            //int maxJump = CboJump.SelectedIndex + 1; // Index 0 = Jump-1

            //// Exécution de l'algorithme A*
            //var route = RouteCalculator.FindRoute(_galaxyManager, start, target, maxJump);

            //if (route != null)
            //{
            //    MapControl.CurrentRoute = route;
            //    TxtStatus.Foreground = Brushes.LimeGreen;
            //    TxtStatus.Text = $"Route établie ! {route.Count - 1} saut(s) requis.";
            //}
            //else
            //{
            //    MapControl.CurrentRoute = null; // Efface le tracé précédent s'il existe
            //    TxtStatus.Foreground = Brushes.Coral;
            //    TxtStatus.Text = $"Impossible de tracer la route. Vous avez besoin d'un plus gros moteur (Moteur de saut insuffisant ou système inaccessible).";
            //}
        }

        //private bool TryParseHex(string input, out GlobalHexCoord coord)
        //{
        //    coord = new GlobalHexCoord(0, 0);
        //    if (string.IsNullOrWhiteSpace(input) || input.Length != 4) return false;

        //    if (int.TryParse(input.Substring(0, 2), out int col) && int.TryParse(input.Substring(2, 2), out int row))
        //    {
        //        coord = new GlobalHexCoord(col, row);
        //        return true;
        //    }
        //    return false;
        //}

    }
}