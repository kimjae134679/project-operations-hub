using System.Text.Json;

namespace AIControlTower.Services;

public sealed class ControlTowerSettings
{
    public string RootPath { get; set; } = @"D:\A_KJ\AI";
    public bool EnableJev { get; set; } = false;
    public bool ReduceMotion { get; set; } = false;
    public bool DarkMode { get; set; } = false;
    public int RosterRefreshSeconds { get; set; } = 3;
    public int RefreshTurnMilliseconds { get; set; } = 1400;
    public bool AutoCommunication { get; set; } = true;
    public bool AutoPublishCommunication { get; set; } = true;
    public string CommunicationHubPath { get; set; } = @"D:\A_KJ\AI\ControlTowerData\communication-hub";
    public Dictionary<string, string> CommunicationFolders { get; set; } = new();
    public List<string> ContinuousStatePaths { get; set; } = new();
    public string MultiplayerServerRoot { get; set; } = @"C:\Users\user\Documents\MultiGod\PhoneLOL_LocalRuntime";
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsTemporary { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public bool TransientReadOnly { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string? TransientDataDirectory { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string ViewDataDirectory => TransientReadOnly && TransientDataDirectory is not null ? TransientDataDirectory : DataDirectory;
    [System.Text.Json.Serialization.JsonIgnore]
    public string ReadOnlyLoadIssue { get; private set; } = "";
    public static string DataDirectory => Environment.GetEnvironmentVariable("AI_CONTROL_TOWER_DATA") is { Length: > 0 } configured && Path.IsPathFullyQualified(configured)
        ? Path.GetFullPath(configured) : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIControlTower");
    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");
    public static ControlTowerSettings Load()
    {
        try { return File.Exists(SettingsPath) ? JsonSerializer.Deserialize<ControlTowerSettings>(File.ReadAllText(SettingsPath)) ?? new() : new(); }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { return new(); }
    }
    public void Save()
    {
        if (IsTemporary || TransientReadOnly) return;
        Directory.CreateDirectory(DataDirectory);
        var temporary = SettingsPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, SettingsPath, true);
    }
    public static ControlTowerSettings LoadReadOnly(string? path=null)
    {
        var result=new ControlTowerSettings();
        try
        {
            path ??= SettingsPath;
            if(File.Exists(path))
            {
                if((File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException();
                using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
                if(stream.Length>256*1024)throw new InvalidDataException();
                var bytes=new byte[checked((int)stream.Length)];stream.ReadExactly(bytes);
                if(stream.ReadByte()!=-1)throw new InvalidDataException();
                var offset=bytes.AsSpan().StartsWith(new byte[]{0xef,0xbb,0xbf})?3:0;
                result=JsonSerializer.Deserialize<ControlTowerSettings>(bytes.AsSpan(offset),new JsonSerializerOptions{MaxDepth=16})??throw new InvalidDataException();
                result.CommunicationFolders ??= new();result.ContinuousStatePaths ??= new();
            }
        }
        catch(Exception ex) when(ex is IOException or InvalidDataException or JsonException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { result=new(){ReadOnlyLoadIssue="로컬 조회 설정 읽기 보류 · 기존 설정과 파일은 변경하지 않았습니다."}; }
        result.TransientReadOnly=true;return result;
    }
}
