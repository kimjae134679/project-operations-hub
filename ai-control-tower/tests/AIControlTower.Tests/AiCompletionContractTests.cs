using System.Reflection;
using System.Text.Json;
using AIControlTower.Services;

namespace AIControlTower.Tests;

// Reflection keeps the RED build compilable before the production contract exists.
public sealed class AiCompletionContractTests
{
    private static Type Required(string name)
    {
        var type = typeof(JobManager).Assembly.GetType("AIControlTower.Services." + name);
        Assert.NotNull(type);
        return type!;
    }
    private static object Evaluator(string adapter = "codex-jsonl-v1", string? report = null, string? root = null)
    {
        var contract = Activator.CreateInstance(Required("AiCompletionContract"), adapter, "gpt-6.1-sol", report)!;
        return Activator.CreateInstance(Required("AiCompletionEvaluator"), contract,
            root ?? Path.GetTempPath(), "owned-job", DateTimeOffset.UtcNow)!;
    }
    private static void Observe(object evaluator, string line) =>
        evaluator.GetType().GetMethod("ObserveStdout")!.Invoke(evaluator, [line]);
    private static JsonElement Complete(object evaluator, int? exitCode = 0, string state = "succeeded") =>
        JsonSerializer.SerializeToElement(evaluator.GetType().GetMethod("Complete")!.Invoke(evaluator, [exitCode, state]),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    private static void Success(object evaluator)
    {
        Observe(evaluator, "{\"type\":\"item.completed\",\"item\":{\"type\":\"agent_message\",\"text\":\"SECRET_REPORT_BODY\"}}");
        Observe(evaluator, "{\"type\":\"turn.completed\",\"usage\":{\"output_tokens\":1}}");
    }
    [Fact]
    public void RunAsyncRetainsExistingArgumentsAndAddsOnlyOptionalWorkspaceScope()
    {
        var parameters = typeof(JobManager).GetMethod("RunAsync")!.GetParameters();
        Assert.Equal(6, parameters.Length);
        Assert.Equal(new[] { "program", "command", "cancellationToken", "standardInput", "aiContract", "independentWorkspaceRoot" }, parameters.Select(p => p.Name));
        Assert.Equal(new[] { typeof(AIControlTower.Models.ProgramItem), typeof(AIControlTower.Models.ProgramCommand), typeof(CancellationToken), typeof(string), typeof(AiCompletionContract), typeof(string) }, parameters.Select(p => p.ParameterType));
        Assert.False(parameters[0].IsOptional); Assert.False(parameters[1].IsOptional);
        foreach (var parameter in parameters.Skip(2)) { Assert.True(parameter.IsOptional); Assert.Null(parameter.DefaultValue); }
    }
    [Theory]
    [InlineData("{\"type\":\"turn.failed\",\"error\":{\"message\":\"account failure\"}}")]
    [InlineData("{\"type\":\"error\",\"message\":\"provider failure\"}")]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("{\"type\":\"turn.completed\"}")]
    public void ExitZeroWithoutSuccessfulAiReportFails(string line)
    {
        var evaluator = Evaluator(); Observe(evaluator, line);
        Assert.Equal("failed", Complete(evaluator).GetProperty("status").GetString());
    }
    [Fact]
    public void ErrorAfterSuccessWinsAndReceiptOmitsPrivateBody()
    {
        var evaluator = Evaluator(); Success(evaluator);
        Observe(evaluator, "{\"type\":\"turn.failed\"}");
        var receipt = Complete(evaluator);
        Assert.Equal("failed", receipt.GetProperty("status").GetString());
        Assert.DoesNotContain("SECRET_REPORT_BODY", receipt.ToString());
    }
    [Theory]
    [InlineData("{\"type\":\"item.completed\",\"item\":{\"type\":\"error\",\"message\":\"SECRET_ITEM_ERROR\"}}", false)]
    [InlineData("{\"type\":\"item.completed\",\"item\":{\"type\":\"error\",\"message\":\"SECRET_ITEM_ERROR\"}}", true)]
    [InlineData("{\"type\":\"item.completed\",\"item\":{\"type\":\"mcp_tool_call\",\"status\":\"failed\",\"error\":{\"message\":\"SECRET_ITEM_ERROR\"}}}", false)]
    [InlineData("{\"type\":\"item.completed\",\"item\":{\"type\":\"mcp_tool_call\",\"status\":\"failed\",\"error\":{\"message\":\"SECRET_ITEM_ERROR\"}}}", true)]
    [InlineData("{\"type\":\"item.updated\",\"item\":{\"type\":\"mcp_tool_call\",\"error\":{\"message\":\"SECRET_ITEM_ERROR\"}}}", false)]
    [InlineData("{\"type\":\"item.updated\",\"item\":{\"type\":\"mcp_tool_call\",\"error\":{\"message\":\"SECRET_ITEM_ERROR\"}}}", true)]
    public void ExplicitItemErrorWinsBeforeOrAfterSuccessfulCompletion(string line, bool afterCompletion)
    {
        // Pure evaluator fixture: no directories, files, processes or model calls are created.
        var evaluator = Evaluator(root: @"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\jev-item-error-tests");
        if (afterCompletion) Success(evaluator);
        Observe(evaluator, line);
        if (!afterCompletion) Success(evaluator);
        var receipt = Complete(evaluator);
        Assert.Equal("failed", receipt.GetProperty("status").GetString());
        Assert.Equal("ai_failed", receipt.GetProperty("failureCode").GetString());
        Assert.True(receipt.GetProperty("terminalObserved").GetBoolean());
        Assert.True(receipt.GetProperty("reportObserved").GetBoolean());
        Assert.Equal(0, receipt.GetProperty("processExitCode").GetInt32());
        Assert.DoesNotContain("SECRET_ITEM_ERROR", receipt.ToString());
        Assert.DoesNotContain("SECRET_REPORT_BODY", receipt.ToString());
    }
    [Theory]
    [InlineData("{\"type\":\"item.completed\",\"item\":{\"type\":\"command_execution\",\"status\":\"failed\",\"exit_code\":17}}")]
    [InlineData("{\"type\":\"item.completed\",\"item\":{\"type\":\"mcp_tool_call\",\"status\":\"completed\",\"error\":null}}")]
    [InlineData("{\"type\":\"item.completed\",\"item\":{\"type\":\"command_execution\",\"status\":\"completed\",\"exit_code\":0}}")]
    [InlineData("{\"type\":\"item.completed\",\"item\":{\"type\":\"reasoning\",\"text\":\"PRIVATE_REASONING\"}}")]
    public void RecoverableToolResultOrNormalItemDoesNotOverrideSuccessfulAiCompletion(string line)
    {
        var evaluator = Evaluator(root: @"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks\jev-item-error-tests");
        Observe(evaluator, line); Success(evaluator);
        var receipt = Complete(evaluator);
        Assert.Equal("succeeded", receipt.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, receipt.GetProperty("failureCode").ValueKind);
        Assert.DoesNotContain("PRIVATE_REASONING", receipt.ToString());
        Assert.DoesNotContain("SECRET_REPORT_BODY", receipt.ToString());
    }
    [Fact]
    public void TypedSuccessReceiptCarriesOnlyBoundIdentityAndEvidence()
    {
        var evaluator = Evaluator(); Success(evaluator);
        var receipt = Complete(evaluator);
        Assert.Equal(1, receipt.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("owned-job", receipt.GetProperty("executionId").GetString());
        Assert.Equal("gpt-6.1-sol", receipt.GetProperty("requestedModel").GetString());
        Assert.Equal("succeeded", receipt.GetProperty("status").GetString());
        Assert.True(receipt.GetProperty("terminalObserved").GetBoolean());
        Assert.True(receipt.GetProperty("reportObserved").GetBoolean());
        Assert.DoesNotContain("SECRET_REPORT_BODY", receipt.ToString());
    }
    [Theory]
    [InlineData("cancelled", 0)]
    [InlineData("timed_out", 0)]
    [InlineData("output_limit", 0)]
    [InlineData("failed", 0)]
    [InlineData("succeeded", 7)]
    public void PhysicalFailureCancellationOrLimitsCannotBeOverridden(string state, int exit)
    {
        var evaluator = Evaluator(); Success(evaluator);
        Assert.Equal("failed", Complete(evaluator, exit, state).GetProperty("status").GetString());
    }
    [Theory]
    [InlineData("{\"state\":\"failed\",\"exitCode\":1,\"reportPresent\":false,\"model\":\"gpt-6.1-sol\"}")]
    [InlineData("{\"state\":\"completed\",\"exitCode\":0,\"reportPresent\":false,\"model\":\"gpt-6.1-sol\"}")]
    [InlineData("{\"state\":\"completed\",\"exitCode\":0,\"reportPresent\":true,\"model\":\"gpt-6-astra\"}")]
    public void ExplicitWrapperRequiresSemanticSuccessReportAndRequestedModel(string line)
    {
        var evaluator = Evaluator("wrapper-json-v1"); Observe(evaluator, line);
        Assert.Equal("failed", Complete(evaluator).GetProperty("status").GetString());
    }
    [Fact]
    public void OversizedDeclaredAiLineFailsClosed()
    {
        var evaluator = Evaluator(); Observe(evaluator, new string('x', 70000)); Success(evaluator);
        Assert.Equal("failed", Complete(evaluator).GetProperty("status").GetString());
    }
    [Fact]
    public void ExistingRequiredReportFailsPreflightWithoutChangingIt()
    {
        var root = Path.Combine(Path.GetTempPath(), "ai-receipt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root); var report = Path.Combine(root, "report.txt");
        try
        {
            File.WriteAllText(report, "old private report");
            var evaluator = Evaluator(report: "report.txt", root: root);
            Assert.NotNull(evaluator.GetType().GetProperty("PreflightFailure")!.GetValue(evaluator));
            Success(evaluator);
            Assert.Equal("failed", Complete(evaluator).GetProperty("status").GetString());
            Assert.Equal("old private report", File.ReadAllText(report));
        }
        finally { File.Delete(report); Directory.Delete(root); }
    }
    [Theory]
    [InlineData("../escaped.txt")]
    [InlineData("Desktop/report.txt")]
    public void RequiredReportCannotEscapeOrTargetDesktop(string report)
    {
        var evaluator = Evaluator(report: report);
        Assert.NotNull(evaluator.GetType().GetProperty("PreflightFailure")!.GetValue(evaluator));
    }
    [Fact]
    public void NewRequiredReportProducesHashNotReportContents()
    {
        var root = Path.Combine(Path.GetTempPath(), "ai-receipt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root); var report = Path.Combine(root, "report.txt");
        try
        {
            var evaluator = Evaluator(report: "report.txt", root: root);
            File.WriteAllText(report, "NEW_SECRET_ARTIFACT");
            // Windows creation/write timestamps may lag precise UtcNow by <1ms.
            // Set this owned fixture's known-fresh timestamp; do not relax production stale checks.
            File.SetLastWriteTimeUtc(report, DateTime.UtcNow);
            Success(evaluator);
            var receipt = Complete(evaluator);
            Assert.Equal("succeeded", receipt.GetProperty("status").GetString());
            Assert.True(receipt.GetProperty("artifactVerified").GetBoolean());
            Assert.Equal(64, receipt.GetProperty("artifactSha256").GetString()!.Length);
            Assert.DoesNotContain("NEW_SECRET_ARTIFACT", receipt.ToString());
        }
        finally { File.Delete(report); Directory.Delete(root); }
    }
}

public sealed class JevExecutionPolicyTests
{
    private static Type Policy()
    {
        var type = typeof(JobManager).Assembly.GetType("AIControlTower.Services.JevExecutionPolicy");
        Assert.NotNull(type); return type!;
    }
    [Theory]
    [InlineData(null, false)]
    [InlineData("gpt-6.1-sol", false)]
    [InlineData("auto", true)]
    [InlineData("jev-router", true)]
    [InlineData("router", true)]
    [InlineData("gpt-6-astra", true)]
    public void MissingUnverifiedOrForbiddenModelNeverCreatesExecutionContract(string? model, bool verified)
    {
        object?[] args = [model, verified, null, null];
        Assert.False((bool)Policy().GetMethod("TryCreate")!.Invoke(null, args)!);
        Assert.Null(args[2]); Assert.False(string.IsNullOrWhiteSpace((string?)args[3]));
    }
    [Fact]
    public void CallerVerifiedExplicitNonAstraModelCreatesContractAndExplicitArguments()
    {
        object?[] args = ["gpt-6.1-sol", true, null, null];
        Assert.True((bool)Policy().GetMethod("TryCreate")!.Invoke(null, args)!);
        Assert.NotNull(args[2]);
        var argv = ((IEnumerable<string>)Policy().GetMethod("BuildArguments")!.Invoke(null, ["fixture.mjs", "gpt-6.1-sol"])!).ToArray();
        Assert.Contains("--model", argv); Assert.Contains("gpt-6.1-sol", argv);
        Assert.Contains("--json", argv); Assert.Contains("workspace-write", argv);
        Assert.DoesNotContain("jev-router", argv);
        Assert.DoesNotContain("--dangerously-bypass-approvals-and-sandbox", argv);
    }
}
