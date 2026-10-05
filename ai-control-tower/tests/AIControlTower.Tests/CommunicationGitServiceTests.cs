using System.Diagnostics;
using AIControlTower.Services;
namespace AIControlTower.Tests;
public sealed class CommunicationGitServiceTests
{
    [Theory]
    [InlineData("../secrets.txt")]
    [InlineData("AGENTS.md")]
    [InlineData("04_COMMUNICATION/announcements/manifest.json")]
    [InlineData("04_COMMUNICATION/project-inbox/P/../../AGENTS.md")]
    [InlineData("04_COMMUNICATION/project-inbox/P/code.exe")]
    public void OnlyGeneratedCommunicationPathsCanPublish(string path) => Assert.False(CommunicationGitService.IsPublishablePath(path));
    [Fact]
    public async Task DedicatedMirrorRefusesForeignDirtyFileThenPublishesOwnRecord()
    {
        var root=Path.Combine(Path.GetTempPath(),"tower-git-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            var seed=Path.Combine(root,"seed");Directory.CreateDirectory(seed);
            await Git(seed,"init","-b","main"); await Git(seed,"config","user.name","Test");await Git(seed,"config","user.email","test@example.invalid");
            File.WriteAllText(Path.Combine(seed,"README.md"),"seed");await Git(seed,"add","README.md");await Git(seed,"commit","-m","seed");
            var origin=Path.Combine(root,"origin.git");await Git(root,"clone","--bare",seed,origin);
            var mirror=Path.Combine(root,"mirror");var service=new CommunicationGitService(origin);
            Assert.True((await service.PrepareAsync(mirror)).Success);
            var receipt="04_COMMUNICATION/announcements/receipts/N-1/r1/P/read.json";
            var path=Path.Combine(mirror,receipt);Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,"{}");
            File.WriteAllText(Path.Combine(mirror,"private.txt"),"do not publish");
            Assert.False((await service.SynchronizeAsync(mirror,[receipt],true)).Success);
            Assert.Equal("",(await Git(mirror,"diff","--cached","--name-only")).Trim());
            File.Delete(Path.Combine(mirror,"private.txt"));
            var result=await service.SynchronizeAsync(mirror,[receipt],true);
            Assert.True(result.Success,result.Message);Assert.True(result.Published);
            Assert.Equal("",(await Git(mirror,"status","--porcelain")).Trim());
            Assert.Contains(receipt,(await Git(mirror,"ls-tree","-r","--name-only","origin/main")));
        }
        finally { if(Directory.Exists(root)) { foreach(var file in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories)) File.SetAttributes(file,FileAttributes.Normal); Directory.Delete(root,true); } }
    }
    [Fact]
    public async Task OutgoingHistoryAndRedirectedPushAreRejectedAndUploadOffStillPulls()
    {
        var root=Path.Combine(Path.GetTempPath(),"tower-git-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        try
        {
            var seed=Path.Combine(root,"seed");Directory.CreateDirectory(seed);
            await Git(seed,"init","-b","main");await Git(seed,"config","user.name","Test");await Git(seed,"config","user.email","test@example.invalid");
            File.WriteAllText(Path.Combine(seed,"README.md"),"seed");await Git(seed,"add","README.md");await Git(seed,"commit","-m","seed");
            var origin=Path.Combine(root,"origin.git");await Git(root,"clone","--bare",seed,origin);
            var mirror=Path.Combine(root,"mirror");var service=new CommunicationGitService(origin);Assert.True((await service.PrepareAsync(mirror)).Success);
            await Git(mirror,"config","remote.origin.pushurl",Path.Combine(root,"wrong.git"));
            Assert.False((await service.SynchronizeAsync(mirror,[],true)).Success);
            await Git(mirror,"config","--unset","remote.origin.pushurl");
            var status="04_COMMUNICATION/announcements/STATUS.md";var file=Path.Combine(mirror,status);Directory.CreateDirectory(Path.GetDirectoryName(file)!);File.WriteAllText(file,"local record");
            await Git(seed,"remote","add","origin",origin);File.WriteAllText(Path.Combine(seed,"NEW.md"),"latest notice");await Git(seed,"add","NEW.md");await Git(seed,"commit","-m","latest");await Git(seed,"push","origin","main");
            var noUpload=await service.SynchronizeAsync(mirror,[status],false);Assert.True(noUpload.Success,noUpload.Message);Assert.False(noUpload.Published);Assert.True(File.Exists(Path.Combine(mirror,"NEW.md")));
            File.WriteAllText(Path.Combine(mirror,"outside.txt"),"do not publish");await Git(mirror,"add","outside.txt");await Git(mirror,"commit","-m","outside added");File.Delete(Path.Combine(mirror,"outside.txt"));await Git(mirror,"add","outside.txt");await Git(mirror,"commit","-m","outside reverted");
            Assert.False((await service.SynchronizeAsync(mirror,[status],true)).Success);
        }
        finally { if(Directory.Exists(root)) { foreach(var file in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories))File.SetAttributes(file,FileAttributes.Normal);Directory.Delete(root,true); } }
    }
    private static async Task<string> Git(string directory,params string[] arguments)
    {
        using var p=new Process{StartInfo=new ProcessStartInfo("git"){WorkingDirectory=directory,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true}};
        foreach(var a in arguments)p.StartInfo.ArgumentList.Add(a);p.Start();var output=p.StandardOutput.ReadToEndAsync();var error=p.StandardError.ReadToEndAsync();await p.WaitForExitAsync();Assert.Equal(0,p.ExitCode);await error;return await output;
    }
}
