using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Markup;
using System.Xml.Linq;
using AIControlTower;
using AIControlTower.Models;
using AIControlTower.Services;
using AIControlTower.ViewModels;
internal static class Program
{
 [STAThread] static int Main(string[] args)
 {
  var output=Path.GetFullPath(args[0]);
  if(!output.StartsWith(@"D:\A_KJ\AI\Workspace\ControlTower\ui-actions-20261008\",StringComparison.OrdinalIgnoreCase))return 2;
  Directory.CreateDirectory(output);var app=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};app.Resources=Resources();var code=0;var reports=new List<object>();
  SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
  Dispatcher.CurrentDispatcher.BeginInvoke(async()=>
  {
   try
   {
    foreach(var rootPath in new[]{@"C:\KJ\Github\Threads",@"D:\AI\VoiceAudiobook"})
    {
     var window=new MainWindow(new ControlTowerSettings{RootPath=rootPath,CommunicationHubPath=@"D:\A_KJ\AI\Projects\project-operations-hub",TransientReadOnly=true,IsTemporary=true,ReduceMotion=true,AutoCommunication=false,AutoPublishCommunication=false},new StartupPolicy(true));
     var vm=(MainViewModel)window.DataContext;
     try
     {
      Check(vm.IsReadOnlyView&&!vm.CanRunRegisteredTasks&&!vm.CanReadLiveStatus,"read-only admission");
      await window.InitializeAsync();
      var project=vm.Projects.Single(p=>Path.GetFullPath(p.Path).TrimEnd('\\')==rootPath);vm.SelectedProject=project;
      var key=rootPath.Contains("Threads")?"Threads":"audiobook";
      var content=(FrameworkElement)window.Content;await Flush();Layout(content,1460,920);
      var result=(Button)window.FindName("ProjectResultShortcut");Check(result.IsEnabled,"result shortcut missing");Click(result);await Flush();
      Check(vm.SelectedProgram?.Id.EndsWith(key=="Threads"?"/current-production":"/outputs")==true,"result shortcut target");
      var realResult=vm.SelectedProgram!;
      if(key=="Threads") {var review=(Button)window.FindName("ProjectReviewShortcut");Check(review.IsEnabled,"review shortcut");Click(review);await Flush();Check(vm.SelectedProgram!.Commands.Single().FileName.Contains("0.3.19"),"review route");Click(result);await Flush();}
      var search=(TextBox)window.FindName("ProjectSearchBox");search.Text="격리 QA 없는 프로젝트";search.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();await Flush();Layout(content,1060,720);
      Check(vm.FilteredProjects.Count==0,"project search");Check(vm.SelectedProject?.Id==project.Id,"project search discarded reading context");
      search.Text="";search.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();await Flush();
      var programSearch=(TextBox)window.FindName("ProgramSearchBox");programSearch.Text=realResult.DisplayName;programSearch.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();await Flush();
      Check(vm.FilteredFunctions.SelectMany(f=>f.Programs).Any(p=>p.Id==realResult.Id),"program search actual result");programSearch.Text="";programSearch.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();await Flush();
      foreach(var dark in new[]{false,true})
      {
       Click((Button)window.FindName(dark?"DarkThemeButton":"LightThemeButton"));await Flush();Check(vm.DarkMode==dark,"theme click");
       foreach(var width in new[]{1460,1060})
       {
        var height=width==1460?920:720;window.Width=width;window.Height=height;await Flush();Layout(content,width,height);
        Click(result);await Flush();Layout(content,width,height);await Flush();
        var inspector=(Border)window.FindName("ProgramInspector");var open=(Button)window.FindName("OpenProgramButton");
        var title=(TextBlock)window.FindName("InspectorTitle");var titleBounds=title.TransformToAncestor(inspector).TransformBounds(new Rect(title.RenderSize));Check(titleBounds.Bottom<=inspector.ActualHeight+1,"selected result title clipped");
        var bounds=open.TransformToAncestor(inspector).TransformBounds(new Rect(open.RenderSize));Check(bounds.Bottom<=inspector.ActualHeight+1&&bounds.Right<=inspector.ActualWidth+1,"open button clipped");
        Check(new WindowInteropHelper(window).Handle==IntPtr.Zero,"native window created");
        Capture(content,width,height,Path.Combine(output,$"{key}-projects-{width}-{(dark?"dark":"light")}.png"));
        reports.Add(new{key,width,height,dark,ProjectId=project.Id,ActualRecords=vm.WorkDashboard.Activities.Count,ResultPath=realResult.Path,NativeWindows=0});
       }
      }
      // Two isolated text artifacts exercise the real OpenProgramButton/reader route. No Explorer, review app or rating store is touched.
      var attention=Descendants(content).OfType<Button>().Single(b=>Equals(b.Tag,"attention"));Click(attention);await Flush();
      Check(((TabControl)window.FindName("WorkspaceTabs")).SelectedItem==window.FindName("WorkDashboardTab"),"work list navigation");Check(vm.WorkDashboard.ProjectFilterId==vm.SelectedProjectRecordId&&vm.WorkDashboard.StatusFilterKey=="needs-action","project action filters");
      Layout(content,1060,720);Capture(content,1060,720,Path.Combine(output,key+"-needs-action-1060.png"));
      var reset=Descendants(content).OfType<Button>().Single(b=>b.Name=="ClearWorkFilters");Click(reset);await Flush();Check(vm.WorkDashboard.ProjectFilterId==""&&vm.WorkDashboard.StatusFilterKey=="","reset actual filters");
      ((TabControl)window.FindName("WorkspaceTabs")).SelectedItem=window.FindName("ProjectsTab");await Flush();
      Click(Descendants(content).OfType<Button>().Single(b=>Equals(b.Tag,"recent")));await Flush();Check(((TabControl)window.FindName("ProjectContentTabs")).SelectedItem==window.FindName("ProjectActivityTab"),"recent records navigation");
      Click(result);await Flush();
      var fixture=Path.Combine(output,"isolated-output-review");Directory.CreateDirectory(fixture);
      foreach(var name in new[]{"출력 안내.md","리뷰 안내.md"})
      {
       var file=Path.Combine(fixture,name);await File.WriteAllTextAsync(file,"# 격리 UI 검증 자료\n실제 제작물·평가가 아닌 읽기 동작 검증입니다.");
       vm.SelectedProgram=new ProgramItem{Id="isolated-qa/"+name,ProjectId=project.Id,Name="격리 QA "+name,DisplayName="격리 QA "+name,Path=file,WorkingDirectory=fixture};await Flush();Layout(content,1060,720);
       Click((Button)window.FindName("OpenProgramButton"));
       for(var i=0;i<100 && (vm.Documents.FilePath!=file||vm.Documents.IsLoading);i++){await Task.Delay(10);await Flush();}
       Check(vm.Documents.FilePath==file&&vm.Documents.RawText.Contains("격리 UI 검증 자료")&&!vm.Documents.IsLoading,"isolated artifact button read");
       vm.Documents.ReturnToOrigin();await Flush();
      }
      vm.SelectedProgram=realResult;Check(vm.RunningCount==0&&!vm.CanRunRegisteredTasks&&!vm.CanReadLiveStatus,"execution occurred");
      reports.Add(new{key,IsolatedArtifactButtons=2,NativeWindows=0,OwnedExecutions=vm.RunningCount});
     }
     finally {vm.Dispose();}
    }
// Explicit QA fixtures only: never inserted into the real project dashboard or user evaluation store.
    var stressVm=new WorkDashboardViewModel(_=>Task.FromResult<IReadOnlyList<WorkActivity>>([]));
    var stressRows=Enumerable.Range(0,600).Select(i=>new WorkActivity{Id="isolated-stress-"+i,Title="격리 검증용 긴 작업 제목 · "+i+" · 실제 제작 기록 아님",ProjectId="isolated-ui-qa",Status="queued",Stage="검증용 단계",ResultSummary="격리 검증 fixture"}).ToArray();stressVm.ApplySnapshot(stressRows);
    var stressView=new AIControlTower.Views.WorkDashboardView{DataContext=stressVm,Foreground=(Brush)app.Resources["TextBrush"]};Layout(stressView,840,660);
    foreach(var text in Descendants(stressView).OfType<TextBlock>())text.FontSize*=1.25;
    Layout(stressView,840,660);var stressList=(ListBox)stressView.FindName("WorkList");stressList.ScrollIntoView(stressRows[^1]);await Flush();Layout(stressView,840,660);stressList.SelectedItem=stressRows[^1];await Flush();Layout(stressView,840,660);
    var listScroll=Descendants(stressList).OfType<ScrollViewer>().First();listScroll.ScrollToEnd();Layout(stressView,840,660);await Flush();Layout(stressView,840,660);
    var realizedRows=Descendants(stressList).OfType<ListBoxItem>().Count();Check(realizedRows>0&&realizedRows<600,"600 rows not virtualized");Check(stressList.ItemContainerGenerator.ContainerFromItem(stressRows[^1]) is ListBoxItem,"last row not visible after scrolling");
    Check(stressVm.Selected?.Id==stressRows[^1].Id,"large text last record selection");
    Capture(stressView,840,660,Path.Combine(output,"isolated-600-records-large-text.png"));
    stressVm.Search="· 599 ·";Layout(stressView,840,660);Check(stressVm.FilteredActivities.Count==1,"600 rows search");Click((Button)stressView.FindName("ClearWorkFilters"));Check(stressVm.FilteredActivities.Count==600,"600 rows reset");
    Check(app.Windows.Cast<Window>().All(w=>new WindowInteropHelper(w).Handle==IntPtr.Zero),"unexpected native source window");
    reports.Add(new{ExplicitQaFixtureRows=600,TextScale=1.25,VisibleListItems=realizedRows,LastRowRealized=true,FixtureSearchAndReset=true});
    File.WriteAllText(Path.Combine(output,"workflow-ui-proof.json"),JsonSerializer.Serialize(new{Pass=true,ApplicationType=app.GetType().FullName,AllWindowHandlesZero=app.Windows.Cast<Window>().All(w=>new WindowInteropHelper(w).Handle==IntPtr.Zero),NativeWindowsShown=0,OperationalReviewProgramsLaunched=0,BridgeTasksSubmitted=0,UserEvaluationWritten=false,IsolatedArtifactButtons=4,Reports=reports},new JsonSerializerOptions{WriteIndented=true}));
   }
   catch(Exception ex){code=1;File.WriteAllText(Path.Combine(output,"error.txt"),ex.ToString());}
   finally{Dispatcher.CurrentDispatcher.InvokeShutdown();}
  });Dispatcher.Run();return code;
 }
 static ResourceDictionary Resources()
 {
  var directory=new DirectoryInfo(AppContext.BaseDirectory);while(directory is not null&&!File.Exists(Path.Combine(directory.FullName,"ai-control-tower","src","AIControlTower","App.xaml")))directory=directory.Parent;
  if(directory is null)throw new DirectoryNotFoundException("Hub source root not found");
  var path=Path.Combine(directory.FullName,"ai-control-tower","src","AIControlTower","App.xaml");
  var root=XDocument.Load(path).Root!;var x=new XElement(root.Name.Namespace+"ResourceDictionary",root.Attributes().Where(a=>a.IsNamespaceDeclaration),root.Elements().Single().Nodes());
  foreach(var declaration in x.Attributes().Where(a=>a.IsNamespaceDeclaration&&a.Value.StartsWith("clr-namespace:AIControlTower")&&!a.Value.Contains(";assembly="))){var ns=declaration.Value;declaration.Value+=";assembly=AIControlTower";foreach(var e in x.Descendants().Where(e=>e.Name.NamespaceName==ns))e.Name=XName.Get(e.Name.LocalName,declaration.Value);}
  return (ResourceDictionary)XamlReader.Parse(x.ToString(),new ParserContext{BaseUri=new Uri(path)});
 }
 static IEnumerable<DependencyObject> Descendants(DependencyObject root){yield return root;for(var i=0;i<VisualTreeHelper.GetChildrenCount(root);i++)foreach(var child in Descendants(VisualTreeHelper.GetChild(root,i)))yield return child;}
 static void Click(Button button)=>button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
 static void Check(bool condition,string name){if(!condition)throw new InvalidOperationException(name);}
 static Task Flush()=>Dispatcher.CurrentDispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle).Task;
 static void Layout(FrameworkElement root,int w,int h){root.Measure(new Size(w,h));root.Arrange(new Rect(0,0,w,h));root.UpdateLayout();}
 static void Capture(FrameworkElement root,int w,int h,string path){var canvas=new DrawingVisual();using(var dc=canvas.RenderOpen())dc.DrawRectangle((Brush)Application.Current.Resources["CanvasBrush"],null,new Rect(0,0,w,h));var bmp=new RenderTargetBitmap(w,h,96,96,PixelFormats.Pbgra32);bmp.Render(canvas);bmp.Render(root);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bmp));using var stream=File.Create(path);encoder.Save(stream);}
}
