namespace CarteTraveller.Models;

// Struct moderne et immuable pour l'adressage galactique
public readonly record struct GlobalHexCoord(int SectorX, int SectorY, int Col, int Row)
{
    public (int q, int r, int s) ToCube()
    {
        // Conversion en coordonnées absolues (grille continue infinie)
        // Les hexagones locaux sont basés sur 1 (1 à 32, 1 à 40)
        // On soustrait 1 pour faire des mathématiques basées sur 0
        int absCol = (SectorX * 32) + (Col - 1);
        int absRow = (SectorY * 40) + (Row - 1);

        int q = absCol;
        int r = absRow - (absCol - (absCol & 1)) / 2;
        int s = -q - r;

        return (q, r, s);
    }

    public static int Distance(GlobalHexCoord a, GlobalHexCoord b)
    {
        var (q1, r1, s1) = a.ToCube();
        var (q2, r2, s2) = b.ToCube();
        return (Math.Abs(q1 - q2) + Math.Abs(r1 - r2) + Math.Abs(s1 - s2)) / 2;
    }
}