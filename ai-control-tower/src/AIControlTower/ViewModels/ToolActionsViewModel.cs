using System.Diagnostics;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed partial class MainViewModel
{
    public void OpenTool(string name)
    {
        if(name is "voicestudio" or "zonos2")
        {
            var folder=Path.Combine(@"D:\A_KJ\AI\Applications",name=="voicestudio" ? "VoiceStudio" : "Zonos2");
            new ProjectGuideWindow(name=="voicestudio" ? "VoiceStudio" : "Zonos2",folder) { Owner=System.Windows.Application.Current?.MainWindow }.Show();
            return;
        }
        if(name is "desktop-commander" or "n8n")
        {
            var url=name=="n8n" ? "http://localhost:5678/" : "https://mcp.desktopcommander.app/";
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute=true })?.Dispose(); }
            catch(Exception ex) { Message="화면 열기 실패 · "+ProcessRunner.Sanitize(ex.Message); }
            return;
        }
        string? path=name switch
        {
            "codex"=>EnvironmentProbe.FindCommand("codex.cmd")??EnvironmentProbe.FindCommand("codex.exe"),
            "github-cli"=>EnvironmentProbe.FindCommand("gh.exe"),
            "aider"=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".local","bin","aider.exe"),
            "hyperframes"=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"npm","node_modules","hyperframes"),
            _=>null
        };
        if(path is not null) { OpenExisting(path); return; }
        var hub=_catalog.Projects.FirstOrDefault(p=>p.Id=="project-operations-hub")?.Path;
        if(hub is not null)
            new ProjectGuideWindow("도구·연결",hub,Path.Combine(hub,"01_CONTROL","TOOLS.md")) { Owner=System.Windows.Application.Current?.MainWindow }.Show();
        else Message="현재 프로젝트 관리 폴더를 찾지 못했습니다.";
    }
}
