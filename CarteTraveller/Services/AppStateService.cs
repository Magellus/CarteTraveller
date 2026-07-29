using System.IO;
using System.Text.Json;
using CarteTraveller.Models;

namespace CarteTraveller.Services;

public static class AppStateService
{
    private static readonly string _stateFilePath = "appstate.json"; // Ajuste le chemin selon ton architecture
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public static AppState LoadState()
    {
        if (!File.Exists(_stateFilePath))
            return new AppState(); // Retourne l'état par défaut

        string json = File.ReadAllText(_stateFilePath);
        return JsonSerializer.Deserialize<AppState>(json, _options) ?? new AppState();
    }

    public static void SaveState(AppState state)
    {
        string json = JsonSerializer.Serialize(state, _options);
        File.WriteAllText(_stateFilePath, json);
    }
}