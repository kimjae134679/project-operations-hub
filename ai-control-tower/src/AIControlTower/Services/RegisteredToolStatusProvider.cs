using AIControlTower.Models;

namespace AIControlTower.Services;

/// <summary>Read-only presence check. Never launches a model, installer, or billable API.</summary>
public sealed class RegisteredToolStatusProvider(string name, string? command, params string[] markers) : IStatusProvider
{
    public string Id => name.ToLowerInvariant();
    public Task<ToolStatus> CheckAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = command is null ? null : EnvironmentProbe.FindCommand(command);
        if (name == "Aider" && path is null)
        {
            var local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin", "aider.exe");
            if (File.Exists(local)) path = local;
        }
        path ??= markers.FirstOrDefault(File.Exists);
        return Task.FromResult(new ToolStatus(Id, name, path is null ? StatusKind.Unknown : StatusKind.Ready,
            path is null ? "등록 위치 또는 PATH에서 확인하지 못했습니다. 설치되지 않았다고 단정하지 않습니다. 사용 안내에서 실제 위치를 확인하세요."
                : "파일 위치 확인 · " + path + " · 실행·모델·외부 연결 성공은 별도 확인합니다.", DateTimeOffset.Now));
    }
}
