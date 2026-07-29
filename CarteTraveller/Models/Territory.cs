namespace CarteTraveller.Models
{
    public class Territory
    {
        public string Name { get; set; } = "Unnamed Empire";

        // Code couleur ARGB (ex: "#40FF0000" pour un rouge semi-transparent)
        public string ColorHex { get; set; } = "#33FFFFFF";

        // La liste des coordonnées des hexagones qui composent ce territoire (ex: "0105", "0205")
        public List<string> HexCoordinates { get; set; } = new();
    }
}
