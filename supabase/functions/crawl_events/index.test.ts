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
  await crawlKwNotice(db, mockFetch as typeof fetch, 0)

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
  await crawlKwNotice(db, mockFetch as typeof fetch, 0)

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
  await crawlKwNotice(db, mockFetch as typeof fetch, 0)

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
  await crawlKwNotice(db, mockFetch as typeof fetch, 0)

  // Stops after page 1 (1 list fetch, 0 detail fetches)
  assertEquals(fetchCallCount, 1)
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

  // 2 items × 2 months = 4 upserts
  assertEquals(upsertLog.length, 4)
  assertEquals((upsertLog[0] as any).source_type, "kw_academic")
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

  await crawlInstagram(db, mockFetch as typeof fetch, undefined, undefined)
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
