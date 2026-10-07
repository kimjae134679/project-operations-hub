using System.Net;
using System.Reflection;
using System.Text.Json;
using AIControlTower.Services;
using AIControlTower.ViewModels;

namespace AIControlTower.Tests;

public sealed class ProjectBridgeResultDeadlineTests
{
    private sealed class BlockedBody : Stream
    {
        public TaskCompletionSource Started { get; }=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Cancelled { get; }=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer,CancellationToken ct=default)
        {Started.TrySetResult();try{await Task.Delay(Timeout.InfiniteTimeSpan,ct);return 0;}catch(OperationCanceledException){Cancelled.TrySetResult();throw;}}
        public override Task<int> ReadAsync(byte[] buffer,int offset,int count,CancellationToken ct)=>ReadAsync(buffer.AsMemory(offset,count),ct).AsTask();
        public override bool CanRead=>true;public override bool CanSeek=>false;public override bool CanWrite=>false;
        public override long Length=>throw new NotSupportedException();public override long Position {get=>throw new NotSupportedException();set=>throw new NotSupportedException();}
        public override void Flush(){}public override int Read(byte[] buffer,int offset,int count)=>throw new NotSupportedException();
        public override long Seek(long offset,SeekOrigin origin)=>throw new NotSupportedException();public override void SetLength(long value)=>throw new NotSupportedException();public override void Write(byte[] buffer,int offset,int count)=>throw new NotSupportedException();
    }
    private sealed class Handler(BlockedBody body):HttpMessageHandler
    {
        public int Requests;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {Requests++;Assert.Equal(HttpMethod.Get,request.Method);return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StreamContent(body)});}
    }
    private static string FakeHome()
    {
        var root=Path.Combine(@"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\pc-jobs-fixtures",Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root,"state"));File.WriteAllText(Path.Combine(root,"state","local_endpoint.json"),JsonSerializer.Serialize(new{baseUrl="http://127.0.0.1:1/",token=new string('x',32)}));return root;
    }
    [Fact]
    public async Task ConfiguredRequestDeadlineAlsoCancelsBodyAfterHeaders()
    {
        var body=new BlockedBody();var handler=new Handler(body);using var client=new HttpClient(handler){Timeout=TimeSpan.FromMilliseconds(80)};
        using var service=new ProjectBridgeService(FakeHome(),client);using var cleanup=new CancellationTokenSource();
        var query=service.ResultAsync("fixture-job",cleanup.Token);
        try{await body.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>query.WaitAsync(TimeSpan.FromSeconds(2)));Assert.True(body.Cancelled.Task.IsCompleted);Assert.Equal(1,handler.Requests);}
        finally{cleanup.Cancel();try{await query;}catch(OperationCanceledException){}}
    }
    [Fact]
    public async Task ExplicitCallerCancellationThroughVmAbortsBodyWithoutAnotherRequest()
    {
        var method=typeof(PcConnectionViewModel).GetMethod("ResultAsync",[typeof(string),typeof(CancellationToken)]);Assert.NotNull(method);
        var body=new BlockedBody();var handler=new Handler(body);using var client=new HttpClient(handler){Timeout=TimeSpan.FromSeconds(3)};
        using var service=new ProjectBridgeService(FakeHome(),client);using var vm=new PcConnectionViewModel(service,null,false,true);using var cancellation=new CancellationTokenSource();
        var query=Assert.IsAssignableFrom<Task<string>>(method.Invoke(vm,["fixture-job",cancellation.Token]));
        try{await body.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));cancellation.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>query.WaitAsync(TimeSpan.FromSeconds(2)));Assert.Equal(1,handler.Requests);Assert.True(body.Cancelled.Task.IsCompleted);}
        finally{cancellation.Cancel();try{await query;}catch(OperationCanceledException){}}
    }
    [Fact]
    public async Task VmLifetimeCancellationStillCancelsLinkedCallerRead()
    {
        var method=typeof(PcConnectionViewModel).GetMethod("ResultAsync",[typeof(string),typeof(CancellationToken)]);Assert.NotNull(method);
        var body=new BlockedBody();var handler=new Handler(body);using var client=new HttpClient(handler){Timeout=TimeSpan.FromSeconds(3)};
        using var service=new ProjectBridgeService(FakeHome(),client);using var vm=new PcConnectionViewModel(service,null,false,true);
        var query=Assert.IsAssignableFrom<Task<string>>(method.Invoke(vm,["fixture-job",CancellationToken.None]));
        await body.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));vm.Dispose();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>query.WaitAsync(TimeSpan.FromSeconds(2)));Assert.Equal(1,handler.Requests);
    }
}
