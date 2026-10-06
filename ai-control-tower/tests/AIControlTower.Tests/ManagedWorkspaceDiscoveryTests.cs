using AIControlTower.Services;

namespace AIControlTower.Tests;

public sealed class ManagedWorkspaceDiscoveryTests
{
    [Fact]
    public void ManagedScratchPublishDoesNotBecomeAProjectButActualProjectsRemainVisible()
    {
        var root=Path.Combine(Path.GetTempPath(),"tower-managed-discovery-"+Guid.NewGuid().ToString("N"));
        try
        {
            var scratch=Path.Combine(root,"Workspace","temporary-publish");
            var install=Path.Combine(root,"Applications","VoiceStudio");
            Directory.CreateDirectory(install);File.WriteAllText(Path.Combine(install,"VoiceStudio.exe"),"isolated fixture");
            var project=Path.Combine(root,"Projects","actual-project");
            Directory.CreateDirectory(scratch);Directory.CreateDirectory(project);
            File.WriteAllText(Path.Combine(scratch,"AIControlTower.exe"),"isolated non-executable fixture");
            File.WriteAllText(Path.Combine(project,"package.json"),"{}");
            var found=new ProjectDiscoveryService().Scan(root).Projects;
            Assert.Equal(project,Assert.Single(found).Path);
            // An explicitly selected workspace is still inspectable.
            Assert.Equal(scratch,Assert.Single(new ProjectDiscoveryService().Scan(Path.Combine(root,"Workspace")).Projects).Path);
        }
        finally { if(Directory.Exists(root)) Directory.Delete(root,true); }
    }
}
