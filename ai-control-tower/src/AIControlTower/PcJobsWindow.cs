using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using AIControlTower.ViewModels;
using AIControlTower.Services;
namespace AIControlTower;
/// <summary>Shared PC operations stay inside the main application; no execution window is spawned.</summary>
public sealed class PcJobsPanel : UserControl
{
 private readonly PcConnectionViewModel _vm; private readonly DispatcherTimer _timer;
 private readonly ComboBox _project = new(), _tool = new();
 private readonly TextBox _target = new(), _command = new() { AcceptsReturn = true, Height = 64, TextWrapping = TextWrapping.Wrap };
 private readonly PcJobResultPresentation _resultReader;
 private readonly TextBox _result = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontSize = 13 };
 private readonly Image _preview = new() { Stretch = System.Windows.Media.Stretch.Uniform };
 private readonly ComboBox _action = new(); private readonly ListBox _jobs = new(); private readonly TextBlock _message = new();
 private readonly Expander _formExpander = new() { Header = "새 작업 요청", IsExpanded = false };
 private bool _active; private string _lastResult = "", _pendingJobId = "";
 private sealed record Choice(string Label, string Action) { public override string ToString() => Label; }
 public PcJobsPanel(PcConnectionViewModel vm,MainViewModel main)
 {
  _vm = vm; _resultReader=new((id,ct)=>vm.ResultAsync(id,ct),TimeSpan.FromSeconds(3));_resultReader.Changed+=RenderResult;
  DataContext = vm; SetResourceReference(ForegroundProperty,"TextBrush");
  _project.ItemsSource=main.Projects;_project.DisplayMemberPath="DisplayName";_project.SelectedValuePath="Id";
  foreach(var issuer in new[]{new Choice("통합관리","control-tower"),new Choice("Codex","codex"),new Choice("Jev","jev"),new Choice("Claude","claude")})_tool.Items.Add(issuer);_tool.SelectedIndex=0;
  if(main.Projects.Count>0)_project.SelectedIndex=0;
  main.Projects.CollectionChanged+=(_,_)=>{if(_project.SelectedIndex<0&&main.Projects.Count>0)_project.SelectedIndex=0;};
  var grid = new Grid(); grid.RowDefinitions.Add(new() { Height = GridLength.Auto }); grid.RowDefinitions.Add(new() { Height = GridLength.Auto }); grid.RowDefinitions.Add(new());
  var heading = new Grid { Margin = new(0,0,0,10) }; heading.ColumnDefinitions.Add(new()); heading.ColumnDefinitions.Add(new() { Width = GridLength.Auto }); grid.Children.Add(heading);
  heading.Children.Add(new TextBlock { Text = "공용 작업과 결과", FontSize = 20, FontWeight = FontWeights.SemiBold });
  var count = new TextBlock { VerticalAlignment = VerticalAlignment.Center }; count.SetResourceReference(TextBlock.ForegroundProperty,"MutedBrush"); count.SetBinding(TextBlock.TextProperty,new Binding("ActiveJobCount") { StringFormat = "실행 중 {0}개" }); Grid.SetColumn(count,1); heading.Children.Add(count);
  var form = new StackPanel { Margin = new(12,4,12,10) }; var formScroll = new ScrollViewer { Content = form, MaxHeight = 270, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
  _formExpander.Content = formScroll; _formExpander.Margin = new(0,0,0,12); Grid.SetRow(_formExpander,1); grid.Children.Add(_formExpander);
  var names = new Grid(); names.ColumnDefinitions.Add(new()); names.ColumnDefinitions.Add(new());
  names.Children.Add(Field("프로젝트",_project)); var tool = Field("요청 도구",_tool); Grid.SetColumn(tool,1); names.Children.Add(tool); form.Children.Add(names);
  foreach(var c in new[]{new Choice("PC 기능 확인","capabilities"),new Choice("폴더 확인","list_dir"),new Choice("파일 읽기","read_file"),new Choice("명령 실행","run_command"),new Choice("장기 작업 시작","start_process"),new Choice("화면 목록 확인","window_list"),new Choice("화면 캡처","screen_capture")}) _action.Items.Add(c);
  _action.SelectedIndex=0; var fields=new Grid(); fields.ColumnDefinitions.Add(new(){Width=new GridLength(220)}); fields.ColumnDefinitions.Add(new()); fields.Children.Add(Field("할 일",_action)); var target=Field("파일·폴더 또는 작업 폴더",_target); Grid.SetColumn(target,1); fields.Children.Add(target); form.Children.Add(fields);
  var commandField=Field("실행할 PowerShell 명령",_command); form.Children.Add(commandField); commandField.Visibility=Visibility.Collapsed;
  _action.SelectionChanged+=(_,_)=>{ var action=(_action.SelectedItem as Choice)?.Action; commandField.Visibility=action is "run_command" or "start_process" ? Visibility.Visible : Visibility.Collapsed; };
  _target.Text="";
  _target.ToolTip="작업 폴더를 비우면 프로젝트·요청 주체별 공간을 사용합니다.";
  var submit=new Button { IsEnabled=!vm.IsReadOnly, Content=vm.IsReadOnly?"로컬 조회 · 요청 보류":"작업 요청",HorizontalAlignment=HorizontalAlignment.Left,MinWidth=130,Margin=new(0,8,0,8) };
  submit.Click+=async(_,_)=>
  {
   if(vm.IsReadOnly){_message.Text="로컬 조회 · PC/API 작업 요청은 실행하지 않습니다.";return;}
   submit.IsEnabled=false;
   try
   {
    var choice=(Choice)_action.SelectedItem; var action=choice.Action;
    object args=action switch { "list_dir"=>new{path=_target.Text,limit=200},"read_file"=>new{path=_target.Text,maxBytes=64000},"run_command" or "start_process"=>new{cwd=_target.Text,powershell=_command.Text,timeoutSeconds=300},"window_list"=>new{action="window_list",args=new{limit=60}},"screen_capture"=>new{action="screen_capture",args=new{maxWidth=1400,maxHeight=1000,quality=65}},_=>new{} };
    if(action is "window_list" or "screen_capture")action="ui_control";
    var projectId=_project.SelectedValue as string; if(string.IsNullOrWhiteSpace(projectId))throw new InvalidOperationException("작업할 프로젝트를 선택하세요.");
    var issuer=(_tool.SelectedItem as Choice)?.Action ?? "control-tower";
    var id=await vm.SubmitAsync(projectId,issuer,action,args); _message.SetResourceReference(TextBlock.ForegroundProperty,"StatusSuccessInk"); _message.Text="요청됨";
    _pendingJobId=id; await vm.PollAsync(); SelectPendingJob(); _formExpander.IsExpanded=false;
   }
   catch(Exception e){_message.SetResourceReference(TextBlock.ForegroundProperty,"StatusErrorInk");_message.Text=ProcessRunner.Sanitize(e.Message);}finally{submit.IsEnabled=!vm.IsReadOnly;}
  };
  form.Children.Add(submit); _message.TextWrapping=TextWrapping.Wrap; form.Children.Add(_message);
  var body=new Grid(); body.ColumnDefinitions.Add(new(){Width=new GridLength(320)}); body.ColumnDefinitions.Add(new(){Width=new GridLength(14)}); body.ColumnDefinitions.Add(new()); Grid.SetRow(body,2); grid.Children.Add(body);
  var listArea=new Grid();body.Children.Add(listArea);
  _jobs.ItemsSource=vm.Jobs; _jobs.Name="InlinePcJobs";_jobs.SetResourceReference(BackgroundProperty,"SurfaceBrush");listArea.Children.Add(_jobs);
  var resultBody=new Grid();resultBody.RowDefinitions.Add(new(){Height=GridLength.Auto});resultBody.RowDefinitions.Add(new());Grid.SetColumn(resultBody,2);body.Children.Add(resultBody);
  var resultHeading=new Grid{Margin=new(0,0,0,10)};resultHeading.ColumnDefinitions.Add(new());resultHeading.ColumnDefinitions.Add(new(){Width=GridLength.Auto});resultBody.Children.Add(resultHeading);
  var selectedTitle=new TextBlock{Text="작업 선택",FontSize=16,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap,Margin=new(0,0,12,0)};selectedTitle.SetBinding(TextBlock.TextProperty,new Binding("SelectedItem.Title"){Source=_jobs,TargetNullValue="작업 선택"});resultHeading.Children.Add(selectedTitle);
  var selectedState=new StatusBadge();selectedState.SetBinding(StatusBadge.ValueProperty,new Binding("SelectedItem.StatusText"){Source=_jobs});selectedState.SetBinding(StatusBadge.HasErrorProperty,new Binding("SelectedItem.HasError"){Source=_jobs});Grid.SetColumn(selectedState,1);resultHeading.Children.Add(selectedState);
  var results=new TabControl();results.Items.Add(new TabItem{Header="작업 결과",Content=_result});results.Items.Add(new TabItem{Header="화면",Content=_preview});Grid.SetRow(results,1);resultBody.Children.Add(results);
  var template=new DataTemplate(typeof(PcJobViewModel));var row=new FrameworkElementFactory(typeof(Grid));
  var panel=new FrameworkElementFactory(typeof(StackPanel));
  var title=new FrameworkElementFactory(typeof(TextBlock));title.SetBinding(TextBlock.TextProperty,new Binding("Title"));title.SetValue(TextBlock.FontSizeProperty,15d);title.SetValue(TextBlock.FontWeightProperty,FontWeights.SemiBold);title.SetValue(TextBlock.TextWrappingProperty,TextWrapping.Wrap);panel.AppendChild(title);
  var state=new FrameworkElementFactory(typeof(StatusBadge));state.SetBinding(StatusBadge.ValueProperty,new Binding("StatusText"));state.SetBinding(StatusBadge.HasErrorProperty,new Binding("HasError"));state.SetValue(FrameworkElement.HorizontalAlignmentProperty,HorizontalAlignment.Left);state.SetValue(FrameworkElement.MarginProperty,new Thickness(0,8,0,8));panel.AppendChild(state);
  foreach(var field in new[]{"Project","Tool"}){var line=new FrameworkElementFactory(typeof(TextBlock));line.SetBinding(TextBlock.TextProperty,new Binding(field){Converter=new FriendlyJobLabel(main),ConverterParameter=field,StringFormat=field=="Tool"?"요청 도구 · {0}":"프로젝트 · {0}"});line.SetResourceReference(TextBlock.ForegroundProperty,"MutedBrush");line.SetValue(TextBlock.FontSizeProperty,12d);line.SetValue(TextBlock.TextWrappingProperty,TextWrapping.Wrap);panel.AppendChild(line);}row.AppendChild(panel);template.VisualTree=row;_jobs.ItemTemplate=template;
  _jobs.SelectionChanged+=async(_,_)=>{_lastResult="";_preview.Source=null;await UpdateResultAsync();};Content=grid;
  _timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};_timer.Tick+=async(_,_)=>{SelectPendingJob();await UpdateResultAsync();};
  Loaded+=async(_,_)=>{_active=true;if(IsVisible&&!vm.IsReadOnly)_timer.Start();await UpdateResultAsync();};Unloaded+=(_,_)=>{_active=false;_timer.Stop();_resultReader.Cancel();};IsVisibleChanged+=async(_,_)=>{if(IsVisible&&_active&&!vm.IsReadOnly){_timer.Start();await UpdateResultAsync();}else{_timer.Stop();_resultReader.Cancel();}};
 }
 private sealed class FriendlyJobLabel(MainViewModel main):IValueConverter
 {
  public object Convert(object value,Type targetType,object parameter,System.Globalization.CultureInfo culture)
  {
   var text=value as string??"";return parameter as string=="Tool"?text.ToLowerInvariant() switch{"control-tower"=>"통합관리","codex"=>"Codex","jev"=>"Jev","claude"=>"Claude",_=>text}:main.Projects.FirstOrDefault(p=>ProjectBridgeService.CanonicalProjectId(p.Id)==ProjectBridgeService.CanonicalProjectId(text))?.DisplayName??(text=="Control-Tower"?"전체 프로젝트 관리":text);
  }
  public object ConvertBack(object value,Type targetType,object parameter,System.Globalization.CultureInfo culture)=>throw new NotSupportedException();
 }
 private void SelectPendingJob()
 {
  if(_pendingJobId.Length==0)return;var job=_vm.Jobs.FirstOrDefault(j=>j.Id==_pendingJobId);if(job is null)return;_jobs.SelectedItem=job;_pendingJobId="";
 }
 public void FocusJobs() { _jobs.Focus(); }
 public void ShowRequestForm() { _formExpander.IsExpanded=true; _project.Focus(); }
 private static StackPanel Field(string label,Control control){var p=new StackPanel{Margin=new(0,0,12,8)};p.Children.Add(new TextBlock{Text=label,Margin=new(0,0,0,4),FontSize=12});p.Children.Add(control);return p;}
 private Task UpdateResultAsync()=>_resultReader.RefreshAsync((_jobs.SelectedItem as PcJobViewModel)?.Id,!_vm.IsReadOnly,_active&&IsVisible);
 private void RenderResult()
 {
  if(_resultReader.State=="idle"){_lastResult="";_result.Text="작업 선택";_preview.Source=null;return;}
  if(_jobs.SelectedItem is not PcJobViewModel job||job.Id!=_resultReader.SelectedId)return;
  if(_resultReader.State!="ready")
  {
   if(_resultReader.State=="loading"&&_lastResult.Length>0)return;
   _lastResult="";_preview.Source=null;_result.Text=_resultReader.Text;return;
  }
  if(!_active||!IsVisible)return;
  try
  {
   var text=_resultReader.Text;
   if(text!=_lastResult)
   {
    using var parsed=JsonDocument.Parse(text);var value=FindCapture(parsed.RootElement,0);_preview.Source=null;
    if(value is not null)
    {
     try{var image=new System.Windows.Media.Imaging.BitmapImage();using var stream=new MemoryStream(Convert.FromBase64String(value));image.BeginInit();image.CacheOption=System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;image.StreamSource=stream;image.EndInit();image.Freeze();_preview.Source=image;}catch(Exception e)when(e is FormatException or IOException or NotSupportedException){_message.Text="화면 자료를 표시하지 못했습니다.";}
     _result.Text="화면을 캡처했습니다. ‘화면’ 탭에서 확인하세요.\n"+job.StatusText;
    }
    else _result.Text=JsonSerializer.Serialize(parsed.RootElement,new JsonSerializerOptions{WriteIndented=true});
    _lastResult=text;
   }
  }
  catch(Exception e){_lastResult="";_preview.Source=null;_result.Text=ProcessRunner.Sanitize(e.Message);}
 }
 private static string? FindCapture(JsonElement node,int depth)
 {
  if(depth>6||node.ValueKind!=JsonValueKind.Object)return null;
  foreach(var p in node.EnumerateObject())
  {
   if(p.Name is "jpeg_base64" or "jpegBase64" or "imageBase64"&&p.Value.ValueKind==JsonValueKind.String)return p.Value.GetString();
   var nested=FindCapture(p.Value,depth+1);if(nested is not null)return nested;
  }
  return null;
 }
}
