using CarteTraveller.Models;
using System.Collections.Generic;

namespace CarteTraveller.Services
{
    public class RouteCalculator
    {
        public static List<GlobalHexCoord>? FindRoute(IGalaxyProvider galaxy, GlobalHexCoord start, GlobalHexCoord target, int maxJump)
        {
            var openSet = new PriorityQueue<GlobalHexCoord, int>();
            var cameFrom = new Dictionary<GlobalHexCoord, GlobalHexCoord>();
            var gScore = new Dictionary<GlobalHexCoord, int> { [start] = 0 };

            // Heuristique de départ
            openSet.Enqueue(start, GlobalHexCoord.Distance(start, target));

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                // Si le point actuel est le point d'arrivé
                if (current == target)
                {
                    // Reconstitution du chemin
                    var path = new List<GlobalHexCoord> { current };
                    while (cameFrom.ContainsKey(current))
                    {
                        current = cameFrom[current];
                        path.Add(current);
                    }
                    path.Reverse();
                    return path;
                }

                // Découverte dynamique : on demande au Provider les mondes à portée
                foreach (var neighbor in galaxy.GetWorldsInRadius(current, maxJump))
                {
                    int tentativeGScore = gScore[current] + 1; // 1 saut = coût de 1

                    if (tentativeGScore < gScore.GetValueOrDefault(neighbor, int.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;

                        int fScore = tentativeGScore + GlobalHexCoord.Distance(neighbor, target);
                        openSet.Enqueue(neighbor, fScore);
                    }
                }
            }

            return null; // Aucun chemin trouvé avec ce moteur de saut
        }
    }
}
