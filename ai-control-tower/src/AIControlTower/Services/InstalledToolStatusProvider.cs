using System.Text.Json;
using AIControlTower.Models;

namespace AIControlTower.Services;

/// <summary>Checks known installation files only; it never launches or authenticates a tool.</summary>
public sealed class InstalledToolStatusProvider : IStatusProvider
{
    private readonly string _installationPath;
    private readonly string _displayName;
    public string Id { get; }

    public InstalledToolStatusProvider(string id)
    {
        Id = id;
        (_displayName, _installationPath) = id switch
        {
            "aider" => ("Aider", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin", "aider.exe")),
            "hyperframes" => ("HyperFrames", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm", "node_modules", "hyperframes", "package.json")),
            "voicestudio" => ("VoiceStudio", @"D:\A_KJ\AI\Applications\VoiceStudio\VoiceStudio.exe"),
            "zonos2" => ("Zonos2", @"D:\A_KJ\AI\Applications\Zonos2\zonos2-app.exe"),
            _ => throw new ArgumentException("지원하지 않는 설치 확인 도구입니다.", nameof(id))
        };
    }

    public Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(_installationPath))
            return Task.FromResult(new ToolStatus(Id, _displayName, StatusKind.NotInstalled,
                "알려진 설치 위치에서 파일을 찾지 못했습니다. 다른 위치의 설치 여부는 확인하지 않았습니다.", DateTimeOffset.Now));

        var version = "";
        if (Id == "hyperframes")
        {
            try
            {
                using var package = JsonDocument.Parse(File.ReadAllText(_installationPath));
                if (package.RootElement.TryGetProperty("version", out var value) && value.ValueKind == JsonValueKind.String)
                {
                    var candidate = value.GetString() ?? "";
                    if (candidate.Length <= 80 && candidate.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '+'))
                        version = candidate;
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
            {
                // A package version read failure does not imply the tool was never installed.
            }
        }
        var detail = "설치 파일" + (version.Length > 0 ? " · 버전 " + version : "") +
            " 확인. 실행·모델 연결·실제 작업 성공은 미검증입니다. 설치 위치: " + _installationPath;
        return Task.FromResult(new ToolStatus(Id, _displayName, StatusKind.Ready, detail, DateTimeOffset.Now));
    }
}
