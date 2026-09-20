namespace AIControlTower.Services;

public static class LocalExecutionPolicy
{
    public static readonly bool AllowLocalJevExecution = false;
    public const string LocalJevDisabledMessage = "사용자 OpenAI 계정 사용 방침에 따라 로컬 Jev 실행이 비활성화되었습니다.";
}
