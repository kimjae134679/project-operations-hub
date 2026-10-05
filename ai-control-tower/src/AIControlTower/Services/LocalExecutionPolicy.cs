namespace AIControlTower.Services;
public static class LocalExecutionPolicy
{
    // Legacy text inbox is never auto-executed. Interactive execution is a per-installation setting.
    public static readonly bool AllowLocalJevExecution = false;
    public const string LocalJevDisabledMessage = "Jev 사용은 허용되어 있습니다. 필요할 때만 Jev 사용 설정을 켜세요. 실행은 사용자가 입력한 작업에 한정하며 기존 텍스트 큐는 자동 실행하지 않습니다.";
}
