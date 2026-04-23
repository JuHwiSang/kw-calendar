# Contributing to KW-Calendar

4인 팀 학기 프로젝트용 기여 가이드입니다.

> 이 문서의 규칙에 이견이 있거나 변경이 필요하면 회의에서 논의 후 수정합니다.

## 목차

1. [기술 스택](#기술-스택)
2. [개발 환경 셋업](#개발-환경-셋업)
3. [변경 제출 플로우](#변경-제출-플로우)
4. [C# / WinForms](#c--winforms)
5. [Supabase](#supabase)
6. [빌드 / CI](#빌드--ci)
7. [공통 규칙](#공통-규칙)

---

## 기술 스택

- **언어/프레임워크**: C# / WinForms (.NET 10)
- **IDE**: Visual Studio 2026 (최소 버전 고정 — 하위 버전 사용 금지)
- **백엔드**: Supabase
- **티켓 관리**: Linear

---

## 개발 환경 셋업

### 1. 사전 요구사항

- **Visual Studio 2026** — 설치 시 ".NET 데스크톱 개발" 워크로드 포함
- **.NET 10 SDK** — VS 설치에 포함됨
- **[Supabase CLI](https://supabase.com/docs/guides/local-development/cli/getting-started)**
- **Deno** (Edge Functions 개발 시) — [설치](https://docs.deno.com/runtime/getting_started/installation/)
- **Docker** (Supabase 로컬 실행에 필요)
- **Git**

### 2. 레포 클론 및 빌드

```bash
git clone <repo-url>
cd KW-Calendar
```

`KW-Calendar.slnx`를 Visual Studio 2026에서 연 뒤 `Ctrl+Shift+B`로 빌드합니다.

### 3. Supabase 로컬 환경

레포의 [supabase/](supabase/) 폴더에 `config.toml`이 있습니다. 처음 셋업 시:

```bash
supabase start      # 로컬 인스턴스 기동
supabase db reset   # 마이그레이션 적용
```

### 4. 시크릿 등록

`KW-Calendar/appsettings.Development.json`을 생성하고 팀 내부 채널에서 공유한 값을 넣습니다.

```json
{
  "Supabase": {
    "Url": "<SUPABASE_URL>",
    "AnonKey": "<SUPABASE_ANON_KEY>"
  }
}
```

### 5. 실행

VS에서 `F5`, 또는:

```bash
dotnet run --project KW-Calendar
```

---

## 변경 제출 플로우

작업을 시작할 때 아래 순서대로 진행합니다.

### 1. 티켓

- 새 기능, 수정 사항은 **Linear 티켓을 발급**하고 할당받아 작업합니다.
- 작업 중 본인 영역과 무관한 버그를 발견하면, **Linear에 티켓만 생성**하고 본인 작업을 계속합니다.
- 오타, 포맷, 주석 등 사소한 수정은 티켓 없이 PR만 올려도 됩니다.
- 라이브러리 추가는 영향 범위가 크므로 **티켓을 발급**합니다.

### 2. 브랜치

**Github flow**를 사용합니다. `main`은 항상 빌드 가능한 상태로 유지.

브랜치 이름은 Linear의 "Copy branch name"으로 복사한 값을 그대로 사용합니다.

```
juhs2005/kw-12-add-login-form
```

> (결정 필요) 한글 티켓명 깨짐 이슈는 회의에서 논의 필요

### 3. 커밋 메시지

표준적인 git commit prefix를 따릅니다.

```
feat: 캘린더 월간 뷰 추가
fix: 로그인 시 토큰 갱신 실패 수정
refactor: CalendarService 의존성 정리
docs: CONTRIBUTING 업데이트
chore: 라이브러리 버전 업
test: CalendarService 단위 테스트 추가
```

### 4. Pull Request

- **1인 이상 리뷰**를 받고 머지하는 것을 원칙으로 합니다.
- **24시간 내** 아무도 리뷰하지 않으면 본인이 머지해도 됩니다.
- 머지 방식은 **Squash merge**.

리뷰를 권장하는 이유:

- 코드 품질 향상
- 팀원 간 코드 공유 / 학습 효과
- 본인이 놓친 사이드 이펙트 발견

---

## C# / WinForms

### 아키텍처 (MVP + Service)

- **Model**: 데이터 구조, DTO, Supabase 응답 매핑.
- **View**: WinForms `Form` / `UserControl`. UI 표시와 사용자 입력 전달만 담당. 비즈니스 로직 금지.
- **Presenter**: View와 Service를 연결. View 이벤트를 받아 Service 호출, 결과를 View에 반영.
- **Service**: 비즈니스 로직과 외부 의존성(Supabase 등). View를 모름. 테스트 대상.

이렇게 나누는 이유는 (1) View와 로직 분리, (2) 계층화로 변경 영향 범위 축소, (3) Service 단위 테스트 가능.

### 네이밍

- 클래스/메서드/프로퍼티: **PascalCase**
- 로컬 변수/파라미터: camelCase
- 상수: PascalCase
- 인터페이스: `I` 접두사 (예: `ICalendarService`)

### 포맷

- [.editorconfig](.editorconfig)를 따릅니다. 에디터 auto-format on save를 켜두면 대부분 자동으로 맞춰집니다.
- 필요 시 커밋 전 `dotnet format`으로 일괄 정리할 수 있습니다. CI에서 별도 포맷 검증은 하지 않습니다.

### WinForms 작업 규칙

- **`*.Designer.cs` 파일은 직접 수정하지 않습니다.** Visual Studio의 GUI 디자이너로만 수정합니다.
- 동일한 Form/UserControl을 여러 명이 동시에 수정하면 `.Designer.cs`에서 머지 충돌이 발생합니다. **본인이 할당받은 티켓 영역의 Form만** 수정해주세요.

### 테스트

- .NET 표준에 따라 **xUnit**을 사용합니다.
- **Service 계층은 가능한 한 단위 테스트를 작성**합니다.
- View, Presenter는 필수 아님 (WinForms 특성상 어려움).

---

## Supabase

### 스키마 / 마이그레이션

- 스키마 변경은 **Supabase CLI로 마이그레이션 파일 생성 후 PR에 포함**합니다.

```bash
supabase migration new <migration_name>
```

- 다른 팀원의 변경이 로컬 DB에 반영되지 않았다면 `supabase db reset`으로 마이그레이션을 다시 적용합니다.

### Edge Functions

Edge Functions는 Deno / TypeScript로 작성합니다.

- 린트: `deno lint supabase/functions/`
- 테스트: 함수 폴더 안에서 `deno test --allow-all`, 또는 상위에서 돌릴 땐 `deno test --allow-all --config supabase/functions/deno.json supabase/functions/`
- 로컬 실행:

```bash
supabase functions serve <function_name>
```

#### Deno 설정 / 의존성

- `deno.json`은 **`supabase/functions/deno.json` 한 개만** 사용합니다.
- `supabase functions new <name>`을 돌리면 `supabase/functions/<name>/deno.json`이 자동 생성되는데, **생성 직후 삭제**하세요. 함수별 config를 두면 상위 디렉토리 단위 `deno test` / `deno lint`에서 config resolve가 꼬입니다.
- 의존성 추가는 상위 `deno.json`이 있는 위치에서 `deno add jsr:<pkg>` / `deno add npm:<pkg>`를 돌리면 `imports`에 자동 반영됩니다.

---

## 빌드 / CI

빌드와 배포는 **GitHub Actions가 자동으로 처리**합니다. 워크플로우는 [.github/workflows/](.github/workflows/)에 3개:

- [build.yml](.github/workflows/build.yml) — PR / `main` push 시 .NET build + test, 그리고 `supabase/functions/` 대상 `deno lint / test`를 돌립니다.
- [auto-tag.yml](.github/workflows/auto-tag.yml) — `main` push 시 커밋 메시지 prefix 기반 자동 태깅 (`feat:` → minor, `fix:` → patch, `BREAKING CHANGE:` → major). Actions 탭에서 수동 bump도 가능.
- [release.yml](.github/workflows/release.yml) — `v*.*.*` 태그 push 시 WinForms 앱 publish (self-contained + framework-dependent 두 ZIP) → GitHub Releases 업로드 + Supabase DB migration / Edge Functions 배포.

로컬에서는:

- C#: VS에서 `Ctrl+Shift+B` (빌드) / `F5` (실행)
- Edge Functions: `supabase functions serve <function_name>`

수동 배포(`supabase functions deploy`)는 CI 장애 등 비상시에만 사용합니다.

### CI에 등록해야 하는 Secrets / Variables

`release.yml`의 Supabase 배포 step은 현재 placeholder 상태입니다. 실제 배포를 활성화하려면 레포 **Settings → Secrets and variables → Actions**에 아래를 등록한 뒤 `release.yml` 내 주석 처리된 step들을 해제합니다.

| 이름 | 종류 |
|---|---|
| `SUPABASE_ACCESS_TOKEN` | Secret |
| `SUPABASE_PROJECT_ID` | Variable |

`GITHUB_TOKEN`은 Actions가 자동 주입하므로 별도 등록은 필요 없습니다.

---

## 공통 규칙

### 시크릿 관리

- API 키, 시크릿 등은 **절대 커밋하지 않습니다.**
- C# 쪽은 `appsettings.Development.json` ([.gitignore](.gitignore)에 등록됨).
- Supabase Edge Functions 쪽은 Supabase Dashboard의 **Secrets**에 등록, 로컬에서는 `supabase/functions/.env`(gitignore) 사용.

### 시스템 파일

다음 파일은 본인 작업과 직접 관련 없으면 수정하지 않습니다.

- [.gitignore](.gitignore)
- [.editorconfig](.editorconfig)
- `*.csproj` (라이브러리 추가 등 의도적인 경우 제외)
