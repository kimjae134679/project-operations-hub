# Installation Burden — 설치/운영 부담 분류

후보 도구를 단순히 `좋다/나쁘다`로 보지 않고 **내 PC에 실제로 깔고 유지하기 얼마나 무거운지** 따로 표기합니다.

## 1. 설치 부담 등급

| 표기 | 의미 | 대략적인 특징 |
|---|---|---|
| ⚪ `NONE / REFERENCE` | 설치 불필요 | 문서·목록·학습자료·GitHub 템플릿 |
| 🟢 `LIGHT` | 가벼움 | 단일 CLI/패키지/바이너리, 작은 의존성, 보통 별도 상시 서비스 없음 |
| 🟡 `MEDIUM` | 보통 | Python/Node 환경, 브라우저 런타임, 로컬 서버, Docker 1개 정도, 몇 가지 연동 설정 |
| 🟠 `HEAVY` | 무거움 | 다중 서비스/Docker Compose, 큰 모델·브라우저·DB, 수 GB 이상 저장공간, 장시간 설치/업데이트 가능 |
| 🔴 `VERY_HEAVY` | 매우 무거움 | GPU/CUDA·대형 모델·수십 GB 저장공간·고 RAM·여러 컨테이너·OS 제약이 겹침 |

등급은 **repo 용량이 아니라 실제 사용 부담** 기준입니다. 작은 repo라도 대형 모델을 내려받아야 하면 `HEAVY/VERY_HEAVY`가 될 수 있습니다.

## 2. 별도 표기

설치 부담과 실제 사용 난이도를 분리합니다.

- `SETUP: LOW / MID / HIGH` — 처음 설치·환경 구성 난이도
- `RUNTIME: LOW / MID / HIGH / GPU-HIGH` — 실행 중 CPU/RAM/GPU/디스크 부담
- `CLOUD-LIGHT` — 로컬 설치 대신 hosted API/MCP를 쓰면 가벼운 경우
- `SELFHOST-HEAVY` — SDK는 가볍지만 직접 서버를 띄우면 무거운 경우

예:

```text
Firecrawl
- Cloud SDK: 🟢 LIGHT
- Self-host: 🟠 HEAVY

Open WebUI
- 외부 API만 연결: 🟡 MEDIUM
- Ollama/CUDA/로컬 모델 포함: 🟠 HEAVY
```

## 3. 🔴 VERY_HEAVY 별도 목록

### ComfyUI — 로컬 생성 모델 사용
- 등급: 🔴 `VERY_HEAVY`
- 이유: 앱 설치 자체는 Desktop/Portable로 쉬울 수 있지만 실제 체크포인트·VAE·LoRA·비디오 모델 등이 매우 커지고, PyTorch/CUDA/GPU 드라이버·VRAM 조건이 따라옵니다.
- 공식 README도 checkpoint를 `huge ckpt/safetensors files`로 설명하며 Windows Portable은 PyTorch/CUDA를 함께 포함합니다.
- 이미지 몇 모델만 제한적으로 쓰면 부담을 줄일 수 있지만, 여러 비디오/3D 워크플로까지 확장하면 저장공간과 VRAM 부담이 급격히 커집니다.

### vLLM — 로컬 LLM serving
- 등급: 🔴 `VERY_HEAVY`
- 이유: `pip/uv install` 자체보다 실제 LLM 모델 파일, GPU 메모리, CUDA/HIP 계층, quantization/kernel 호환성이 핵심 부담입니다.
- 고성능 serving/parallelism이 목적이라 단순 개인용 로컬 챗보다 서버 성격이 강합니다.

### RAGFlow — self-host
- 등급: 🔴 `VERY_HEAVY`
- 공식 self-host 최소 요구사항: CPU 4 cores+, RAM 16 GB+, Disk 50 GB+, Docker/Compose.
- DB/검색/스토리지 등 여러 서비스가 함께 움직이는 전체 RAG 플랫폼이라 단순 Python 라이브러리와 다릅니다.

### DeepSeek 로컬 대형 모델
- 등급: 🔴 `VERY_HEAVY` 가능성이 큼
- `deepseek-ai`는 하나의 설치 프로그램이 아니라 여러 모델/repo를 가진 조직입니다.
- 어떤 모델/quantization을 쓰는지에 따라 수 GB부터 서버급 자원까지 차이가 매우 큽니다.

## 4. 🟠 HEAVY 후보

- `Ollama` — 프로그램 설치는 쉽지만 실제 모델 다운로드/VRAM/디스크가 부담의 대부분
- `Dify self-host` — Docker Compose 기반 플랫폼. 공식 최소 CPU 2 cores / RAM 4 GiB이지만 기능을 붙일수록 DB·RAG·모델 provider 관리가 늘어남
- `Firecrawl self-host` — 클라우드 SDK는 가볍지만 브라우저/크롤링 인프라를 직접 운영하면 무거움
- `Open WebUI + bundled Ollama/CUDA` — UI 자체보다 로컬 모델 계층이 무거움
- `llama.cpp + 큰 GGUF 모델` — 실행기는 비교적 단순하지만 모델 파일·GPU offload/빌드 옵션 부담이 큼
- `Transformers + PyTorch + 큰 모델` — 라이브러리만 설치하면 중간급이지만 실제 모델/학습·추론 환경은 무거워질 수 있음
- `nanoGPT` 대규모 학습 — 학습 규모에 따라 GPU/시간 부담이 크게 증가
- `LobeHub self-host` — Docker/DB/여러 Agent·provider를 묶어 운영하면 중간 이상

## 5. 설치 정책

1. 후보 카탈로그의 모든 실행 도구는 가능하면 설치 부담 등급을 같이 기록합니다.
2. Cloud/API 사용과 Self-host 사용의 부담이 크게 다르면 **두 등급을 따로** 적습니다.
3. 🔴 `VERY_HEAVY`는 다른 가벼운 후보와 같은 줄에서 `그냥 설치 후보`로 취급하지 않습니다.
4. 모델 파일, CUDA, Docker image, 브라우저 runtime처럼 실제 용량을 키우는 요소를 별도로 적습니다.
5. 설치됨(`INSTALLED`)과 실제 채택됨(`ACTIVE/PROJECT`)은 다른 상태입니다.
6. 실제 채택 상태의 원본은 `01_CONTROL/TOOLS.md`, 실제 설치 위치/버전의 원본은 `01_CONTROL/AI_INSTALLATIONS.md`입니다.

마지막 정리: **2026-09-15**
