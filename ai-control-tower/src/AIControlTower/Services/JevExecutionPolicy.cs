using System.Text.RegularExpressions;

namespace AIControlTower.Services;

/// <summary>Caller-supplied verified evidence only; model cache visibility is not execution verification.</summary>
public static class JevExecutionPolicy
{
    public static bool IsExplicitAllowedModel(string? model) => model is not null
        && Regex.IsMatch(model, @"^gpt-[0-9][A-Za-z0-9._:-]{0,95}$", RegexOptions.CultureInvariant)
        && !model.Contains("astra", StringComparison.OrdinalIgnoreCase)
        && !model.Contains("router", StringComparison.OrdinalIgnoreCase)
        && !model.Equals("auto", StringComparison.OrdinalIgnoreCase);
    public static bool TryCreate(string? model, bool accountExecutionVerified, out AiCompletionContract? contract, out string reason)
    {
        contract = null;
        if (!IsExplicitAllowedModel(model)) { reason = "명시적 비-Astra 모델이 필요합니다. auto/router 자동 선택은 허용되지 않습니다."; return false; }
        if (model != "gpt-6.1-sol") { reason = "해당 명시 모델은 현재 앱의 지원 검증 목록에 없습니다. 실행하지 않습니다."; return false; }
        if (!accountExecutionVerified) { reason = "이 계정의 해당 모델 실행 지원 검증이 필요합니다. 모델 목록 표시는 실행 성공 증거가 아닙니다."; return false; }
        contract = new("codex-jsonl-v1", model!); reason = ""; return true;
    }
    public static IReadOnlyList<string> BuildArguments(string package, string model)
    {
        if (!IsExplicitAllowedModel(model) || model != "gpt-6.1-sol") throw new ArgumentException("known explicit non-Astra model required", nameof(model));
        return [package, "exec", "--model", model, "--json", "--sandbox", "workspace-write", "-c", "approval_policy=\"never\"", "-"];
    }
}
