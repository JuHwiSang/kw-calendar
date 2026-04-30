# AGENTS.md

이 레포에서 작업하는 AI 에이전트를 위한 가이드. 사람 기여자용 문서는 [CONTRIBUTING.md](CONTRIBUTING.md)를 참고하고, 이 문서는 그 위에 에이전트가 반복적으로 틀리기 쉬운 부분과 프로젝트 맥락만 덧붙인다.

## 프로젝트 한눈에

**KW-Calendar** — 광운대 학생용 일정 통합 데스크톱 앱. 학교 홈페이지/공지사항/인스타그램 등에 흩어진 일정을 서버가 크롤링·LLM 파싱·정규화해서 DB에 저장하고, WinForms 클라이언트가 캘린더 UI로 보여준다.

### 컴포넌트

- **클라이언트** — C# / WinForms (.NET 10). 캘린더 뷰, 상세보기, 즐겨찾기, 알림 담당.
- **서버** — Supabase (Edge Functions + Postgres). 크롤링, LLM 파싱, 일정 API 제공.
- **클라이언트 DB** — SQLite (EF Core). 서버 동기화본 + 사용자 로컬 상태(즐겨찾기 등).
- **서버 DB** — Postgres. `source_items`(원본) / `events`(파싱본) / `categories` 3테이블. 사용자 상태는 저장하지 않음.
- **LLM** — Gemini Flash 기본. 공지 → 구조화된 일정 JSON 변환, 중복 체크.

### 기술 스택 / 버전 고정

- **.NET 10**, **Visual Studio 2026** — 하위 버전 금지. `KW-Calendar.csproj`가 `net10.0-windows`를 타겟팅한다.
- **WinForms 강제** — 과제 제약조건. WPF/MAUI/Avalonia 같은 대안 제안 금지.
- Edge Functions는 **Deno / TypeScript**.
- 테스트는 **xUnit**.

## 아키텍처 — MVP + Service

클라이언트는 **Model / View / Presenter / Service** 4계층으로 엄격히 분리한다. 폴더 구조가 그대로 계층 구조다:

- `KW-Calendar/Models/` — 데이터 구조, DTO, Supabase 응답 매핑
- `KW-Calendar/Views/` — `Form` / `UserControl`. UI와 입력 전달만. **비즈니스 로직 금지**
- `KW-Calendar/Presenters/` — View 이벤트 ↔ Service 호출 중계. **인터페이스에만 의존**
- `KW-Calendar/Services/` — 비즈니스 로직 + 외부 의존성(Supabase, SQLite, 알림 등). View를 모름

의존 방향은 **Presenter가 `IView`와 `IService` 인터페이스에 의존**하고, View / Service 구현체가 그 인터페이스를 구현하는 형태다(의존성 역전). View는 Presenter 구현체를 몰라야 하며, Presenter는 구체 타입이 아닌 인터페이스에만 의존한다. 이벤트 흐름(View 입력 → Presenter → Service)과 의존 방향은 다른 얘기다. Service 계층은 단위 테스트 대상이므로 반드시 인터페이스(`ICalendarService` 등)로 추상화한다.

## 반드시 지켜야 할 것들

### WinForms
- **`*.Designer.cs`를 직접 편집하지 않는다.** Visual Studio 디자이너로만 수정. `.resx`, `.Designer.cs`는 한 명이 한 Form만 만지는 것이 원칙 — 동시 수정 시 머지 충돌이 거의 반드시 난다.
- UI 변경 태스크를 받으면: Form 자체의 코드 변경이 아닌 한 디자이너 편집이 필요하다는 점을 사용자에게 알리고, 필요하면 사용자에게 디자이너 조작을 요청하거나 최소한의 코드 편집만 제안한다.

### 시크릿 / 설정
- `appsettings.json`은 커밋된다. C# 클라이언트는 Anon Key(공개 키)만 사용하므로 시크릿이 없다.
- 설정 값은 `appsettings.json`에 직접 채워 넣는다. 코드에 하드코딩하지 않는다.
- 설정 키 형태(`Supabase:Url`, `Supabase:AnonKey` 등)로만 참조한다.
- Edge Functions 시크릿은 GitHub Secrets에 등록, 로컬에서는 `supabase/functions/.env`(gitignore) 사용.
- 새 Edge Functions 시크릿 추가 시 `.github/workflows/release.yml`의 `supabase secrets set` 스텝에도 해당 시크릿을 추가한다.

### 시스템 파일 수정 금지
다음 파일은 작업 요구사항이 **명시적으로** 그 파일을 가리키지 않는 한 건드리지 않는다:
- `.gitignore`
- `.editorconfig`
- `*.csproj` (라이브러리 추가는 별도 티켓 필요)

라이브러리 추가 필요성이 느껴지면 사용자에게 먼저 확인한다.

### 포맷 / 네이밍
- `.editorconfig`를 따른다. 줄바꿈은 **CRLF** (`.editorconfig` 211번 라인 기준).
- 인터페이스는 `I` 접두사 (`ICalendarService`).
- 클래스/메서드/프로퍼티/상수: PascalCase. 로컬 변수/파라미터: camelCase.

### 커밋 / 브랜치
- 커밋 메시지 prefix: `feat:` / `fix:` / `refactor:` / `docs:` / `chore:` / `test:`.
- PR 머지는 **Squash merge**.
- **사용자가 명시적으로 요청하지 않으면 커밋/푸시하지 않는다.**

## 요구사항 맥락 (반복 실수 방지용)

명세 전문은 Linear에 있지만, 에이전트가 자주 틀리는 포인트만 추린다:

- **추천 기능은 제거됨** (2026-04-12). 유저 정보 입력(학년·학과 등) 관련 로직을 추가하지 않는다. 분류는 카테고리 기반만.
- **로그인/계정 기능 없음**. 사용자 상태는 클라이언트 DB에만 존재.
- **서버 DB와 클라이언트 DB 스키마가 다르다.** `events`는 양쪽 다 있지만 클라이언트 쪽에만 `is_favorited` 같은 개인 상태 컬럼이 있다. 동기화 시 서버 응답을 클라이언트 스키마로 매핑해야 한다.
- **한 `source_item`에서 여러 `event`가 나올 수 있다** (신청기간 + 행사기간 등). 1:N 관계.
- **카테고리 8종 고정** — 학사/수업, 행사, 장학금/등록금/지원금, 취업/창업/경력, 국제/교환/유학생, 비교과/자기계발, 생활/복지/시설, 봉사. 마이그레이션에 하드코딩.
- **알림은 즐겨찾기한 일정에만** 울린다. 기본 채널은 Windows 기본 알림. `CommunityToolkit.WinUI.Notifications` 패키지가 이미 추가되어 있다.
- **일정에 "신청기간" 컬럼은 없다.** 신청기간은 별도 `event`로 분리해 저장한다.
- **외부 소스는 3개만** — 소프트웨어학부 인스타, 인공지능융합대학 인스타, 광운대 공지사항, 학사일정.

## 테스트

- xUnit, `KW-Calendar.Tests/` 프로젝트.
- **Service 계층은 가능한 한 단위 테스트를 작성한다.** View / Presenter는 필수 아님.
- 테스트 실행: `dotnet test` 또는 VS 테스트 탐색기.

## 빌드 / 실행

- 빌드: VS에서 `Ctrl+Shift+B`, 또는 `dotnet build`.
- 실행: VS에서 `F5`, 또는 `dotnet run --project KW-Calendar`.
- Supabase 로컬: `supabase start` (Docker 필요) / `supabase db reset` (마이그레이션 재적용).
- Edge Function 로컬: `supabase functions serve <name>`.
    - 이 명령어는 포그라운드에서 돌아가며 콘솔 점유적임. 따라서 에이전트가 직접 이를 실행하면 콘솔점유에 의해 무한루프 문제가 생기니, 유저에게 별도의 콘솔에서 미리 실행해달라고 요청해야함.

## 에이전트용 체크리스트

작업 전:
- [ ] 어느 계층(Model/View/Presenter/Service)의 파일인지 먼저 판단한다.
- [ ] Form을 건드릴 태스크라면 `.Designer.cs` 수정이 필요한지 먼저 확인한다.

작업 후 / PR 전:
- [ ] 시크릿이 코드나 설정 파일에 하드코딩되지 않았는지.
- [ ] 새 Service는 인터페이스로 추상화되어 있고, Presenter가 구체 타입이 아닌 인터페이스에 의존하는지.
- [ ] 빌드가 통과하는지 (`dotnet build`).
- [ ] Service 변경이면 xUnit 테스트를 추가/갱신했는지.
- [ ] 커밋/푸시는 사용자가 명시적으로 요청할 때만.
