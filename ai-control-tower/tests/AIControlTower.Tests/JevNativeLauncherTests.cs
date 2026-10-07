using System.Diagnostics;
using System.Text.Json;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class JevNativeLauncherTests
{
    [Fact] public async Task InstalledProxyLauncherUsesHiddenNativeSpawnAndClosesOnlyOwnedProxyWithoutModelCall()
    {
        var assembly=typeof(JobManager).Assembly;var resource=assembly.GetManifestResourceNames().SingleOrDefault(n=>n.EndsWith("jev-native-launcher.mjs",StringComparison.Ordinal));Assert.NotNull(resource);
        var root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\jev-native-launcher-tests",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        using(var src=assembly.GetManifestResourceStream(resource!)!)using(var dst=File.Create(Path.Combine(root,"launcher.mjs")))await src.CopyToAsync(dst);
        var script="""
        import {runJevTask} from './launcher.mjs';
        import {EventEmitter} from 'node:events';
        import {PassThrough} from 'node:stream';
        let closed=false,captured=null,output='',input='';
        const deps={env:{JEV_API_KEY:'FIXTURE_ONLY'},cwd:process.cwd(),nativeCli:'OWNED_FAKE_NATIVE.exe',
          loadEnv(){},codexArgs(url,args){return ['-c','model_provider="jev"','-c','model_providers.jev.name="Jev Router"',...args]},
          async startCodexProxy(){return {port:9,close(){closed=true}}},
          stdout:{write(x){output+=x}},stderr:{write(){}},input:new PassThrough(),
          spawn(file,args,options){captured={file,args,options:{windowsHide:options.windowsHide,shell:options.shell,stdio:options.stdio}};
            const child=new EventEmitter();child.stdout=new PassThrough();child.stderr=new PassThrough();child.stdin=new PassThrough();child.stdin.on('data',b=>input+=b.toString());
            queueMicrotask(()=>{child.stdout.end('{"type":"turn.completed"}\n');child.stderr.end();child.emit('exit',0);child.emit('close',0)});return child;
          }};
        deps.input.end('FIXTURE_TASK');
        const code=await runJevTask(['--model','gpt-6.1-sol','--json','--sandbox','workspace-write','-'],deps);
        console.log(JSON.stringify({code,closed,captured,output,input}));
        process.exit(code);
        """;
        var fixture=Path.Combine(root,"fake-runtime.mjs");File.WriteAllText(fixture,script);
        var node=EnvironmentProbe.FindCommand("node.exe");Assert.NotNull(node);
        var info=new ProcessStartInfo(node!){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=root,RedirectStandardOutput=true,RedirectStandardError=true};info.ArgumentList.Add(fixture);
        using var process=Process.Start(info)!;var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try{await process.WaitForExitAsync(timeout.Token);}catch(OperationCanceledException){process.Kill(true);throw;}
        var outText=await stdout;var errText=await stderr;Assert.Equal(0,process.ExitCode);Assert.True(string.IsNullOrEmpty(errText),errText);
        using var json=JsonDocument.Parse(outText);var result=json.RootElement;Assert.True(result.GetProperty("closed").GetBoolean());Assert.Equal(0,result.GetProperty("code").GetInt32());
        var captured=result.GetProperty("captured");Assert.Equal("OWNED_FAKE_NATIVE.exe",captured.GetProperty("file").GetString());Assert.True(captured.GetProperty("options").GetProperty("windowsHide").GetBoolean());Assert.False(captured.GetProperty("options").GetProperty("shell").GetBoolean());
        var args=captured.GetProperty("args").EnumerateArray().Select(a=>a.GetString()).ToArray();var exec=Array.IndexOf(args,"exec");Assert.True(exec>0);
        Assert.DoesNotContain(args.Skip(exec+1),a=>a is "-c" or "--disable");Assert.Contains("gpt-6.1-sol",args);Assert.Contains("--ignore-user-config",args);Assert.DoesNotContain(args,a=>a?.Contains("tools.view_image")==true);
        Assert.Equal("FIXTURE_TASK",result.GetProperty("input").GetString());Assert.Contains("turn.completed",result.GetProperty("output").GetString());
        Assert.Contains("approval_policy=\"never\"",args);Assert.Contains("model_providers.jev.request_max_retries=0",args);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task NativeCloseDrainsLateTerminalOutputAndSpawnErrorDoesNotWaitForMissingCloseOrInput(bool spawnError)
    {
        var assembly=typeof(JobManager).Assembly;var resource=assembly.GetManifestResourceNames().Single(n=>n.EndsWith("jev-native-launcher.mjs",StringComparison.Ordinal));
        var root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\jev-native-lifecycle-tests",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        using(var source=assembly.GetManifestResourceStream(resource)!)using(var destination=File.Create(Path.Combine(root,"launcher.mjs")))await source.CopyToAsync(destination);
        var script="""
        import {runJevTask} from './launcher.mjs';
        import {EventEmitter} from 'node:events';
        import {PassThrough,Writable} from 'node:stream';
        const fail=__SPAWN_ERROR__;let output='',closed=false;
        const deps={env:{JEV_API_KEY:'FIXTURE_ONLY'},cwd:process.cwd(),nativeCli:'FAKE.exe',loadEnv(){},codexArgs(url,args){return args},
          async startCodexProxy(){return {port:9,close(){closed=true}}},stdout:{write(b){output+=b}},stderr:{write(){}},input:new PassThrough(),
          spawn(){const child=new EventEmitter();child.stdout=new PassThrough();child.stderr=new PassThrough();
            child.stdin=fail?new Writable({write(chunk,encoding,done){/* no finish: failed spawn */}}):new PassThrough();
            child.stdin.on('data',()=>{});
            if(fail)queueMicrotask(()=>child.emit('error',new Error('PRIVATE_SPAWN_ERROR')));
            else child.stdin.once('finish',()=>{child.emit('exit',0);setTimeout(()=>{child.stdout.end('{"type":"turn.completed"}\n');child.stderr.end();child.emit('close',0)},35)});
            return child;}};
        deps.input.end('FIXTURE_TASK');
        const code=await runJevTask(['--model','gpt-6.1-sol','--json','--sandbox','workspace-write','-'],deps);
        console.log(JSON.stringify({code,closed,terminalSeen:output.includes('turn.completed')}));process.exit(0);
        """;
        script=script.Replace("__SPAWN_ERROR__",spawnError?"true":"false",StringComparison.Ordinal);
        var fixture=Path.Combine(root,"lifecycle.mjs");File.WriteAllText(fixture,script);
        var info=new ProcessStartInfo(EnvironmentProbe.FindCommand("node.exe")!){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=root,RedirectStandardOutput=true,RedirectStandardError=true};info.ArgumentList.Add(fixture);
        using var process=Process.Start(info)!;var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();
        var completion=process.WaitForExitAsync();var timely=await Task.WhenAny(completion,Task.Delay(TimeSpan.FromSeconds(3)))==completion;
        if(!timely){process.Kill(true);await process.WaitForExitAsync();}
        Assert.True(timely,"Owned launcher hung waiting for nonexistent close/stdin after spawn error.");
        Assert.Equal(0,process.ExitCode);Assert.True(string.IsNullOrEmpty(await stderr));
        using var result=JsonDocument.Parse(await stdout);Assert.True(result.RootElement.GetProperty("closed").GetBoolean());
        Assert.Equal(spawnError?1:0,result.RootElement.GetProperty("code").GetInt32());
        Assert.Equal(!spawnError,result.RootElement.GetProperty("terminalSeen").GetBoolean());
    }}
