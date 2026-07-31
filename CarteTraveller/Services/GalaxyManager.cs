using System.IO;
using System.Collections.Generic;
using CarteTraveller.Models;

namespace CarteTraveller.Services;

public class GalaxyManager : IGalaxyProvider
{
    private readonly Dictionary<SectorCoordinate, Sector> _loadedSectors = new();
    private readonly ICampaignContext _campaignContext;

    public GalaxyManager(ICampaignContext campaignContext)
    {
        _campaignContext = campaignContext;
    }

    /// <summary>
    /// Sert à charger une liste de coordonné universelle des étoiles à l'intérieur du saut maximum pour calculer la route.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="maxJump"></param>
    /// <returns></returns>
    public IEnumerable<GlobalHexCoord> GetWorldsInRadius(GlobalHexCoord center, int maxJump)
    {
        // 1. Convertir le centre en coordonnées absolues (grille continue infinie)
        int centerAbsCol = (center.SectorX * 32) + center.Col - 1;
        int centerAbsRow = (center.SectorY * 40) + center.Row - 1;

        // 2. Définir la boîte englobante (Rayon maximal du saut)
        int minAbsCol = centerAbsCol - maxJump;
        int maxAbsCol = centerAbsCol + maxJump;
        int minAbsRow = centerAbsRow - maxJump;
        int maxAbsRow = centerAbsRow + maxJump;

        // 3. Déterminer quels secteurs sont touchés par cette boîte
        // Utilisation de Math.Floor cruciale pour gérer correctement les secteurs négatifs
        int minSectorX = (int)Math.Floor((double)minAbsCol / 32);
        int maxSectorX = (int)Math.Floor((double)maxAbsCol / 32);
        int minSectorY = (int)Math.Floor((double)minAbsRow / 40);
        int maxSectorY = (int)Math.Floor((double)maxAbsRow / 40);

        // 4. Parcourir uniquement les secteurs potentiellement touchés
        for (int secX = minSectorX; secX <= maxSectorX; secX++)
        {
            for (int secY = minSectorY; secY <= maxSectorY; secY++)
            {
                var sectorCoord = new SectorCoordinate(secX, secY);
                var sector = GetOrLoadSector(sectorCoord);

                if (sector == null) continue; // Espace vide, pas de fichier JSON

                // 5. Filtrer les mondes du secteur par distance réelle (Hexagonale)
                foreach (var kvp in sector.Worlds)
                {
                    var localWorld = kvp.Value;
                    var globalCandidate = new GlobalHexCoord(secX, secY, localWorld.HexX, localWorld.HexY);

                    if (GlobalHexCoord.Distance(center, globalCandidate) <= maxJump)
                    {
                        // L'utilisation de yield return évite de créer des listes intermédiaires
                        yield return globalCandidate;
                    }
                }
            }
        }
    }

    public Sector? GetOrLoadSector(SectorCoordinate coord)
    {
        if (_loadedSectors.TryGetValue(coord, out var sector))
            return sector;

        // Si non chargé, on utilise ton SectorPersistenceService
        string campaignPath = _campaignContext.CurrentCampaignPath;
        string filename = $"Sector_{coord.X}_{coord.Y}.json";
        string fullPath = System.IO.Path.Combine(campaignPath, filename);

        sector = SectorPersistenceService.LoadSector(fullPath);
        if (sector != null)
        {
            _loadedSectors[coord] = sector;
        }
        // TODO: générer un secteur ici si sector est null.
        return sector;
    }

    /// <summary>
    /// Va vider le dictionnaire de secteur au fur et à la mesure qu'on se déplace sur la carte.
    /// </summary>
    /// <param name="currentCenter"></param>
    /// <param name="keepRadius"></param>
    void IGalaxyProvider.PurgeDistantSectors(SectorCoordinate currentCenter, int keepRadius)
    {
        var keysToRemove = new List<SectorCoordinate>();

        foreach (var loadedCoord in _loadedSectors.Keys)
        {
            // Si la distance X ou Y dépasse le rayon de conservation, on marque pour suppression
            if (Math.Abs(loadedCoord.X - currentCenter.X) > keepRadius ||
                Math.Abs(loadedCoord.Y - currentCenter.Y) > keepRadius)
            {
                keysToRemove.Add(loadedCoord);
            }
        }

        foreach (var key in keysToRemove)
        {
            // Optionnel : Sauvegarder si l'état en RAM est "Dirty" (modifié mais non sauvegardé)
            _loadedSectors.Remove(key);
        }
    }

    //TODO: faire la même chose mais pour charger des secteur.


    public void ClearCache()
    {
        _loadedSectors.Clear();
    }

}