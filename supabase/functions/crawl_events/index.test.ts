import { assertEquals } from "@std/assert"
import { crawlKwNotice, crawlKwAcademic, crawlInstagram, crawlInstagramAccount } from "./crawlers.ts"

// ---------------------------------------------------------------------------
// Shared mock helpers
// ---------------------------------------------------------------------------

function createMockDb(existingSourceIds = new Set<string>()) {
  const upsertLog: Record<string, unknown>[] = []
  let _lastEqVal = ""
  const selectChain: any = {
    eq(_col: string, val: string) { _lastEqVal = val; return selectChain },
    maybeSingle: () => Promise.resolve({
      data: existingSourceIds.has(_lastEqVal) ? { id: 1 } : null,
      error: null,
    }),
  }
  const db: any = {
    from(_table: string) {
      return {
        select(_fields: string) { return selectChain },
        upsert(payload: Record<string, unknown>) {
          upsertLog.push(payload)
          return Promise.resolve({ error: null })
        },
      }
    },
  }
  return { db, upsertLog }
}

// ---------------------------------------------------------------------------
// crawlKwNotice
// ---------------------------------------------------------------------------

Deno.test("crawlKwNotice: saves new notice items", async () => {
  const listHtml = `
    <div class="board-list-box">
      <ul>
        <li>
          <a href="/ko/life/notice.jsp?BoardMode=view&DUID=52518">
            <strong class="category">[장학]</strong> 공지 제목
          </a>
        </li>
      </ul>
    </div>`

  const detailHtml = `<div class="board-view-box"><p>상세 내용</p></div>`

  let callCount = 0
  const mockFetch = (_url: string): Promise<Response> => {
    callCount++
    if (callCount === 1) return Promise.resolve(new Response(listHtml, { status: 200 }))
    if (callCount === 2) return Promise.resolve(new Response(detailHtml, { status: 200 }))
    return Promise.resolve(new Response("", { status: 200 }))
  }

  const { db, upsertLog } = createMockDb()
  await crawlKwNotice({ db, fetchFn: mockFetch as typeof fetch, throttleMs: 0 })

  assertEquals(upsertLog.length, 1)
  assertEquals((upsertLog[0] as any).source_type, "kw_notice")
  assertEquals((upsertLog[0] as any).source_id, "52518")
})

Deno.test("crawlKwNotice: skips notice when content area is missing", async () => {
  const listHtml = `
    <div class="board-list-box">
      <ul>
        <li>
          <a href="/ko/life/notice.jsp?BoardMode=view&DUID=99991">공지</a>
        </li>
      </ul>
    </div>`

  const detailHtml = `<div class="no-matching-selector"><p>내용</p></div>`

  let callCount = 0
  const mockFetch = (): Promise<Response> => {
    callCount++
    if (callCount === 1) return Promise.resolve(new Response(listHtml, { status: 200 }))
    return Promise.resolve(new Response(detailHtml, { status: 200 }))
  }

  const { db, upsertLog } = createMockDb()
  await crawlKwNotice({ db, fetchFn: mockFetch as typeof fetch, throttleMs: 0 })

  assertEquals(upsertLog.length, 0)
})

Deno.test("crawlKwNotice: skips already-saved notice items", async () => {
  const listHtml = `
    <div class="board-list-box">
      <ul>
        <li>
          <a href="/ko/life/notice.jsp?BoardMode=view&DUID=52518">공지</a>
        </li>
      </ul>
    </div>`

  const mockFetch = (): Promise<Response> =>
    Promise.resolve(new Response(listHtml, { status: 200 }))

  const { db, upsertLog } = createMockDb(new Set(["52518"]))
  await crawlKwNotice({ db, fetchFn: mockFetch as typeof fetch, throttleMs: 0 })

  assertEquals(upsertLog.length, 0)
})

Deno.test("crawlKwNotice: stops pagination when page has no new items", async () => {
  const listHtml = `
    <div class="board-list-box">
      <ul>
        <li>
          <a href="/ko/life/notice.jsp?BoardMode=view&DUID=99999">공지</a>
        </li>
      </ul>
    </div>`

  let fetchCallCount = 0
  const mockFetch = (): Promise<Response> => {
    fetchCallCount++
    return Promise.resolve(new Response(listHtml, { status: 200 }))
  }

  const { db } = createMockDb(new Set(["99999"]))
  await crawlKwNotice({ db, fetchFn: mockFetch as typeof fetch, throttleMs: 0 })

  // Stops after page 1 (1 list fetch, 0 detail fetches)
  assertEquals(fetchCallCount, 1)
})

// ---------------------------------------------------------------------------
// crawlKwNotice — error paths
// ---------------------------------------------------------------------------

Deno.test("crawlKwNotice: retries and saves item after initial HTTP 500", async () => {
  const listHtml = `
    <div class="board-list-box">
      <ul>
        <li>
          <a href="/ko/life/notice.jsp?BoardMode=view&DUID=11111">공지</a>
        </li>
      </ul>
    </div>`
  const detailHtml = `<div class="board-view-box"><p>내용</p></div>`

  let callCount = 0
  const mockFetch = (): Promise<Response> => {
    callCount++
    // 첫 번째 목록 fetch는 500, retry 후 성공
    if (callCount === 1) return Promise.resolve(new Response("", { status: 500 }))
    if (callCount === 2) return Promise.resolve(new Response(listHtml, { status: 200 }))
    if (callCount === 3) return Promise.resolve(new Response(detailHtml, { status: 200 }))
    return Promise.resolve(new Response("", { status: 200 }))
  }

  const { db, upsertLog } = createMockDb()
  await crawlKwNotice({ db, fetchFn: mockFetch as typeof fetch, throttleMs: 0, retryBaseDelayMs: 0 })

  assertEquals(upsertLog.length, 1)
  assertEquals((upsertLog[0] as any).source_id, "11111")
})

Deno.test("crawlKwNotice: skips page and continues after exhausting all retries", async () => {
  // page 1은 항상 500 (maxRetries=3, 총 4회 시도 후 throw)
  // continue 시 page 2 목록을 1회 이상 추가 fetch → fetchCallCount > 4
  // break 시 page 2 fetch 없이 종료 → fetchCallCount === 4
  const emptyListHtml = `<div class="board-list-box"><ul></ul></div>`

  let fetchCallCount = 0
  const mockFetch = (url: string): Promise<Response> => {
    fetchCallCount++
    if (url.includes("tpage=1")) return Promise.resolve(new Response("", { status: 500 }))
    return Promise.resolve(new Response(emptyListHtml, { status: 200 }))
  }

  const { db } = createMockDb()
  await crawlKwNotice({ db, fetchFn: mockFetch as typeof fetch, throttleMs: 0, retryBaseDelayMs: 0 })

  // page 1: 4회 시도 실패, page 2: emptyListHtml → break = 총 5회
  assertEquals(fetchCallCount, 5)
})

Deno.test("crawlKwNotice: skips page and continues on timeout (AbortError)", async () => {
  const emptyListHtml = `<div class="board-list-box"><ul></ul></div>`

  let fetchCallCount = 0
  const mockFetch = (url: string): Promise<Response> => {
    fetchCallCount++
    if (url.includes("tpage=1")) return Promise.reject(new DOMException("The operation was aborted.", "AbortError"))
    return Promise.resolve(new Response(emptyListHtml, { status: 200 }))
  }

  const { db } = createMockDb()
  await crawlKwNotice({ db, fetchFn: mockFetch as typeof fetch, throttleMs: 0, retryBaseDelayMs: 0 })

  // page 1: 4회 시도 실패, page 2: emptyListHtml → break = 총 5회
  assertEquals(fetchCallCount, 5)
})

Deno.test("crawlKwNotice: saves all items when skipExisting=false (full crawl mode)", async () => {
  const listHtml = `
    <div class="board-list-box">
      <ul>
        <li>
          <a href="/ko/life/notice.jsp?BoardMode=view&DUID=52518">공지</a>
        </li>
      </ul>
    </div>`
  const detailHtml = `<div class="board-view-box"><p>내용</p></div>`

  let callCount = 0
  const mockFetch = (): Promise<Response> => {
    callCount++
    if (callCount === 1) return Promise.resolve(new Response(listHtml, { status: 200 }))
    if (callCount === 2) return Promise.resolve(new Response(detailHtml, { status: 200 }))
    return Promise.resolve(new Response("", { status: 200 }))
  }

  // source_id "52518"이 이미 존재 — skipExisting=true면 저장 안 함
  const { db, upsertLog } = createMockDb(new Set(["52518"]))
  await crawlKwNotice({ db, fetchFn: mockFetch as typeof fetch, throttleMs: 0, skipExisting: false, retryBaseDelayMs: 0 })

  assertEquals(upsertLog.length, 1)
})

// ---------------------------------------------------------------------------
// crawlKwAcademic
// ---------------------------------------------------------------------------

Deno.test("crawlKwAcademic: saves single-day and range-date schedule items", async () => {
  const html = `
    <div class="schedule-this-monthlist">
      <ul>
        <li><strong>05.01(금) </strong><p>1학기 수업일수 60일</p></li>
        <li><strong>05.04(월)  ~ 05.15(금)</strong><p>졸업종합시험</p></li>
      </ul>
    </div>`

  const mockFetch = (): Promise<Response> =>
    Promise.resolve(new Response(html, { status: 200 }))

  const { db, upsertLog } = createMockDb()
  await crawlKwAcademic(db, mockFetch as typeof fetch)

  // 2 items × 6 months = 12 upserts
  assertEquals(upsertLog.length, 12)
  assertEquals((upsertLog[0] as any).source_type, "kw_academic")
})

Deno.test("crawlKwAcademic: generates correct 6-month targets with year boundary in November", async () => {
  // 11월 기준: 11, 12, 1, 2, 3, 4 — 연도 경계(12→1) 처리 검증
  const months: number[] = []
  const mockFetch = (_url: string, options?: RequestInit): Promise<Response> => {
    const body = (options as any)?.body as string ?? ""
    const params = new URLSearchParams(body)
    const month = Number(params.get("sm"))
    if (!isNaN(month)) months.push(month)
    return Promise.resolve(new Response(`<div class="schedule-this-monthlist"><ul></ul></div>`, { status: 200 }))
  }

  const { db } = createMockDb()
  await crawlKwAcademic(db, mockFetch as typeof fetch, new Date(2026, 10, 1))

  assertEquals(months, [11, 12, 1, 2, 3, 4])
})

Deno.test("crawlKwAcademic: sourceId includes index and is space-free", async () => {
  const html = `
    <div class="schedule-this-monthlist">
      <ul>
        <li><strong>05.04(월)  ~ 05.15(금)</strong><p>졸업 종합시험</p></li>
        <li><strong>05.04(월)  ~ 05.15(금)</strong><p>졸업 종합시험</p></li>
      </ul>
    </div>`

  const mockFetch = (): Promise<Response> =>
    Promise.resolve(new Response(html, { status: 200 }))

  const { db, upsertLog } = createMockDb()
  await crawlKwAcademic(db, mockFetch as typeof fetch)

  // 같은 날짜·내용이라도 index가 달라 sourceId가 겹치지 않아야 함
  const ids = upsertLog.slice(0, 2).map((r) => (r as any).source_id as string)
  assertEquals(/\s/.test(ids[0]), false)
  assertEquals(ids[0] !== ids[1], true)
})

// ---------------------------------------------------------------------------
// crawlInstagram / crawlInstagramAccount
// ---------------------------------------------------------------------------

Deno.test("crawlInstagram: skips when credentials are missing", async () => {
  const { db, upsertLog } = createMockDb()
  const mockFetch = (): Promise<Response> => Promise.reject(new Error("should not be called"))

  await crawlInstagram({ db, fetchFn: mockFetch as typeof fetch, igBusinessId: undefined, igAccessToken: undefined })
  assertEquals(upsertLog.length, 0)
})

Deno.test("crawlInstagramAccount: saves new posts", async () => {
  const apiResponse = {
    business_discovery: {
      media: {
        data: [
          { id: "post_1", caption: "Hello", permalink: "https://instagram.com/p/1", timestamp: "2026-01-01", media_type: "IMAGE", media_url: "" },
        ],
        paging: {},
      },
    },
  }

  const mockFetch = (): Promise<Response> =>
    Promise.resolve(new Response(JSON.stringify(apiResponse), { status: 200 }))

  const { db, upsertLog } = createMockDb()
  await crawlInstagramAccount("kwu_studentcouncil", db, mockFetch as typeof fetch, "BIZ_ID", "TOKEN")

  assertEquals(upsertLog.length, 1)
  assertEquals((upsertLog[0] as any).source_id, "post_1")
})

Deno.test("crawlInstagramAccount: stops on already-saved post", async () => {
  const apiResponse = {
    business_discovery: {
      media: {
        data: [
          { id: "post_new", permalink: "https://instagram.com/p/new" },
          { id: "post_existing", permalink: "https://instagram.com/p/existing" },
        ],
        paging: { next: "https://graph.facebook.com/next_page" },
      },
    },
  }

  let fetchCallCount = 0
  const mockFetch = (): Promise<Response> => {
    fetchCallCount++
    return Promise.resolve(new Response(JSON.stringify(apiResponse), { status: 200 }))
  }

  const { db, upsertLog } = createMockDb(new Set(["post_existing"]))
  await crawlInstagramAccount("kwu_studentcouncil", db, mockFetch as typeof fetch, "BIZ_ID", "TOKEN")

  // Stops after first page (does not follow next page cursor)
  assertEquals(fetchCallCount, 1)
  assertEquals(upsertLog.length, 1)
  assertEquals((upsertLog[0] as any).source_id, "post_new")
})
