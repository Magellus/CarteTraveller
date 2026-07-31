using CarteTraveller.Models;

namespace CarteTraveller.Services;

public interface IGalaxyProvider
{
    // Retourne tous les mondes situés à une distance X d'une coordonnée
    IEnumerable<GlobalHexCoord> GetWorldsInRadius(GlobalHexCoord center, int maxJump);

    void PurgeDistantSectors(SectorCoordinate currentCenter, int keepRadius = 1);

    Sector? GetOrLoadSector(SectorCoordinate coord);

}