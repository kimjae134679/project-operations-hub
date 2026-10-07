using System.Reflection;
using AIControlTower.Models;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class ContinuousRegistrationTests
{
    private static WorkDashboardViewModel New(Func<CancellationToken,Task<IReadOnlyList<WorkActivity>>> read)
        =>new(read){SourcePathInput=@"D:\approved\state.json"};
    // Reproduce the old void callback faithfully until production can report its bool outcome.
    private static void SetCallback(WorkDashboardViewModel vm,Func<string,bool> callback)
    {
        var property=typeof(WorkDashboardViewModel).GetProperty("RegisterContinuousPath")!;
        if(property.PropertyType==typeof(Action<string>))property.SetValue(vm,(Action<string>)(path=>{_ = callback(path);}));
        else property.SetValue(vm,callback);
    }
    [Fact] public async Task MissingCallbackReportsUnavailableWithoutReadingOrClaimingSuccess()
    {
        var reads=0;var vm=New(_=>{reads++;return Task.FromResult<IReadOnlyList<WorkActivity>>([]);});
        await vm.RegisterSourceAsync();Assert.Equal(0,reads);Assert.Contains("연결하지 못함",vm.Message);Assert.DoesNotContain("연결했습니다",vm.Message);
    }
    [Fact] public async Task RejectedManualNoOpCallbackCannotRefreshOrReportConnected()
    {
        var reads=0;var calls=0;var vm=New(_=>{reads++;return Task.FromResult<IReadOnlyList<WorkActivity>>([]);});
        SetCallback(vm,path=>{calls++;Assert.Equal(vm.SourcePathInput,path);return false;});
        await vm.RegisterSourceAsync();Assert.Equal(1,calls);Assert.Equal(0,reads);Assert.Contains("거절",vm.Message);Assert.DoesNotContain("연결했습니다",vm.Message);
    }
    [Fact] public async Task AcceptedRegistrationReportsConnectionNotTaskExecutionAfterRefresh()
    {
        var reads=0;var vm=New(_=>{reads++;return Task.FromResult<IReadOnlyList<WorkActivity>>([]);});SetCallback(vm,_=>true);
        await vm.RegisterSourceAsync();Assert.Equal(1,reads);Assert.Contains("연결했습니다",vm.Message);Assert.Contains("작업 실행 없음",vm.Message);
    }
    [Fact] public async Task CallbackExceptionReportsFailureWithoutReadingOrSuccess()
    {
        var reads=0;var vm=New(_=>{reads++;return Task.FromResult<IReadOnlyList<WorkActivity>>([]);});SetCallback(vm,_=>throw new IOException("source_read_denied"));
        await vm.RegisterSourceAsync();Assert.Equal(0,reads);Assert.Contains("연결하지 못함",vm.Message);Assert.Contains("source_read_denied",vm.Message);Assert.DoesNotContain("연결했습니다",vm.Message);
    }
    [Fact] public async Task CallbackCancellationIsDistinctFromFailureOrSuccess()
    {
        var reads=0;var vm=New(_=>{reads++;return Task.FromResult<IReadOnlyList<WorkActivity>>([]);});SetCallback(vm,_=>throw new OperationCanceledException());
        await vm.RegisterSourceAsync();Assert.Equal(0,reads);Assert.Contains("취소",vm.Message);Assert.DoesNotContain("연결했습니다",vm.Message);
    }
    [Fact] public async Task AcceptedRegistrationCannotOverwriteReadFailureWithSuccess()
    {
        var vm=New(_=>throw new IOException("checkpoint_read_failed"));SetCallback(vm,_=>true);
        await vm.RegisterSourceAsync();Assert.Contains("조회 실패",vm.Message);Assert.Contains("checkpoint_read_failed",vm.Message);Assert.DoesNotContain("연결했습니다",vm.Message);
    }
    [Fact] public async Task AcceptedRegistrationWithCancelledReadCannotClaimReadCompleted()
    {
        var vm=New(_=>throw new OperationCanceledException());SetCallback(vm,_=>true);
        await vm.RegisterSourceAsync();Assert.Contains("조회 취소",vm.Message);Assert.DoesNotContain("연결했습니다",vm.Message);Assert.Contains("작업 실행 없음",vm.Message);
    }
    [Fact] public void RegistrationContractReturnsBoolRatherThanAssumingCallbackSuccess()
    {
        Assert.Equal(typeof(Func<string,bool>),typeof(WorkDashboardViewModel).GetProperty("RegisterContinuousPath")!.PropertyType);
        // Manual/local-view source safety is exercised directly by ProjectSessionsRefinementTests;
        // constructing MainViewModel here would touch host watchers and settings.
    }
}
