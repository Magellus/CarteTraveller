using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CarteTraveller.Models
{
    public class World
    {
        // Coordonnées de l'hexagone dans le secteur (ex: X = 1, Y = 5 pour "0105")
        public int HexX { get; set; }
        public int HexY { get; set; }

        public string Name { get; set; } = "Unnamed";
        public Uwp Profile { get; set; } = new();

        // Autres attributs Traveller
        public bool HasGasGiant { get; set; }

        public bool HasWater => Profile.Hydrographics >= 1;

        public List<string> TradeCodes { get; set; } = new();
        public string Allegiance { get; set; } = string.Empty;

        // Tes notes de MJ rattachées à ce monde
        public CampaignNotes CampaignData { get; set; } = new();

        // Propriété calculée (Read-Only) qui retourne "0105" basé sur X et Y.
        // L'équivalent d'un Get sans Set en VB.NET.
        [JsonIgnore] // On ne sérialise pas ça en JSON, on peut le déduire de HexX et HexY
        public string HexCoordinate => $"{HexX:D2}{HexY:D2}";
    }
}
