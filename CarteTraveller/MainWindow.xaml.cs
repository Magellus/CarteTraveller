using CarteTraveller.Models;
using CarteTraveller.Services;
using System.Windows;

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

            LoadMapAt(_currentAppState.LastActiveSector,1);
        }

        private void LoadMapAt(SectorCoordinate coord, double zoomLevel)
        {
            //_galaxyManager.GetOrLoadSector(coord)
            // TODO: Pourquoi j'ai fait ça en private ?

            string filename = $"Sector_{coord.X}_{coord.Y}.json";

            Sector mySector = new Sector();
            // Chargement
            var monSecteurCharge = SectorPersistenceService.LoadSector(filename);

            if (monSecteurCharge != null)
            {
                // Campagne existante trouvé
                mySector = monSecteurCharge;
            }
            else
            {
                // Nouvelle campagne
                SectorGeneratorService générateur = new SectorGeneratorService();
                mySector = générateur.GenerateSector("Sector_0_0");

                // Combinaison sécurisée du répertoire et du fichier
                string fullFilePath = System.IO.Path.Combine(_currentAppState.LastCampaignPath, "Sector_0_0.json");
                // Sauvegarde
                SectorPersistenceService.SaveSector(fullFilePath, mySector);
            }
            MapControl.SectorData = mySector;
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