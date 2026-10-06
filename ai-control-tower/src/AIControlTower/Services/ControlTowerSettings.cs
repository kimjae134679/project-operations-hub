using System.Text.Json;

namespace AIControlTower.Services;

public sealed class ControlTowerSettings
{
    public string RootPath { get; set; } = @"D:\A_KJ\AI";
    public bool EnableJev { get; set; } = false;
    public bool ReduceMotion { get; set; } = false;
    public int RosterRefreshSeconds { get; set; } = 5;
    public int RefreshTurnMilliseconds { get; set; } = 1200;
    public bool DarkMode { get; set; } = false;
    public bool AutoCommunication { get; set; } = true;
    public bool AutoPublishCommunication { get; set; } = true;
    public string CommunicationHubPath { get; set; } = @"D:\A_KJ\AI\ControlTowerData\communication-hub";
    public Dictionary<string, string> CommunicationFolders { get; set; } = new();
    public string MultiplayerServerRoot { get; set; } = @"C:\Users\user\Documents\MultiGod\PhoneLOL_LocalRuntime";
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsTemporary { get; set; }
    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower");
    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");
    public static ControlTowerSettings Load()
    {
        try { return File.Exists(SettingsPath) ? JsonSerializer.Deserialize<ControlTowerSettings>(File.ReadAllText(SettingsPath)) ?? new() : new(); }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { return new(); }
    }
    public void Save()
    {
        if (IsTemporary) return;
        Directory.CreateDirectory(DataDirectory);
        var temporary = SettingsPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, SettingsPath, true);
    }
}
