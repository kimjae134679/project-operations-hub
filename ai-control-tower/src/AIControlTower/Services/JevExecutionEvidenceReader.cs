using System.Security.Cryptography;
using System.Text.Json;

namespace AIControlTower.Services;

public sealed record JevExecutionEvidence(bool IsVerified,string? RequestedModel,string Reason);

/// <summary>Private local execution evidence; no auth contents, paths or hashes are returned to UI/logs.</summary>
public static class JevExecutionEvidenceReader
{
    public const string DefaultReceiptPath=@"D:\A_KJ\AI\ControlTowerData\ai-evidence\jev-gpt-6.1-sol.json";
    private const string ManagedRoot=@"D:\A_KJ\AI";
    private static readonly string[] Keys=["nativeCli","jevCodexCli","jevCodexProxy","jevRouter","jevConfig","jevLog","auth"];
    private static JevExecutionEvidence Denied()=>new(false,null,"지원 검증이 없거나 만료됐습니다. 설치·인증 변경도 다시 확인해야 합니다.");
    public static JevExecutionEvidence ReadDefault()=>Read(DefaultReceiptPath,InstalledFiles(),DateTimeOffset.UtcNow);
    internal static IReadOnlyDictionary<string,string> InstalledFiles()
    {
        var npm=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"npm","node_modules");
        var jev=Path.Combine(npm,"jev-router","src");
        return new Dictionary<string,string>(StringComparer.Ordinal)
        {
            ["nativeCli"]=Path.Combine(npm,"@openai","codex","node_modules","@openai","codex-win32-x64","vendor","x86_64-pc-windows-msvc","bin","codex.exe"),
            ["jevCodexCli"]=Path.Combine(jev,"codex-cli.mjs"),["jevCodexProxy"]=Path.Combine(jev,"codex-proxy.mjs"),
            ["jevRouter"]=Path.Combine(jev,"router.mjs"),["jevConfig"]=Path.Combine(jev,"config.mjs"),["jevLog"]=Path.Combine(jev,"log.mjs"),
            ["auth"]=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".codex","auth.json")
        };
    }
    public static JevExecutionEvidence Read(string receiptPath,IReadOnlyDictionary<string,string> installedFiles,DateTimeOffset now)
    {
        try
        {
            if(!IsManagedPath(receiptPath)||!File.Exists(receiptPath)||installedFiles.Count!=Keys.Length||Keys.Any(k=>!installedFiles.ContainsKey(k)))return Denied();
            using var stream=new FileStream(receiptPath,FileMode.Open,FileAccess.Read,FileShare.Read);
            if(stream.Length is <=0 or >65536)return Denied();
            using var document=JsonDocument.Parse(stream,new JsonDocumentOptions{MaxDepth=16});var root=document.RootElement;
            if(root.ValueKind!=JsonValueKind.Object||!Int(root,"schemaVersion",1)||!Text(root,"provider","installed-jev-proxy")||!Text(root,"requestedModel","gpt-6.1-sol")||!Text(root,"cliVersion","0.160.1"))return Denied();
            foreach(var key in new[]{"success","chatgptOnly","apiFallbackBlocked","turnCompleted","expectedAnswer"})if(!Flag(root,key,true))return Denied();
            foreach(var key in new[]{"autoRoutingAllowed","timedOut","turnFailed"})if(!Flag(root,key,false))return Denied();
            if(!Int(root,"exitCode",0)||!Int(root,"toolItemCount",0))return Denied();
            if(!root.TryGetProperty("checkedAt",out var stamp)||stamp.ValueKind!=JsonValueKind.String||!DateTimeOffset.TryParse(stamp.GetString(),out var checkedAt)||checkedAt.Offset!=TimeSpan.Zero||checkedAt>now.AddMinutes(1)||now-checkedAt>TimeSpan.FromHours(24))return Denied();
            if(!root.TryGetProperty("fingerprints",out var fingerprints)||fingerprints.ValueKind!=JsonValueKind.Object||fingerprints.EnumerateObject().Count()!=Keys.Length)return Denied();
            foreach(var key in Keys)
            {
                if(!fingerprints.TryGetProperty(key,out var field)||field.ValueKind!=JsonValueKind.String)return Denied();
                var expected=field.GetString();if(expected is null||expected.Length!=64||expected.Any(c=>!Uri.IsHexDigit(c)))return Denied();
                var file=installedFiles[key];if(!Path.IsPathFullyQualified(file)||!NoReparse(file)||!File.Exists(file))return Denied();
                using var input=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.Read);
                if(input.Length<=0||input.Length>512L*1024*1024)return Denied();
                var actual=Convert.ToHexString(SHA256.HashData(input));if(!actual.Equals(expected,StringComparison.OrdinalIgnoreCase))return Denied();
            }
            return new(true,"gpt-6.1-sol","");
        }
        catch{return Denied();}
    }
    private static bool Flag(JsonElement e,string key,bool value)=>e.TryGetProperty(key,out var f)&&f.ValueKind==(value?JsonValueKind.True:JsonValueKind.False);
    private static bool Int(JsonElement e,string key,int value)=>e.TryGetProperty(key,out var f)&&f.TryGetInt32(out var n)&&n==value;
    private static bool Text(JsonElement e,string key,string value)=>e.TryGetProperty(key,out var f)&&f.ValueKind==JsonValueKind.String&&f.GetString()==value;
    internal static bool IsManagedPath(string path)
    {
        if(!Path.IsPathFullyQualified(path)||path.StartsWith(@"\\",StringComparison.Ordinal))return false;
        var full=Path.GetFullPath(path);var root=Path.GetFullPath(ManagedRoot).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
        return full.StartsWith(root,StringComparison.OrdinalIgnoreCase)&&!full.Split(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar).Any(p=>p.Equals("Desktop",StringComparison.OrdinalIgnoreCase))&&NoReparse(full);
    }
    internal static bool NoReparse(string path)
    {
        for(string? current=Path.GetFullPath(path);current is not null;current=Path.GetDirectoryName(current))
            if((File.Exists(current)||Directory.Exists(current))&&(File.GetAttributes(current)&FileAttributes.ReparsePoint)!=0)return false;
        return true;
    }
}
