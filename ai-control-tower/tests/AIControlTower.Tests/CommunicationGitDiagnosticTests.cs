using System.Diagnostics;
using System.Reflection;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class CommunicationGitDiagnosticTests : IDisposable
{
    private const string Status="04_COMMUNICATION/announcements/STATUS.md";
    private readonly string _root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks","git-diagnostic-tests-"+Guid.NewGuid().ToString("N"));
    public CommunicationGitDiagnosticTests()=>Directory.CreateDirectory(_root);
    private string StatusFile { get { var file=Path.Combine(_root,Status);Directory.CreateDirectory(Path.GetDirectoryName(file)!);return file; } }
    private static T? Value<T>(object result,string property)=>(T?)result.GetType().GetProperty(property)?.GetValue(result);
    private void Marker(string origin) { Directory.CreateDirectory(Path.Combine(_root,".git"));File.WriteAllText(Path.Combine(_root,".git","control-tower-communication-mirror"),origin); }
    private static CommunicationGitService WithRunner(string origin,Func<string?,CancellationToken,string[],Task<(int Code,string Output,string Error)>> run)
    {
        var ctor=typeof(CommunicationGitService).GetConstructors().SingleOrDefault(c=>c.GetParameters().Length==2);
        Assert.NotNull(ctor);return (CommunicationGitService)ctor.Invoke([origin,run]);
    }
    [Fact] public async Task RealIndexLockFailurePreservesDiagnosticHeadIndexLockAndStopsBeforeAnyFollowupGit()
    {
        await Command(_root,"init","-b","main");await Command(_root,"config","user.name","Diagnostic Fixture");await Command(_root,"config","user.email","test@example.invalid");
        File.WriteAllText(StatusFile,"original");await Command(_root,"add","--",Status);await Command(_root,"commit","-m","fixture");
        var origin=Path.Combine(_root,"not-contacted-origin.git");await Command(_root,"remote","add","origin",origin);Marker(origin);
        var beforeHead=(await Command(_root,"rev-parse","HEAD")).Output;var index=Path.Combine(_root,".git","index");var beforeIndex=File.ReadAllBytes(index);
        var lockPath=index+".lock";File.WriteAllText(lockPath,"test-owned-lock");var beforeLock=File.ReadAllBytes(lockPath);File.WriteAllText(StatusFile,"updated");
        var calls=new List<string>();
        var service=WithRunner(origin,async(root,ct,args)=>
        {
            calls.Add(args[0]);
            var real=typeof(CommunicationGitService).GetMethod("ExecuteGit",BindingFlags.Static|BindingFlags.NonPublic);Assert.NotNull(real);
            return await (Task<(int Code,string Output,string Error)>)real.Invoke(null,[root,ct,args])!;
        });
        var result=await service.SynchronizeAsync(_root,[Status],true);
        Assert.False(result.Success);Assert.False(result.Published);Assert.Equal("add",Value<string>(result,"FailureStage"));Assert.Equal(128,Value<int?>(result,"ExitCode"));
        Assert.Contains("index.lock",Value<string>(result,"Diagnostic")!);Assert.Contains("exit 128",result.Message);
        Assert.DoesNotContain(_root,result.Message,StringComparison.OrdinalIgnoreCase);Assert.DoesNotContain("test-owned-lock",result.Message);
        Assert.Equal(new[]{"remote","remote","status","add"},calls);
        Assert.Equal(beforeHead,(await Command(_root,"rev-parse","HEAD")).Output);Assert.Equal(beforeIndex,File.ReadAllBytes(index));Assert.Equal(beforeLock,File.ReadAllBytes(lockPath));
    }
    [Fact] public async Task StderrSecretsPathsAndPromptAreMaskedAndDiagnosticIsBounded()
    {
        const string origin="fixture-origin";Marker(origin);File.WriteAllText(StatusFile,"updated");
        var secret="sk-secretSECRET123456789";var stderr="fatal: Unable to create 'C:\\Users\\PRIVATE_USER\\project\\.git\\index.lock': File exists\nhttps://USERNAME:PASSVALUE@example.invalid/path?token=QUERY_SECRET\nAuthorization: Bearer BEARER_SECRET\napi_key="+secret+"\nprompt: PRIVATE_TASK_INSTRUCTIONS\nuser PRIVATE_EMAIL@example.invalid\n"+new string('x',5000);
        var service=WithRunner(origin,(root,ct,args)=>Task.FromResult(args[0] switch
        { "remote"=>(0,origin+"\n",""), "status"=>(0," M "+Status+"\0",""), _=>(128,"PRIVATE_STDOUT_NOT_DIAGNOSTIC",stderr) }));
        var result=await service.SynchronizeAsync(_root,[Status],true);var diagnostic=Value<string>(result,"Diagnostic")!;
        Assert.Equal("add",Value<string>(result,"FailureStage"));Assert.True(diagnostic.Length<=1024);Assert.Contains("File exists",diagnostic);
        foreach(var value in new[]{"PRIVATE_USER","USERNAME","PASSVALUE","QUERY_SECRET","BEARER_SECRET",secret,"PRIVATE_TASK_INSTRUCTIONS","PRIVATE_EMAIL","PRIVATE_STDOUT_NOT_DIAGNOSTIC"}) Assert.DoesNotContain(value,result.Message);
    }
    [Fact] public async Task SuccessfulNulPorcelainStdoutIsNotMixedWithWarningStderr()
    {
        const string origin="fixture-origin";Marker(origin);File.WriteAllText(StatusFile,"updated");var calls=new List<string>();
        var service=WithRunner(origin,(root,ct,args)=>
        {
            calls.Add(args[0]);return Task.FromResult(args[0] switch
            { "remote"=>(0,origin+"\n","warning unrelated remote metadata"), "status"=>(0," M "+Status+"\0","warning: must not become a porcelain row\0"), "rev-list"=>(0,"0\n",""), _=>(0,"","warning: no parser contamination") });
        });
        var result=await service.SynchronizeAsync(_root,[Status],false);
        Assert.True(result.Success,result.Message);Assert.Equal(new[]{"remote","remote","status","add","commit","fetch","rebase","rev-list"},calls);
        Assert.Null(Value<int?>(result,"ExitCode"));Assert.True(string.IsNullOrEmpty(Value<string>(result,"Diagnostic")));
    }
    [Theory] [InlineData("status")] [InlineData("fetch")] [InlineData("commit")]
    public async Task FailureStageAndExitCodeArePreservedWithoutLeakingStdout(string stage)
    {
        const string origin="fixture-origin";Marker(origin);File.WriteAllText(StatusFile,"updated");
        var service=WithRunner(origin,(root,ct,args)=>Task.FromResult(args[0]==stage?(23,"stdout-secret","fatal: fixture failure"):args[0] switch
        {"remote"=>(0,origin+"\n",""),"status"=>(0," M "+Status+"\0",""),_=>(0,"","")}));
        var result=await service.SynchronizeAsync(_root,[Status],false);
        Assert.False(result.Success);Assert.Equal(stage,Value<string>(result,"FailureStage"));Assert.Equal(23,Value<int?>(result,"ExitCode"));Assert.Contains("fixture failure",result.Message);Assert.DoesNotContain("stdout-secret",result.Message);
    }
    [Fact] public async Task MultilinePromptFooterBodiesAreHeldAndStatusBriefContainsTheSameSafeDiagnostic()
    {
        const string origin="fixture-origin";Marker(origin);File.WriteAllText(StatusFile,"updated");
        var service=WithRunner(origin,(root,ct,args)=>Task.FromResult(args[0] switch
        {"remote"=>(0,origin+"\n",""),"status"=>(0," M "+Status+"\0",""),_=>(128,"","prompt:\nPRIVATE_MULTILINE_TASK\nPRIVATE_PROMPT_CONTINUATION\nfatal: index.lock exists\nfooter:\nPRIVATE_FOOTER_CONTENT")}));
        var result=await service.SynchronizeAsync(_root,[Status],false);
        foreach(var value in new[]{"PRIVATE_MULTILINE_TASK","PRIVATE_PROMPT_CONTINUATION","PRIVATE_FOOTER_CONTENT"})Assert.DoesNotContain(value,result.Message);
        Assert.Contains("index.lock exists",result.Message);Assert.Equal(result.Message,Value<string>(result,"StatusBrief"));
    }
    private static async Task<(int Code,string Output,string Error)> Command(string root,params string[] args)
    {
        using var p=new Process{StartInfo=new ProcessStartInfo("git"){WorkingDirectory=root,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true}};
        p.StartInfo.ArgumentList.Add("-c");p.StartInfo.ArgumentList.Add("core.hooksPath="+Path.Combine(root,"fixture-no-hooks"));foreach(var arg in args)p.StartInfo.ArgumentList.Add(arg);
        p.Start();var output=p.StandardOutput.ReadToEndAsync();var error=p.StandardError.ReadToEndAsync();await p.WaitForExitAsync();Assert.Equal(0,p.ExitCode);return(p.ExitCode,await output,await error);
    }
    public void Dispose()
    {
        var checks=Path.GetFullPath(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks")+Path.DirectorySeparatorChar;var target=Path.GetFullPath(_root);
        if(!target.StartsWith(checks,StringComparison.OrdinalIgnoreCase)||!Path.GetFileName(target).StartsWith("git-diagnostic-tests-",StringComparison.Ordinal))throw new InvalidOperationException("Unexpected fixture cleanup root.");
        if(Directory.Exists(target)){foreach(var file in Directory.EnumerateFiles(target,"*",SearchOption.AllDirectories))File.SetAttributes(file,FileAttributes.Normal);Directory.Delete(target,true);}
    }
}
