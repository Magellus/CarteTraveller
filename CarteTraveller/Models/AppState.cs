namespace CarteTraveller.Models;

public class AppState
{
    // Coordonnée par défaut si c'est la toute première ouverture
    public SectorCoordinate LastActiveSector { get; set; } = new(0, 0);

    // Tu pourras ajouter d'autres choses ici plus tard
    public double LastZoomLevel { get; set; } = 1.0;
}