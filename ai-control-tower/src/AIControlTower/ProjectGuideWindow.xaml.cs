using System.Diagnostics;
using System.Windows;
using AIControlTower.Services;

namespace AIControlTower;

public partial class ProjectGuideWindow : Window
{
    private readonly string _folder;
    public string GuideFilePath { get; }
    public bool GuideLoaded { get; }
    public ProjectGuideWindow(string name,string folder,string? documentPath=null)
    {
        InitializeComponent();
        PreviewMouseWheel += MouseWheelRouting.HandlePreviewMouseWheel;
        _folder=folder;
        GuideFilePath=documentPath??Path.Combine(folder,"프로젝트_사용안내.md");
        GuideTitle.Text=name+" · 사용 안내";
        GuideLocation.Text=GuideFilePath;
        NoticeDocument.SetBaseDirectory(GuideBody,Path.GetDirectoryName(GuideFilePath)!);
        try
        {
            var body=File.ReadAllText(GuideFilePath);
            NoticeDocument.SetText(GuideBody,body);
            GuideLoaded=true;
        }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException)
        {
            NoticeDocument.SetText(GuideBody,"## 안내 작성 대기\n이 프로젝트의 실제 작업 폴더에 프로젝트_사용안내.md를 작성해야 합니다.\n\n입력·결과·참고·보관 자료의 위치와 하는 일을 실제 파일에서 확인해 적습니다. 아직 없는 안내를 만든 것처럼 표시하지 않습니다.");
        }
    }
    private void OpenFolder_Click(object sender,RoutedEventArgs e)
    {
        if(Directory.Exists(_folder)) Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute=true,Arguments="\""+_folder+"\"" })?.Dispose();
    }
}
