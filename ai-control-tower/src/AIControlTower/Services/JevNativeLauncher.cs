using System.Reflection;
using System.Security.Cryptography;

namespace AIControlTower.Services;

public static class JevNativeLauncher
{
    public static string Prepare()=>PrepareIn(@"D:\A_KJ\AI\ControlTowerData\launchers");
    public static string PrepareIn(string directory)
    {
        if(!JevExecutionEvidenceReader.IsManagedPath(directory))throw new InvalidOperationException("Jev 런처 저장 위치를 확인할 수 없습니다.");
        var assembly=typeof(JevNativeLauncher).Assembly;
        var name=assembly.GetManifestResourceNames().Single(n=>n.EndsWith("jev-native-launcher.mjs",StringComparison.Ordinal));
        using var source=assembly.GetManifestResourceStream(name)!;using var buffer=new MemoryStream();source.CopyTo(buffer);
        if(buffer.Length is <=0 or >65536)throw new InvalidOperationException("Jev 런처 검증 실패");
        var bytes=buffer.ToArray();var digest=Convert.ToHexString(SHA256.HashData(bytes));
        Directory.CreateDirectory(directory);
        var path=Path.Combine(directory,"jev-native-"+digest[..16].ToLowerInvariant()+".mjs");
        if(!JevExecutionEvidenceReader.IsManagedPath(path))throw new InvalidOperationException("Jev 런처 위치 검증 실패");
        if(File.Exists(path))
        {
            if(!Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).Equals(digest,StringComparison.Ordinal))throw new InvalidOperationException("기존 Jev 런처가 달라 실행하지 않습니다.");
        }
        else using(var output=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None))output.Write(bytes);
        return path;
    }
}
