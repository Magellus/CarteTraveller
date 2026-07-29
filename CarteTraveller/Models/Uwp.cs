using System;
using System.Text.Json.Serialization;

namespace CarteTraveller.Models
{
    public class Uwp
    {
        // Le Starport est une lettre (A, B, C, D, E, X)
        public char Starport { get; set; } = 'E';

        public int Size { get; set; }
        public int Atmosphere { get; set; }
        public int Hydrographics { get; set; }
        public int Temperature { get; set; }
        public int Population { get; set; }
        public int Government { get; set; }
        public int LawLevel { get; set; }
        public int TechLevel { get; set; }

        // En C#, surcharger ToString() est l'équivalent du classique Overrides ToString() en VB.NET.
        // Pratique pour le débuggage ou un affichage rapide dans le UI.
        public override string ToString()
        {
            // On convertit les entiers en format pseudo-hexadécimal (0-9, A-F, etc.)
            // Note: Une vraie méthode d'extension "ToTravellerHex()" serait idéale ici pour gérer les chiffres au-delà de 9.
            return $"{Starport}{Size:X}{Atmosphere:X}{Hydrographics:X}{Population:X}{Government:X}{LawLevel:X}-{TechLevel:X}";
        }
    }
}
