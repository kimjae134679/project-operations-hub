# AI Ops Runner

ChatGPT가 `project-operations-hub`에 등록한 작업을 Windows PC에서 찾아 `jev-codex`로 실행하고, 대상 저장소에 브랜치와 Pull Request를 만드는 로컬 실행기입니다.

## 안전 범위

- 제목이 `[AI-RUN]`으로 시작하는 열린 Issue만 처리합니다.
- `project-map.json`에 등록된 저장소와 로컬 경로만 사용합니다.
- 작업 시작 전 대상 저장소가 깨끗한지 확인합니다.
- 기본 브랜치에 직접 커밋하거나 병합하지 않습니다.
- 강제 push, 배포, 삭제, 결제, 비밀값 변경은 수행하지 않습니다.
- 결과는 원래 Issue의 댓글과 대상 저장소의 Pull Request에 남깁니다.

## 준비

Windows에 다음 명령이 있어야 합니다.

```powershell
git --version
gh --version
jev-codex --version
```

GitHub CLI가 없다면 설치합니다.

```powershell
winget install --id GitHub.cli
```

처음 한 번 GitHub에 로그인합니다.

```powershell
gh auth login
```

Jev API 키와 Codex 로그인은 기존 설정을 사용합니다. API 키를 이 저장소에 저장하지 않습니다.

## 프로젝트 등록

`project-map.json`의 `localPath`를 실제 PC 경로와 맞춥니다. 현재 확인된 Threads 경로만 기본 등록되어 있습니다.

## 시험 실행

관리자 권한이 아닌 일반 PowerShell에서 실행할 수 있습니다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\ai-ops-runner\Invoke-AiOpsRunner.ps1 -Once
```

## 자동 실행 설치

2분마다 새 작업을 확인하도록 Windows 작업 스케줄러에 등록합니다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\ai-ops-runner\Install-AiOpsRunner.ps1
```

해제:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\ai-ops-runner\Uninstall-AiOpsRunner.ps1
```

## 작업 형식

GitHub Issue 제목:

```text
[AI-RUN] 작업 제목
```

본문에는 다음 JSON 블록을 넣습니다.

````markdown
```ai-task
{
  "project": "Threads",
  "instruction": "수행할 작업과 완료 조건을 구체적으로 작성",
  "communicationThread": "T-0008-ai-content-monetization"
}
```
````

ChatGPT가 작업을 등록할 때도 같은 형식을 사용합니다.

