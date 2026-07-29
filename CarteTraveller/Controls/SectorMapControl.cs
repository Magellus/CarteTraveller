using CarteTraveller.Models;
using CarteTraveller.Services;
using System;
using System.Windows;
using System.Windows.Media;

namespace CarteTraveller.Controls;

public class SectorMapControl : FrameworkElement
{
    // Propriété de dépendance pour lier le modèle Sector
    public static readonly DependencyProperty SectorDataProperty =
        DependencyProperty.Register(
            nameof(SectorData),
            typeof(Sector),
            typeof(SectorMapControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Sector? SectorData
    {
        get => (Sector?)GetValue(SectorDataProperty);
        set => SetValue(SectorDataProperty, value);
    }

    // 2. NOUVELLE propriété pour le tracé de la route
    public static readonly DependencyProperty CurrentRouteProperty =
        DependencyProperty.Register(
            nameof(CurrentRoute),
            typeof(List<HexCoord>),
            typeof(SectorMapControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public List<HexCoord>? CurrentRoute
    {
        get => (List<HexCoord>?)GetValue(CurrentRouteProperty);
        set => SetValue(CurrentRouteProperty, value);
    }

    // Le rayon de l'hexagone (la distance entre le centre et un sommet)
    private const double HexSize = 25.0;

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // 0. Déclare le pinceau transparent en haut de OnRender avec tes autres ressources
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

        // 1. Préparation de la typographie (en dehors de la boucle)
        var typeface = new Typeface("Consolas"); // Une police monospace est idéale pour les coordonnées
        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip; // Requis par .NET moderne

        // Mathématiques corrigées pour un hexagone "Flat-Topped" (Sommet plat)
        double width = 2 * HexSize;
        double height = Math.Sqrt(3) * HexSize;

        // Dans cette orientation, la distance horizontale est de 3/4 de la largeur
        double horizSpacing = width * 0.75; 
        double vertSpacing = height; 

        // Dessin de la grille 32x40 standard de Traveller
        for (int col = 1; col <= 32; col++)
        {
            for (int row = 1; row <= 40; row++)
            {
                // Calcul de la position X et Y
                double xOffset = col * horizSpacing;
                double yOffset = row * vertSpacing;

                // Décalage vertical pour les colonnes paires (crée l'imbrication hexagonale)
                if (col % 2 == 0)
                {
                    yOffset += height / 2.0;
                }

                var center = new Point(xOffset, yOffset);

                // 1. Dessiner la forme de l'hexagone
                DrawHexagon(dc, center, hexPen, transparentBrush);

                // 2. Formatage et dessin du texte des coordonnées
                string hexCoord = $"{col:D2}{row:D2}";
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

                // 3. S'il y a des données et qu'un monde existe à cette coordonnée, on le dessine
                if (SectorData != null && SectorData.HasWorldAt(col, row))
                {
                    // Dessine le système stellaire (un simple cercle blanc pour l'instant)
                    World? world = SectorData.GetWorldAt(col, row);
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

        // 4. Rendu du tracé de saut (Route vectorielle)
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

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        // Récupération de la coordonnée X,Y exacte par rapport au coin supérieur gauche du contrôle
        Point clickPosition = e.GetPosition(this);

        // Appel à notre moteur mathématique
        var (col, row) = HexMathHelper.PixelToHex(clickPosition);

        // Validation : On s'assure que le clic tombe dans les limites du secteur Traveller
        if (col >= 1 && col <= 32 && row >= 1 && row <= 40)
        {
            string hexCoord = $"{col:D2}{row:D2}";
            System.Windows.MessageBox.Show($"Cible acquise : Hexagone {hexCoord}");

            // Plus tard, c'est ici qu'on mettra à jour la propriété SelectedHexagon du ViewModel
        }
    }

}