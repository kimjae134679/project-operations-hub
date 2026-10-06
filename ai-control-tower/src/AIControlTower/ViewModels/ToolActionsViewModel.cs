using System.Diagnostics;
using AIControlTower.Services;

namespace AIControlTower.ViewModels;

public sealed partial class MainViewModel
{
    public async void OpenTool(string name)
    {
        if (name == "project-bridge") { PcConnection.OpenJobsCommand.Execute(null); return; }
        if(name is "voicestudio" or "zonos2")
        {
            var folder=Path.Combine(@"D:\A_KJ\AI\Applications",name=="voicestudio" ? "VoiceStudio" : "Zonos2");
            await Documents.OpenAsync(folder,Path.Combine(folder,"프로젝트_사용안내.md"),name=="voicestudio" ? "VoiceStudio" : "Zonos2");
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
            await Documents.OpenAsync(hub,Path.Combine(hub,"01_CONTROL","TOOLS.md"),"도구·연결");
        else Message="현재 프로젝트 관리 폴더를 찾지 못했습니다.";
    }
}
