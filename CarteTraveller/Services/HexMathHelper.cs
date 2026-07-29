using System;
using System.Windows;

namespace CarteTraveller.Services
{
    public static class HexMathHelper
    {
        // Doit correspondre à la constante HexSize de ton SectorMapControl
        private const double HexSize = 25.0;

        // --- 1. DÉTECTION DE CLIC (Pixel -> Offset) ---
        public static (int col, int row) PixelToHex(Point clickPoint)
        {
            double height = Math.Sqrt(3.0) * HexSize;

            // CORRECTIF DE CALIBRATION : 
            // Notre boucle OnRender dessine la rangée 1 à Y = height.
            // On soustrait exactement un demi-hexagone en Y pour réaligner la souris 
            // avec l'origine de l'algorithme matriciel standard (Even-Q).
            double adjustedY = clickPoint.Y - (height / 2.0);
            double adjustedX = clickPoint.X;

            // 1. Matrice d'inversion : Pixel vers Axe fractionnaire
            double q = (2.0 / 3.0 * adjustedX) / HexSize;
            double r = (-1.0 / 3.0 * adjustedX + Math.Sqrt(3.0) / 3.0 * adjustedY) / HexSize;

            // 2. Conversion en coordonnées Cubiques fractionnaires (la somme donne toujours 0)
            double x = q;
            double y = r;
            double z = -x - y;

            // Arrondi initial au plus proche
            int rx = (int)Math.Round(x, MidpointRounding.AwayFromZero);
            int ry = (int)Math.Round(y, MidpointRounding.AwayFromZero);
            int rz = (int)Math.Round(z, MidpointRounding.AwayFromZero);

            // 3. Résolution des conflits d'arrondi sur les bordures géométriques
            double xDiff = Math.Abs(rx - x);
            double yDiff = Math.Abs(ry - y);
            double zDiff = Math.Abs(rz - z);

            if (xDiff > yDiff && xDiff > zDiff)
            {
                rx = -ry - rz;
            }
            else if (yDiff > zDiff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            // 4. Reconversion du Cube entier vers notre système Even-Q (Colonnes/Rangées)
            int col = rx;

            // En C#, l'opérateur bitwise & 1 permet d'extraire la parité très proprement
            // Cette formule replace la ligne en fonction du décalage des colonnes paires
            int row = ry + (rx + (rx & 1)) / 2;

            return (col, row);
        }

        // --- 2. POSITIONNEMENT DES RENDERINGS (Offset -> Pixel) ---
        // Utile pour dessiner le tracé A* ou placer des icônes au centre exact
        public static Point HexToPixel(int col, int row)
        {
            double width = 2.0 * HexSize;
            double height = Math.Sqrt(3.0) * HexSize;

            double horizSpacing = width * 0.75;
            double vertSpacing = height;

            double xOffset = col * horizSpacing;
            double yOffset = row * vertSpacing;

            if (col % 2 == 0)
            {
                yOffset += height / 2.0;
            }

            return new Point(xOffset, yOffset);
        }

        // --- 3. CONVERSIONS CUBIQUES (Offset <-> Cube) ---
        public static (int q, int r, int s) OffsetToCube(int col, int row)
        {
            int q = col - 1;
            int r = row - 1 - (col - (col & 1)) / 2;
            int s = -q - r;
            return (q, r, s);
        }

        // --- 4. CALCUL DE DISTANCE SANS ALGORITHME ---
        // Calcule la portée directe en sauts (Jump-N) entre deux hexagones
        public static int GetDistance(int col1, int row1, int col2, int row2)
        {
            var (q1, r1, s1) = OffsetToCube(col1, row1);
            var (q2, r2, s2) = OffsetToCube(col2, row2);

            return (Math.Abs(q1 - q2) + Math.Abs(r1 - r2) + Math.Abs(s1 - s2)) / 2;
        }

    }
}
