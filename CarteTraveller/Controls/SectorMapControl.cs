using CarteTraveller.Models;
using CarteTraveller.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CarteTraveller.Controls;

public class SectorMapControl : FrameworkElement
{
    // Coordonnée du secteur affiché (ex: 0,0 ou 1,-1)
    public static readonly DependencyProperty ActiveSectorCoordinateProperty =
        DependencyProperty.Register(
            nameof(ActiveSectorCoordinate),
            typeof(SectorCoordinate),
            typeof(SectorMapControl),
            new FrameworkPropertyMetadata(new SectorCoordinate(0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

    public SectorCoordinate ActiveSectorCoordinate
    {
        get => (SectorCoordinate)GetValue(ActiveSectorCoordinateProperty);
        set => SetValue(ActiveSectorCoordinateProperty, value);
    }

    // Un délégué qui prend une coordonnée de secteur et retourne le secteur correspondant.
    // La Vue demande, le parent (MainWindow) fournit.
    public Func<SectorCoordinate, Sector?>? SectorProvider { get; set; }

    // NOUVELLE propriété pour le tracé de la route
    public static readonly DependencyProperty CurrentRouteProperty =
        DependencyProperty.Register(
            nameof(CurrentRoute),
            typeof(List<GlobalHexCoord>),
            typeof(SectorMapControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public List<GlobalHexCoord>? CurrentRoute
    {
        get => (List<GlobalHexCoord>?)GetValue(CurrentRouteProperty);
        set => SetValue(CurrentRouteProperty, value);
    }

    // Le rayon de l'hexagone (la distance entre le centre et un sommet)
    private const double HexSize = 25.0;

    private Point? _dragStart = null;
    private Vector _mapOffset = new Vector(0, 0); // Le décalage actuel de la caméra
    private Vector _tempDragOffset = new Vector(0, 0); // Le décalage pendant qu'on glisse

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // On calcule le décalage total (permanent + le drag en cours)
        double totalOffsetX = _mapOffset.X + _tempDragOffset.X;
        double totalOffsetY = _mapOffset.Y + _tempDragOffset.Y;

        // On pousse une transformation spatiale. 
        // TOUT ce qui sera dessiné ensuite sera automatiquement décalé par WPF.
        dc.PushTransform(new TranslateTransform(totalOffsetX, totalOffsetY));

        // Déclare le pinceau transparent en haut de OnRender avec tes autres ressources
        var transparentBrush = Brushes.Transparent;
        transparentBrush.Freeze(); // Toujours geler pour les performances

        // Astuce Senior: Toujours appeler Freeze() sur les objets GDI dans OnRender.
        // Cela empêche WPF de surveiller leurs changements d'état et booste les performances.
        var hexPen = new Pen(Brushes.DimGray, 1.0);
        hexPen.Freeze();

        var worldBrush = Brushes.White;
        var worldBrushBlue = Brushes.Blue;

        worldBrush.Freeze();
        worldBrushBlue.Freeze();

        // Préparation de la typographie (en dehors de la boucle)
        var typeface = new Typeface("Consolas"); // Une police monospace est idéale pour les coordonnées
        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip; // Requis par .NET moderne

        // Mathématiques corrigées pour un hexagone "Flat-Topped" (Sommet plat)
        double width = 2 * HexSize;
        double height = Math.Sqrt(3) * HexSize;

        // Dans cette orientation, la distance horizontale est de 3/4 de la largeur
        double horizSpacing = width * 0.75; 
        double vertSpacing = height;

        // --- DÉBUT DU CULLING ---
        // On récupère la taille actuelle du contrôle à l'écran
        double viewWidth = this.ActualWidth;
        double viewHeight = this.ActualHeight;

        // On calcule la "boîte" de pixels visible, en inversant le décalage de la caméra
        // Si je me déplace de +100 pixels, la caméra regarde les pixels à -100.
        Point topLeftPixel = new Point(-totalOffsetX, -totalOffsetY);
        Point bottomRightPixel = new Point(-totalOffsetX + viewWidth, -totalOffsetY + viewHeight);

        // On demande au MathHelper quels hexagones se trouvent dans ces coins
        var (minCol, minRow) = HexMathHelper.PixelToHex(topLeftPixel);
        var (maxCol, maxRow) = HexMathHelper.PixelToHex(bottomRightPixel);

        // On ajoute une "marge de sécurité" (Buffer) de 2 hexagones.
        // Cela empêche de voir les hexagones disparaître subitement sur les bords de l'écran pendant qu'on glisse.
        int startCol = minCol - 2;
        int endCol = maxCol + 2;
        int startRow = minRow - 2;
        int endRow = maxRow + 2;

        // --- FIN DU CULLING ---


        for (int globalCol = startCol; globalCol <= endCol; globalCol++)
        {
            for (int globalRow = startRow; globalRow <= endRow; globalRow++)
            {
                // Calcul de la position X et Y
                double xOffset = globalCol * horizSpacing;
                double yOffset = globalRow * vertSpacing;

                // Décalage vertical pour les colonnes paires (crée l'imbrication hexagonale)
                if (globalCol % 2 == 0)
                {
                    yOffset += height / 2.0;
                }

                var center = new Point(xOffset, yOffset);

                // Dessiner la forme de l'hexagone
                DrawHexagon(dc, center, hexPen, transparentBrush);

                // --- AFFICHAGE TEMPORAIRE POUR LE DÉBOGAGE ---
                // Pour l'instant, on affiche la coordonnée GLOBALE pour que tu voies la caméra travailler.
                // Si tu glisses vers la gauche, tu vas voir des colonnes négatives (ex: -01, -02).
                string hexCoord = $"{globalCol}{globalRow}";
                var formattedText = new FormattedText(
                    hexCoord,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    9, // Taille de la police
                    Brushes.Azure,
                    dpi);

                // Calcul pour centrer le texte horizontalement et le placer en haut de l'hexagone
                double textX = center.X - (formattedText.Width / 2);
                double textY = center.Y - (height / 2) + 2; // Juste en dessous de la bordure supérieure

                dc.DrawText(formattedText, new Point(textX, textY));


                // --- GESTION DES DONNÉES  ---
                // Trouver dans quel secteur macroscopique se trouve cet hexagone global
                var coordSecteur = HexMathHelper.GetSectorFromGlobal(globalCol, globalRow);

                // Trouver la coordonnée locale (ex: 0105) à l'intérieur de ce secteur
                var (localCol, localRow) = HexMathHelper.GetLocalFromGlobal(globalCol, globalRow);

                // Demander le secteur au fournisseur (qui fera un cache hit ou un chargement JSON)
                Sector? secteurCible = SectorProvider?.Invoke(coordSecteur);

                // On s'assure de ne dessiner les planètes que si on regarde la bonne zone.
                if (secteurCible != null && secteurCible.HasWorldAt(localCol, localRow))
                {
                    World? world = secteurCible.GetWorldAt(localCol, localRow);
                    if (world != null)
                    {
                        if (world.HasWater)
                        {
                            dc.DrawEllipse(worldBrushBlue, null, center, 4.0, 4.0);
                        }
                        else
                        {
                            dc.DrawEllipse(worldBrush, null, center, 4.0, 4.0);
                        }
                    }
                }

            }

        }

        // Rendu du tracé de saut (Route vectorielle)
        if (CurrentRoute != null && CurrentRoute.Count > 1)
        {
            var routePen = new Pen(Brushes.LimeGreen, 3.0)
            {
                DashStyle = DashStyles.Dash // Aspect ligne discontinue hyperespace
            };
            routePen.Freeze();

            var routeGeometry = new StreamGeometry();
            using (var ctx = routeGeometry.Open())
            {
                for (int i = 0; i < CurrentRoute.Count; i++)
                {
                    Point p = GetHexCenter(CurrentRoute[i].Col, CurrentRoute[i].Row, horizSpacing, vertSpacing, height);

                    if (i == 0)
                        ctx.BeginFigure(p, isFilled: false, isClosed: false);
                    else
                        ctx.LineTo(p, isStroked: true, isSmoothJoin: true);
                }
            }
            routeGeometry.Freeze();
            dc.DrawGeometry(null, routePen, routeGeometry);

            // Dessiner des marqueurs de surbrillance sur les étapes
            var haloBrush = new SolidColorBrush(Color.FromArgb(100, 50, 205, 50));
            haloBrush.Freeze();
            foreach (var step in CurrentRoute)
            {
                Point p = GetHexCenter(step.Col, step.Row, horizSpacing, vertSpacing, height);
                dc.DrawEllipse(haloBrush, new Pen(Brushes.LimeGreen, 1), p, 8.0, 8.0);
            }
        }

        // À la toute fin de OnRender, on retire la transformation
        dc.Pop();

    }

    private void DrawHexagon(DrawingContext dc, Point center, Pen pen, Brush fond)
    {
        // StreamGeometry est plus léger et rapide que PathGeometry
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            for (int i = 0; i < 6; i++)
            {
                // Angle en radians pour chaque sommet (décalé de 30 degrés pour la pointe en haut)
                double angle_deg = 60 * i;
                double angle_rad = Math.PI / 180 * angle_deg;

                var point = new Point(
                    center.X + HexSize * Math.Cos(angle_rad),
                    center.Y + HexSize * Math.Sin(angle_rad));

                if (i == 0)
                    ctx.BeginFigure(point, true, true);
                else
                    ctx.LineTo(point, true, false);
            }
        }
        geometry.Freeze();
        dc.DrawGeometry(fond, pen, geometry);
    }

    // Méthode utilitaire pour extraire le calcul du centre
    private Point GetHexCenter(int col, int row, double horizSpacing, double vertSpacing, double height)
    {
        double xOffset = col * horizSpacing;
        double yOffset = row * vertSpacing;
        if (col % 2 == 0) yOffset += height / 2.0;

        return new Point(xOffset, yOffset);
    }

    #region Surcharge souris


    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        // Capture la souris pour continuer à recevoir les événements 
        // même si le curseur sort de la zone du contrôle
        CaptureMouse();
        _dragStart = e.GetPosition(this);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_dragStart.HasValue && IsMouseCaptured)
        {
            var currentPosition = e.GetPosition(this);
            // Calcule la distance parcourue depuis le clic
            _tempDragOffset = currentPosition - _dragStart.Value;

            // Force WPF à redessiner immédiatement
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();
            _dragStart = null;

            // On consolide le déplacement temporaire dans le déplacement global
            _mapOffset += _tempDragOffset;
            _tempDragOffset = new Vector(0, 0);

            // TODO plus tard : C'est ICI qu'on vérifiera si le décalage justifie 
            // de charger le secteur adjacent via le GalaxyManager.
        }
    }


    #endregion

}