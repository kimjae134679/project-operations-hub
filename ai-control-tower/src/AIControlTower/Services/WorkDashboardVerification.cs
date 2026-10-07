using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AIControlTower.Models;
using AIControlTower.ViewModels;
using AIControlTower.Views;
namespace AIControlTower.Services;

/// <summary>Independent in-memory UI fixture. Never constructs MainViewModel or loads settings.</summary>
public static class WorkDashboardVerification
{
    public static WorkDashboardViewModel CreateFixture()
    {
        var at = new DateTimeOffset(2026, 10, 7, 5, 0, 0, TimeSpan.FromHours(9));
        IReadOnlyList<WorkActivity> rows = [
            new() { Id="fixture:manager", Project="컨트롤타워", ProjectId="project-operations-hub", ProjectDisplayName="컨트롤타워", Worker="연속 실행기", WorkerKind="연속 실행기", Source="검증용 가짜 자료", Title="두 단계 목록 · 기반 검증", Status="running", Stage="1/2단계 완료 · 두 번째 단계 기록", UpdatedAt=at, Evidence="가짜 체크포인트 · 현재 생존 미확인 · 실제 실행 아님", NextCheckpoint="검증 결과 확인 · 가짜 예시" },
            new() { Id="fixture:bridge", Project="공용 PC", Worker="ProjectBridge", Source="검증용 가짜 자료", Title="장기 작업 시작 접수", Status="accepted", Stage="접수만 확인 · 자식 완료 미확인", UpdatedAt=at, Evidence="연결 상태와 실제 작업 완료를 구분하는 가짜 예시" },
            new() { Id="fixture:jev", Project="콘텐츠 제작", Worker="Jev", Source="검증용 가짜 자료", Title="Jev 외부 작업 · 연결 미확인", Status="unknown", UpdatedAt=at, Evidence="설치/프로세스만으로 실제 진행을 추측하지 않음" },
            new() { Id="fixture:process", Project="컨트롤타워", ProjectId="Control-Tower", ProjectDisplayName="컨트롤타워", Worker="관리자 기록", Source="검증용 가짜 task_exchange", Title="통합관리 진행 · 최신 기록", Status="in_progress", Stage="revision 2", IsManagementRecord=true, Revision=2, UpdatedAt=at, Evidence="가짜 명령·답변 기록 · 자동 채팅 감시 아님", RecentLog="상태 조회 UI 연결\n비밀값을 공개하지 않는 정제 표시", NextCheckpoint="조회 화면 검증 → 설치는 별도 승인·검증", Error="실제 Jev 작업 연결은 미확인" }
        ];
        return new(_ => Task.FromResult(rows)) { IsFixture = true };
    }
    public static async Task RunAsync(string output)
    {
        output = WorkDashboardService.SafePath(output);
        if (!output.StartsWith(@"D:\", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("검증 출력은 명시한 D: 검사 폴더만 사용합니다.");
        Directory.CreateDirectory(output);
        var vm = CreateFixture(); await vm.RefreshAsync();
        var view = new WorkDashboardView { DataContext=vm };
        // No Window, Show, screen capture or foreground interaction, even off-screen.
        // Render only this new in-memory UserControl tree.
            vm.Selected = vm.Activities.First(); await vm.LoadSelectedDetailsAsync();
            Capture(view,1460,900,Path.Combine(output,"work-dashboard-wide.png"));
            Capture(view,1060,720,Path.Combine(output,"work-dashboard-compact.png"));
            vm.ManagementOnly=true; vm.Selected=vm.Activities.Single(r=>r.IsManagementRecord);
            Capture(view,1060,720,Path.Combine(output,"management-process.png"));
            vm.Search="Jev";vm.ManagementOnly=false;
            if(vm.FilteredActivities.Count!=1 || vm.CanStopOwned) throw new InvalidOperationException("Fixture filter or ownership boundary failed.");
            File.WriteAllText(Path.Combine(output,"work-dashboard-verification.json"),JsonSerializer.Serialize(new { Mode="isolated in-memory control fixture", WindowsCreated=0, GlobalSettingsLoaded=false, MainViewModelConstructed=false, ExecutedProcesses=0, ExternalSync=false, StoppedProcesses=0, Rows=vm.Activities.Count, SearchMatched=vm.FilteredActivities.Count, Build=typeof(WorkDashboardVerification).Assembly.GetName().Version?.ToString() },new JsonSerializerOptions{WriteIndented=true}));
    }
    private static void Capture(FrameworkElement view,int width,int height,string path)
    {
        view.Measure(new Size(width,height));view.Arrange(new Rect(0,0,width,height));view.UpdateLayout();
        var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(view);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var stream=File.Create(path);encoder.Save(stream);
    }
}
