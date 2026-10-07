using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class ManualLaunchNoActivationTests
{
    [Fact] public void ExplicitNoActivationManualModeUsesExistingManualIdentity()
    {
        Assert.True(StartupPolicy.TryCreate(["--manual-control","--no-activate-existing"],out var policy));
        Assert.True(policy.IsManualControl);Assert.Equal("ManualControl",policy.InstanceIdentity);
        Assert.False(AIControlTower.App.ShouldActivateExistingInstance(["--manual-control","--no-activate-existing"]));
    }
    [Fact] public void OrdinaryManualActivationContractIsUnchanged()
    {Assert.True(StartupPolicy.TryCreate(["--manual-control"],out _));Assert.True(AIControlTower.App.ShouldActivateExistingInstance(["--manual-control"]));}
    [Theory]
    [InlineData("--no-activate-existing")]
    [InlineData("--no-activate-existing|--manual-control")]
    [InlineData("--manual-control|--no-activate-existing|--no-activate-existing")]
    [InlineData("--manual-control|--manual-control|--no-activate-existing")]
    [InlineData("--manual-control|--no-activate-existing|--background")]
    [InlineData("--manual-control|--no-activate-existing|--local-view")]
    [InlineData("--manual-control|--no-activate-existing|--remote-supervisor")]
    [InlineData("--local-view|--no-activate-existing")]
    [InlineData("--remote-control|--no-activate-existing")]
    public void MalformedNoActivationFlagsNeverFallThrough(string flags)
    {Assert.False(StartupPolicy.TryCreate(flags.Split('|'),out _));}

    private static string SourceRoot()
    {var d=new DirectoryInfo(AppContext.BaseDirectory);while(d!=null&&!File.Exists(Path.Combine(d.FullName,"AGENTS.md")))d=d.Parent;Assert.NotNull(d);return d!.FullName;}
    private sealed record Fixture(string Root,string Manifest,string Version,string Commit,string Hash,string VersionInfo);
    private static Fixture CreateFixture(string defect)
    {
        var root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks","manual-launch-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        var exe=Path.Combine(root,"AIControlTower.exe");File.Copy(typeof(StartupPolicy).Assembly.Location,exe);
        var info=FileVersionInfo.GetVersionInfo(exe);var version=$"{info.FileMajorPart}.{info.FileMinorPart}.{info.FileBuildPart}";var commit=info.ProductVersion!.Split('+')[1];var hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(exe))).ToLowerInvariant();
        var manifest=Path.Combine(root,"DEPLOYMENT.json");
        var doc=new Dictionary<string,object>{["version"]=version,["sourceCommit"]=commit,["fileSha256"]=hash,["fileVersion"]=info.FileVersion!,["productVersion"]=info.ProductVersion!,["expiresAtUtc"]=DateTimeOffset.UtcNow.AddMinutes(10).ToString("O")};
        switch(defect){case "hash":doc["fileSha256"]=new string('0',64);break;case "version":doc["fileVersion"]="1.0.0.0";break;case "commit":doc["sourceCommit"]=new string('b',40);break;case "commit_binding":commit=new string('b',40);doc["sourceCommit"]=commit;break;case "expired":doc["expiresAtUtc"]=DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O");break;case "missing":doc.Remove("productVersion");break;}
        File.WriteAllText(manifest,JsonSerializer.Serialize(doc));return new(root,manifest,version,commit,hash,info.FileVersion!);
    }
    private static Dictionary<string,string> Snapshot(string root)=>Directory.GetFiles(root,"*",SearchOption.AllDirectories).ToDictionary(x=>x,x=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(x)))+":"+(int)File.GetAttributes(x));
    private static async Task<(int Exit,string Out)> Check(Fixture f,bool checkOnly=true)
    {
        var script=Path.Combine(SourceRoot(),"scripts","start_manual_control.ps1");Assert.True(File.Exists(script),"Verified manual launcher is not implemented.");
        var start=new ProcessStartInfo("powershell.exe"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        foreach(var arg in new[]{"-NoLogo","-NoProfile","-NonInteractive","-ExecutionPolicy","Bypass","-File",script,"-VersionRoot",f.Root,"-ExpectedVersion",f.Version,"-ExpectedSourceCommit",f.Commit,"-ExpectedSha256",f.Hash,"-ManifestPath",f.Manifest})start.ArgumentList.Add(arg);
        if(checkOnly)start.ArgumentList.Add("-CheckOnly");
        using var p=Process.Start(start)!;var output=p.StandardOutput.ReadToEndAsync();var error=p.StandardError.ReadToEndAsync();
        using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(20));await p.WaitForExitAsync(timeout.Token);var text=await output;Assert.True(string.IsNullOrWhiteSpace(await error));return(p.ExitCode,text);
    }
    [Theory][InlineData("hash")][InlineData("version")][InlineData("commit")][InlineData("expired")][InlineData("missing")][InlineData("commit_binding")]
    public async Task CheckOnlyRejectsInvalidDescriptorWithoutMutatingFixture(string defect)
    {
        var f=CreateFixture(defect);var before=Snapshot(f.Root);var result=await Check(f);Assert.NotEqual(0,result.Exit);
        using var json=JsonDocument.Parse(result.Out);Assert.False(json.RootElement.GetProperty("started").GetBoolean());Assert.Equal("held",json.RootElement.GetProperty("status").GetString());
        Assert.Equal(before.OrderBy(x=>x.Key),Snapshot(f.Root).OrderBy(x=>x.Key));
    }
    [Fact] public async Task CheckOnlyValidatesOwnedDescriptorButNeverStartsOrProbesOperatingInstance()
    {
        var f=CreateFixture("");var before=Snapshot(f.Root);var result=await Check(f);Assert.Equal(0,result.Exit);
        using var json=JsonDocument.Parse(result.Out);Assert.Equal("verified",json.RootElement.GetProperty("status").GetString());Assert.False(json.RootElement.GetProperty("started").GetBoolean());Assert.False(json.RootElement.GetProperty("instanceProbed").GetBoolean());
        Assert.Equal(before.OrderBy(x=>x.Key),Snapshot(f.Root).OrderBy(x=>x.Key));
    }
    [Fact] public async Task FixtureOverrideWithoutCheckOnlyIsRejectedBeforeAnyLaunch()
    {var f=CreateFixture("");var result=await Check(f,false);Assert.NotEqual(0,result.Exit);using var json=JsonDocument.Parse(result.Out);Assert.False(json.RootElement.GetProperty("started").GetBoolean());}
    [Fact] public void LauncherUsesNoActivationChildContractAndNoFocusOrControlCommands()
    {
        var script=Path.Combine(SourceRoot(),"scripts","start_manual_control.ps1");Assert.True(File.Exists(script));var text=File.ReadAllText(script);
        Assert.Contains("--manual-control --no-activate-existing",text);Assert.Contains("WaitOne(0)",text);Assert.Contains("ReleaseMutex",text);
        foreach(var forbidden in new[]{"Stop-Process","SetForegroundWindow",".Set()","Register-ScheduledTask","Set-ItemProperty"})Assert.DoesNotContain(forbidden,text);
    }
}
