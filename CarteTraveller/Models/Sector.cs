namespace CarteTraveller.Models
{
    public class Sector
    {
        public string Name { get; set; } = "New Sector";

        // Clé: Coordonnée formattée "XXYY" (ex: "0105"). Valeur: L'objet World.
        // En C#, Dictionary<TKey, TValue> est l'équivalent exact de Dictionary(Of TKey, TValue) en VB.NET.
        public Dictionary<string, World> Worlds { get; set; } = new();

        public List<Territory> Territories { get; set; } = new();

        // Méthode utilitaire pour vérifier s'il y a une planète à une coordonnée précise
        public bool HasWorldAt(int x, int y)
        {
            return Worlds.ContainsKey($"{x:D2}{y:D2}");
        }

        public World? GetWorldAt(int x, int y)
        {
            Worlds.TryGetValue($"{x:D2}{y:D2}", out var world);
            return world;
        }
    }
}
