using CarteTraveller.Models;
using System.IO;
using System.Text.Json;

namespace CarteTraveller.Services
{
    public static class SectorPersistenceService
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true, // Pour que ton JSON soit lisible par un humain
            PropertyNameCaseInsensitive = true
        };

        public static void SaveSector(string filePath, Sector sector)
        {
            string jsonString = JsonSerializer.Serialize(sector, _options);
            File.WriteAllText(filePath, jsonString);
        }

        public static Sector? LoadSector(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Sector>(jsonString, _options);
        }
    }
}
