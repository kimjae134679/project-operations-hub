using System.Text.Json;

namespace AIControlTower.Services;

/// <summary>User viewing state only. Never creates or changes an AI notice receipt.</summary>
public sealed class CommunicationReadStore
{
    private readonly string? _path;
    private Dictionary<string, string> _seen = new(StringComparer.Ordinal);
    public CommunicationReadStore(string? path = null)
    {
        _path = path;
        if (path is null) return;
        try { if (File.Exists(path)) _seen = JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(path)) ?? _seen; }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { }
    }
    public bool IsUnread(string identity, string contentHash) => !_seen.ContainsKey(identity+"\n"+contentHash) && _seen.GetValueOrDefault(identity) != contentHash;
    public void MarkRead(string identity, string contentHash)
    {
        if (!IsUnread(identity, contentHash)) return;
        _seen[identity+"\n"+contentHash] = contentHash;
        if (_path is null) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
            var temporary = _path + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(_seen));
            File.Move(temporary, _path, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
}
