using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class JevExecutionEvidenceTests
{
    private static readonly string[] Keys=["nativeCli","jevCodexCli","jevCodexProxy","jevRouter","jevConfig","jevLog","auth"];
    private static readonly DateTimeOffset Now=new(2026,10,7,14,0,0,TimeSpan.Zero);
    private static Type Required(string name){var t=typeof(JobManager).Assembly.GetType("AIControlTower.Services."+name);Assert.NotNull(t);return t!;}
    private sealed class Fixture
    {
        public string Root {get;}=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\jev-evidence-tests",Guid.NewGuid().ToString("N"));
        public string Receipt=>Path.Combine(Root,"evidence.json");
        public Dictionary<string,string> Files {get;}=new(StringComparer.Ordinal);
        public Dictionary<string,object> Data {get;}
        public Fixture()
        {
            Directory.CreateDirectory(Root);
            foreach(var key in Keys){var p=Path.Combine(Root,key+".fixture");File.WriteAllText(p,"PRIVATE_FIXTURE_"+key);Files[key]=p;}
            var hashes=Files.ToDictionary(p=>p.Key,p=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p.Value))).ToLowerInvariant());
            Data=new(){["schemaVersion"]=1,["provider"]="installed-jev-proxy",["requestedModel"]="gpt-6.1-sol",["success"]=true,["chatgptOnly"]=true,["apiFallbackBlocked"]=true,["autoRoutingAllowed"]=false,["cliVersion"]="0.160.1",["exitCode"]=0,["timedOut"]=false,["turnCompleted"]=true,["turnFailed"]=false,["toolItemCount"]=0,["expectedAnswer"]=true,["checkedAt"]=Now.AddMinutes(-1).ToString("O"),["fingerprints"]=hashes};
            Save();
        }
        public void Save()=>File.WriteAllText(Receipt,JsonSerializer.Serialize(Data));
    }
    private static JsonElement Read(Fixture f,string? path=null)=>JsonSerializer.SerializeToElement(
        Required("JevExecutionEvidenceReader").GetMethod("Read")!.Invoke(null,[path??f.Receipt,f.Files,Now]),new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
    [Fact] public void MatchingFreshPrivateReceiptVerifiesWithoutChangingAnyBytesOrAttributes()
    {
        var f=new Fixture();var before=Directory.GetFiles(f.Root).ToDictionary(p=>p,p=>(Hash:Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))),Attrs:File.GetAttributes(p)));
        var r=Read(f);Assert.True(r.GetProperty("isVerified").GetBoolean());Assert.Equal("gpt-6.1-sol",r.GetProperty("requestedModel").GetString());
        Assert.DoesNotContain("PRIVATE_FIXTURE",r.ToString());Assert.DoesNotContain("fingerprints",r.ToString());
        Assert.Equal(before.Count,Directory.GetFiles(f.Root).Length);
        foreach(var p in before){Assert.Equal(p.Value.Hash,Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p.Key))));Assert.Equal(p.Value.Attrs,File.GetAttributes(p.Key));}
    }
    [Theory][InlineData("success",false)][InlineData("chatgptOnly",false)][InlineData("apiFallbackBlocked",false)][InlineData("autoRoutingAllowed",true)][InlineData("timedOut",true)][InlineData("turnCompleted",false)][InlineData("turnFailed",true)][InlineData("expectedAnswer",false)]
    public void InvalidExecutionEvidenceFailsClosed(string field,bool value){var f=new Fixture();f.Data[field]=value;f.Save();Assert.False(Read(f).GetProperty("isVerified").GetBoolean());}
    [Theory][InlineData("requestedModel","gpt-6-astra")][InlineData("requestedModel","jev-router")][InlineData("provider","api")][InlineData("cliVersion","0.159.0")]
    public void WrongIdentityFailsClosed(string field,string value){var f=new Fixture();f.Data[field]=value;f.Save();Assert.False(Read(f).GetProperty("isVerified").GetBoolean());}
    [Theory][InlineData(-1441)][InlineData(2)]
    public void ExpiredOrExcessivelyFutureReceiptFailsClosed(int minutes){var f=new Fixture();f.Data["checkedAt"]=Now.AddMinutes(minutes).ToString("O");f.Save();Assert.False(Read(f).GetProperty("isVerified").GetBoolean());}
    [Theory][InlineData("exitCode",1)][InlineData("toolItemCount",1)][InlineData("schemaVersion",2)]
    public void InvalidCountsFailClosed(string field,int value){var f=new Fixture();f.Data[field]=value;f.Save();Assert.False(Read(f).GetProperty("isVerified").GetBoolean());}
    [Theory][InlineData("nativeCli")][InlineData("auth")][InlineData("jevCodexProxy")]
    public void ChangedInstallationOrAccountFailsClosed(string key){var f=new Fixture();File.AppendAllText(f.Files[key],"changed");Assert.False(Read(f).GetProperty("isVerified").GetBoolean());}
    [Fact] public void MissingMalformedOversizeReceiptFailsClosedWithoutPrivateErrorDetails()
    {
        var f=new Fixture();Assert.False(Read(f,Path.Combine(f.Root,"missing.json")).GetProperty("isVerified").GetBoolean());
        File.WriteAllText(f.Receipt,"PRIVATE_MALFORMED_VALUE");var malformed=Read(f);Assert.False(malformed.GetProperty("isVerified").GetBoolean());Assert.DoesNotContain("PRIVATE_MALFORMED",malformed.ToString());
        File.WriteAllText(f.Receipt,new string('x',65537));Assert.False(Read(f).GetProperty("isVerified").GetBoolean());
    }
    [Fact] public void ReceiptCannotSelectCredentialOrInstallationPathsAndRequiresAllFingerprints()
    {
        var f=new Fixture();f.Data["fingerprints"]=new Dictionary<string,string>{{"nativeCli",f.Files["auth"]}};f.Save();Assert.False(Read(f).GetProperty("isVerified").GetBoolean());
    }
    [Fact] public void COrUnmanagedReceiptAndReparseAreRejectedBeforeRead()
    {
        var f=new Fixture();Assert.False(Read(f,@"C:\forbidden\auth.json").GetProperty("isVerified").GetBoolean());
        var link=Path.Combine(f.Root,"linked.json");File.CreateSymbolicLink(link,f.Receipt);Assert.False(Read(f,link).GetProperty("isVerified").GetBoolean());
    }
    [Fact] public void NativeLauncherIsEmbeddedAndArgumentsCannotRouteAutomatically()
    {
        var type=Required("JevNativeLauncher");Assert.Contains(typeof(JobManager).Assembly.GetManifestResourceNames(),n=>n.EndsWith("jev-native-launcher.mjs",StringComparison.Ordinal));
        var argv=JevExecutionPolicy.BuildArguments("owned.mjs","gpt-6.1-sol");Assert.DoesNotContain("exec",argv);Assert.Contains("workspace-write",argv);
        Assert.DoesNotContain("--dangerously-bypass-hook-trust",argv);Assert.DoesNotContain("jev-router",argv);
    }
    [Fact] public void MainJevBoundaryRequiresVerifiedReceiptBeforeAnyLauncherExtractionOrExecution()
    {
        var dir=new DirectoryInfo(AppContext.BaseDirectory);while(dir is not null&&!File.Exists(Path.Combine(dir.FullName,"src","AIControlTower","ViewModels","MainViewModel.cs")))dir=dir.Parent;
        Assert.NotNull(dir);var body=File.ReadAllText(Path.Combine(dir!.FullName,"src","AIControlTower","ViewModels","MainViewModel.cs"));var start=body.IndexOf("public async void RunJevTask()",StringComparison.Ordinal);var end=body.IndexOf("public Task CancelJevTaskAsync()",start,StringComparison.Ordinal);var method=body[start..end];
        var evidence=method.IndexOf("JevExecutionEvidenceReader.ReadDefault",StringComparison.Ordinal);Assert.True(evidence>=0);
        Assert.True(evidence<method.IndexOf("JevNativeLauncher.Prepare",StringComparison.Ordinal));Assert.DoesNotContain("TryCreate(null,false",method);Assert.DoesNotContain("true,out",method);
        Assert.Contains("evidence.IsVerified",method);Assert.Contains("evidence.RequestedModel",method);Assert.DoesNotContain("jev-codex.mjs",method);
    }
    [Fact] public void ManualSessionEnableIsTransientAndRequiresExplicitVerifiedPanePreparation()
    {
        var policy=typeof(JevExecutionPolicy);var method=policy.GetMethod("ResolveSessionEnable");Assert.NotNull(method);
        Assert.False((bool)method!.Invoke(null,[false,true,null])!);Assert.False((bool)method.Invoke(null,[true,true,null])!);Assert.True((bool)method.Invoke(null,[false,true,true])!);
        Assert.False((bool)method.Invoke(null,[true,true,false])!);Assert.False((bool)method.Invoke(null,[false,false,true])!);
    }
    [Fact] public void JevPanePreparationIsExplicitAndDoesNotPutPrivateAuthReadsIntoConstructorOrGetter()
    {
        var vm=typeof(AIControlTower.ViewModels.MainViewModel);Assert.NotNull(vm.GetMethod("PrepareJevSession"));
        var dir=new DirectoryInfo(AppContext.BaseDirectory);while(dir is not null&&!File.Exists(Path.Combine(dir.FullName,"src","AIControlTower","ViewModels","MainViewModel.cs")))dir=dir.Parent;
        Assert.NotNull(dir);var source=File.ReadAllText(Path.Combine(dir!.FullName,"src","AIControlTower","ViewModels","MainViewModel.cs"));
        var getter=source[source.IndexOf("public bool EnableJev",StringComparison.Ordinal)..source.IndexOf("public bool ReduceMotion",StringComparison.Ordinal)];
        Assert.DoesNotContain("ReadDefault",getter);Assert.Contains("_jevSessionEnabled",getter);Assert.Contains("JevExecutionPolicy.ResolveSessionEnable",getter);
    }}
