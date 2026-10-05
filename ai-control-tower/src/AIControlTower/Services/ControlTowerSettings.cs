using System.Text.Json;

namespace AIControlTower.Services;

public sealed class ControlTowerSettings
{
    public string RootPath { get; set; } = @"D:\A_KJ\AI";
    public bool EnableJev { get; set; } = false;
    public bool ReduceMotion { get; set; } = false;
    public bool AutoCommunication { get; set; } = true;
    public bool AutoPublishCommunication { get; set; } = true;
    public string CommunicationHubPath { get; set; } = @"D:\A_KJ\AI\ControlTowerData\communication-hub";
    public Dictionary<string, string> CommunicationFolders { get; set; } = new();
    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower");
    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");
    public static ControlTowerSettings Load()
    {
        try { return File.Exists(SettingsPath) ? JsonSerializer.Deserialize<ControlTowerSettings>(File.ReadAllText(SettingsPath)) ?? new() : new(); }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { return new(); }
    }
    public void Save()
    {
        Directory.CreateDirectory(DataDirectory);
        var temporary = SettingsPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, SettingsPath, true);
    }
}
