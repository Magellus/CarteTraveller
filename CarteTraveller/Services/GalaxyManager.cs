using System;
using System.Collections.Generic;
using CarteTraveller.Models;

namespace CarteTraveller.Services;

public class GalaxyManager : IGalaxyProvider
{
    private readonly Dictionary<SectorCoordinate, Sector> _loadedSectors = new();
    private readonly string _storagePath;

    public GalaxyManager(string storagePath)
    {
        _storagePath = storagePath;
    }

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

    private Sector? GetOrLoadSector(SectorCoordinate coord)
    {
        if (_loadedSectors.TryGetValue(coord, out var sector))
            return sector;

        // Si non chargé, on utilise ton SectorPersistenceService
        string filename = $"Sector_{coord.X}_{coord.Y}.json";
        string fullPath = System.IO.Path.Combine(_storagePath, filename);

        sector = SectorPersistenceService.LoadSector(fullPath);
        if (sector != null)
        {
            _loadedSectors[coord] = sector;
        }

        return sector;
    }
}