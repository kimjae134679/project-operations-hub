using System.Reflection;
using System.Xml.Linq;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;
public sealed class ProjectProgressIntegrationTests
{
    [Fact] public void RefreshPreservesSelectedProjectProgramAndRecordWithoutInventingOverallProgress()
    {
        Exception? error=null;var thread=new Thread(()=>{try
        {
            var root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks","progress-vm-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
            try
            {
                var settings=new ControlTowerSettings{RootPath=root,CommunicationHubPath=root,TransientDataDirectory=Path.Combine(root,"data"),TransientReadOnly=true,IsTemporary=true};
                using var vm=new MainViewModel(settings,false,()=>throw new Exception("no recovery"));
                var p=new ProjectItem{Id="p",Path=root,DisplayName="테스트 프로젝트",Functions=[new FunctionItem{Programs=[new ProgramItem{Id="p/one",ProjectId="p"}]}]};
                vm.Projects.Add(p);vm.Projects.Add(new(){Id="other",Path=root,DisplayName="다른 프로젝트"});vm.SelectedProject=p;
                var records=new[]{new WorkActivity{Id="older",ProjectId="p",Source="명령·답변 task_exchange 기록",Revision=1,UpdatedAt=DateTimeOffset.UtcNow.AddMinutes(-1)},new WorkActivity{Id="newer",ProjectId="p",Source="명령·답변 task_exchange 기록",Revision=2,UpdatedAt=DateTimeOffset.UtcNow,CommandSummary="실제 명령",ResponseSummary="이미 전달한 답변",RecentLog="실제 한 일",NextCheckpoint="남은 일 · 검증 기록",Status="completed"}};
                vm.WorkDashboard.ApplySnapshot(records);vm.WorkDashboard.SelectedRecord=records[0];
                var program=vm.SelectedProgram;
                var refresh=typeof(MainViewModel).GetMethod("RefreshProjectProgressAsync");Assert.NotNull(refresh);((Task)refresh!.Invoke(vm,[])!).GetAwaiter().GetResult();
                Assert.Same(p,vm.SelectedProject);Assert.Same(program,vm.SelectedProgram);Assert.Equal("older",vm.WorkDashboard.SelectedRecord?.Id);
                var progress=typeof(MainViewModel).GetProperty("SelectedProjectProgress");Assert.NotNull(progress);var observation=progress!.GetValue(vm)!;
                Assert.False((bool)observation.GetType().GetProperty("IsKnown")!.GetValue(observation)!);
                var handoff=typeof(MainViewModel).GetProperty("SelectedProjectHandoff");Assert.NotNull(handoff);Assert.Equal("newer",((WorkActivity)handoff!.GetValue(vm)!).Id);
                var actual=(WorkActivity)handoff.GetValue(vm)!;
                Assert.Equal("이미 전달한 답변",actual.ResponseSummary);Assert.Equal("실제 한 일",actual.RecentLog);Assert.Equal("남은 일 · 검증 기록",actual.NextCheckpoint);
                Assert.Empty(Directory.GetFiles(root,"*",SearchOption.AllDirectories));
            }
            finally{Directory.Delete(root,true);}
        }
        catch(Exception e){error=e;}});thread.SetApartmentState(ApartmentState.STA);thread.Start();Assert.True(thread.Join(TimeSpan.FromSeconds(15)));if(error is not null)System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
    }
    [Fact] public void ProgressCardUsesExistingProjectScopeThemeAndRealHandoffBindingsNotControls()
    {
        var path=@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\source\ai-control-tower\src\AIControlTower\MainWindow.xaml";var doc=XDocument.Load(path);XNamespace x="http://schemas.microsoft.com/winfx/2006/xaml";
        var card=doc.Descendants().SingleOrDefault(e=>(string?)e.Attribute(x+"Name")=="ProjectProgressCard");Assert.NotNull(card);
        var body=card!.ToString();Assert.Contains("SelectedProjectProgress",body);Assert.Contains("Headline",body);Assert.Contains("SourceTimeText",body);Assert.Contains("FreshnessText",body);Assert.Contains("NextCheckpoint",body);Assert.Contains("ResponseSummary",body);Assert.Contains("RecentLog",body);Assert.Contains("DynamicResource",body);
        Assert.DoesNotContain("Command=",body);Assert.DoesNotContain("Click=",body);
        var list=doc.Descendants().Single(e=>(string?)e.Attribute(x+"Name")=="ProjectList").ToString();Assert.DoesNotContain("Percent",list);Assert.DoesNotContain("Progress",list);
    }
}
