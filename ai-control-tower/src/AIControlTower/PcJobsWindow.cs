using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using AIControlTower.ViewModels;
using AIControlTower.Services;

namespace AIControlTower;
public sealed class PcJobsWindow : Window
{
    private readonly PcConnectionViewModel _vm; private readonly DispatcherTimer _timer;
    private readonly TextBox _project = new() { Text = "Control-Tower" }, _tool = new() { Text = "통합관리", IsReadOnly = true }, _target = new(), _command = new() { AcceptsReturn = true, Height = 80, TextWrapping = TextWrapping.Wrap };
    private readonly TextBox _result = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    private readonly Image _preview = new() { Stretch = System.Windows.Media.Stretch.Uniform };
    private readonly ComboBox _action = new(); private readonly ListBox _jobs = new(); private readonly TextBlock _message = new();
    private bool _loading, _closed; private string _lastResult = "";
    private sealed record Choice(string Label, string Action) { public override string ToString() => Label; }
    public PcJobsWindow(PcConnectionViewModel vm)
    {
        _vm = vm; DataContext = vm; Title = "공용 PC 작업"; Width = 1000; Height = 730; MinWidth = 820; MinHeight = 580;
        SetResourceReference(BackgroundProperty,"CanvasBrush"); SetResourceReference(ForegroundProperty,"TextBrush"); FontFamily = new System.Windows.Media.FontFamily("Malgun Gothic");
        var grid = new Grid { Margin = new Thickness(22) }; grid.RowDefinitions.Add(new() { Height = GridLength.Auto }); grid.RowDefinitions.Add(new() { Height = GridLength.Auto }); grid.RowDefinitions.Add(new());
        var title = new TextBlock { Text = "같은 PC에서 여러 도구의 작업을 함께 처리합니다", FontSize = 21, FontWeight = FontWeights.SemiBold, Margin = new(0,0,0,16) }; grid.Children.Add(title);
        var form = new StackPanel { Margin = new(0,0,0,18) }; Grid.SetRow(form,1); grid.Children.Add(form);
        var names = new Grid(); names.ColumnDefinitions.Add(new()); names.ColumnDefinitions.Add(new());
        names.Children.Add(Field("프로젝트",_project)); var tool = Field("요청 도구",_tool); Grid.SetColumn(tool,1); names.Children.Add(tool); form.Children.Add(names);
        foreach(var c in new[]{new Choice("PC 기능 확인","capabilities"),new Choice("폴더 확인","list_dir"),new Choice("파일 읽기","read_file"),new Choice("명령 실행","run_command"),new Choice("장기 작업 시작","start_process"),new Choice("화면 목록 확인","window_list"),new Choice("화면 캡처","screen_capture")}) _action.Items.Add(c);
        _action.SelectedIndex=0; form.Children.Add(Field("할 일",_action)); form.Children.Add(Field("파일·폴더 또는 명령을 실행할 폴더",_target)); form.Children.Add(Field("실행할 PowerShell 명령",_command));
        _target.Text=ProjectBridgeService.DefaultHome; var submit = new Button { Content="작업 요청",HorizontalAlignment=HorizontalAlignment.Left,MinWidth=130,Margin=new(0,10,0,8) };
        submit.Click += async(_,_)=>
        {
            submit.IsEnabled=false;
            try
            {
                var choice=(Choice)_action.SelectedItem; var action=choice.Action;
                object args=action switch { "list_dir"=>new {path=_target.Text,limit=200},"read_file"=>new {path=_target.Text,maxBytes=64000},"run_command" or "start_process"=>new {cwd=_target.Text,powershell=_command.Text,timeoutSeconds=300},"window_list"=>new {action="window_list",args=new {limit=60}},"screen_capture"=>new {action="screen_capture",args=new {maxWidth=1400,maxHeight=1000,quality=65}},_=>new{} };
                if(action is "window_list" or "screen_capture")action="ui_control";
                var id=await vm.SubmitAsync(_project.Text,"control-tower",action,args);_message.Text="요청했습니다. 다른 작업과 함께 처리되며 목록에서 결과를 볼 수 있습니다.";await vm.PollAsync();_jobs.SelectedItem=vm.Jobs.FirstOrDefault(j=>j.Id==id);
            }
            catch(Exception e){_message.Text=ProcessRunner.Sanitize(e.Message);} finally{submit.IsEnabled=true;}
        };
        form.Children.Add(submit); _message.TextWrapping=TextWrapping.Wrap;form.Children.Add(_message);
        var body=new Grid();body.ColumnDefinitions.Add(new(){Width=new GridLength(350)});body.ColumnDefinitions.Add(new());Grid.SetRow(body,2);grid.Children.Add(body);
        _jobs.ItemsSource=vm.Jobs;_jobs.DisplayMemberPath="Title";_jobs.Margin=new(0,0,18,0);body.Children.Add(_jobs);
        var results=new TabControl();results.Items.Add(new TabItem{Header="작업 결과",Content=_result});results.Items.Add(new TabItem{Header="화면",Content=_preview});Grid.SetColumn(results,1);body.Children.Add(results);
        // Each row keeps project/tool/state visible instead of a bare command title.
        var template=new DataTemplate(typeof(PcJobViewModel));var panel=new FrameworkElementFactory(typeof(StackPanel));panel.SetValue(FrameworkElement.MarginProperty,new Thickness(8));
        foreach(var field in new[]{"Title","StatusText","Project","Tool"}){var line=new FrameworkElementFactory(typeof(TextBlock));line.SetBinding(TextBlock.TextProperty,new Binding(field));line.SetValue(TextBlock.TextWrappingProperty,TextWrapping.Wrap);panel.AppendChild(line);}template.VisualTree=panel;_jobs.ClearValue(ItemsControl.DisplayMemberPathProperty);_jobs.ItemTemplate=template;
        _jobs.SelectionChanged+=async(_,_)=>await UpdateResultAsync(); Content=grid;
        _timer=new DispatcherTimer {Interval=TimeSpan.FromSeconds(2)};_timer.Tick+=async(_,_)=>{await vm.PollAsync();await UpdateResultAsync();};_timer.Start();
        Closed+=(_,_)=>{_closed=true;_timer.Stop();};
    }
    private static StackPanel Field(string label,Control control){var p=new StackPanel{Margin=new(0,0,12,8)};p.Children.Add(new TextBlock{Text=label,Margin=new(0,0,0,4)});p.Children.Add(control);return p;}
    private async Task UpdateResultAsync()
    {
        if(_loading||_closed||_jobs.SelectedItem is not PcJobViewModel job)return;_loading=true;
        try
        {
            var text=await _vm.ResultAsync(job.Id);
            if(!_closed&&_jobs.SelectedItem is PcJobViewModel selected&&selected.Id==job.Id&&text!=_lastResult)
            {
                using var parsed=JsonDocument.Parse(text);var value=FindCapture(parsed.RootElement,0);_preview.Source=null;
                if(value is not null)
                {
                    try{var image=new System.Windows.Media.Imaging.BitmapImage();using var stream=new MemoryStream(Convert.FromBase64String(value));image.BeginInit();image.CacheOption=System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;image.StreamSource=stream;image.EndInit();image.Freeze();_preview.Source=image;}catch(Exception e) when(e is FormatException or IOException or NotSupportedException){_message.Text="화면 자료를 표시하지 못했습니다.";}
                    _result.Text="화면을 캡처했습니다. ‘화면’ 탭에서 확인하세요.\n"+job.StatusText;
                }
                else _result.Text=text;
                _lastResult=text;
            }
        }
        catch(Exception e){if(!_closed)_message.Text=ProcessRunner.Sanitize(e.Message);}finally{_loading=false;}
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
