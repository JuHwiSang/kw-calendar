import { parseHTML } from "linkedom"
import { getSupabase, IG_BUSINESS_ID, IG_ACCESS_TOKEN } from "./client.ts"

const KW_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36"

class ClientError extends Error {
  constructor(status: number) {
    super(`HTTP error! status: ${status}`)
  }
}

async function fetchWithRetry(
  url: string,
  options: RequestInit,
  fetchFn: typeof fetch,
  maxRetries = 3,
  timeoutMs = 5000,
): Promise<Response> {
  let lastError: unknown
  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      const response = await fetchFn(url, { ...options, signal: AbortSignal.timeout(timeoutMs) })
      if (response.status >= 400 && response.status < 500) throw new ClientError(response.status)
      if (response.status >= 500) throw new Error(`HTTP error! status: ${response.status}`)
      return response
    } catch (err) {
      if (err instanceof ClientError) throw err
      lastError = err
      if (attempt < maxRetries) {
        await new Promise(r => setTimeout(r, 1000 * Math.pow(2, attempt)))
      }
    }
  }
  throw lastError
}

export async function crawlKwNotice({
  db = getSupabase(),
  fetchFn = globalThis.fetch as typeof fetch,
  throttleMs = 500,
  skipExisting = true,
}: {
  db?: any
  fetchFn?: typeof fetch
  throttleMs?: number
  skipExisting?: boolean
} = {}) {
  console.log("Crawling KW Notice...")
  const MAX_PAGES = 5

  for (let page = 1; page <= MAX_PAGES; page++) {
    try {
      const listUrl = `https://www.kw.ac.kr/ko/life/notice.jsp?tpage=${page}&searchKey=1&searchVal=&srCategoryId=`
      const response = await fetchWithRetry(listUrl, { headers: { "User-Agent": KW_USER_AGENT } }, fetchFn)

      const html = await response.text()
      const { document } = parseHTML(html) as any
      const listItems = document.querySelectorAll(".board-list-box ul li")

      if (listItems.length === 0) break

      let processedCount = 0
      for (const li of listItems) {
        let detailUrl = ""
        try {
          const linkAnchor = li.querySelector("a")
          const href = linkAnchor?.getAttribute("href")
          if (!href) continue

          const idMatch = href.match(/(?:articleNo|DUID)=(\d+)/)
          if (!idMatch) continue

          const articleNo = idMatch[1]

          if (skipExisting) {
            const { data: existing } = await db
              .from("source_items")
              .select("id")
              .eq("source_type", "kw_notice")
              .eq("source_id", articleNo)
              .maybeSingle()

            if (existing) continue
          }

          detailUrl = href.startsWith("http") ? href : `https://www.kw.ac.kr${href}`
          const category = li.querySelector(".category")?.textContent?.trim() || "일반"

          await new Promise(r => setTimeout(r, throttleMs))

          const detailRes = await fetchWithRetry(detailUrl, { headers: { "User-Agent": KW_USER_AGENT } }, fetchFn)

          const detailHtml = await detailRes.text()
          const { document: detailDoc } = parseHTML(detailHtml) as any
          const contentArea = detailDoc.querySelector(".board-view-box") || detailDoc.querySelector(".board-view-content")

          if (!contentArea) {
            console.error(`No content area found: ${detailUrl}`)
            continue
          }

          await db.from("source_items").upsert({
            source_type: "kw_notice",
            source_id: articleNo,
            source_url: detailUrl,
            content_type: "html",
            raw_content: `<!-- Category: ${category} -->\n${contentArea.outerHTML}`
          }, { onConflict: "source_type, source_id" })

          processedCount++
        } catch (itemError) {
          console.error(`Error processing notice item${detailUrl ? ` ${detailUrl}` : ""}:`, itemError)
        }
      }

      console.log(`KW Notice page ${page}: ${processedCount} items saved.`)
      if (skipExisting && processedCount === 0) break
    } catch (pageError) {
      console.error(`Error processing KW Notice page ${page}:`, pageError)
      break
    }
  }
}

export async function crawlKwAcademic(
  db: any = getSupabase(),
  fetchFn: typeof fetch = globalThis.fetch,
) {
  console.log("Crawling KW Academic...")
  const now = new Date()
  const currentYear = now.getFullYear()
  const currentMonth = now.getMonth() + 1

  const targets = Array.from({ length: 6 }, (_, i) => {
    const month = ((currentMonth - 1 + i) % 12) + 1
    const year = currentYear + Math.floor((currentMonth - 1 + i) / 12)
    return { year, month }
  })

  const apiUrl = "https://www.kw.ac.kr/KWBoard/list5_detail.jsp"
  const pageUrl = "https://www.kw.ac.kr/ko/life/bachelor_calendar.jsp"

  for (const { year, month } of targets) {
    try {
      const response = await fetchWithRetry(apiUrl, {
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
          "Referer": pageUrl,
          "User-Agent": KW_USER_AGENT,
        },
        body: `sy=${year}&sm=${month}`,
      }, fetchFn)

      const html = await response.text()
      const { document } = parseHTML(html) as any
      const listItems = document.querySelectorAll(".schedule-this-monthlist li")

      let index = 0
      let savedCount = 0
      for (const li of listItems) {
        try {
          const dateText = li.querySelector("strong")?.textContent?.trim() || ""
          const content = li.querySelector("p")?.textContent?.trim()
          if (!content) continue

          const dateMatch = dateText.match(/(\d{1,2})\.(\d{1,2})/)
          const normalizedDate = dateMatch
            ? `${dateMatch[1].padStart(2, "0")}${dateMatch[2].padStart(2, "0")}`
            : dateText.replace(/[^0-9]/g, "").slice(0, 4)

          const sourceId = `${year}-${normalizedDate}-${index}-${content}`.replace(/\s+/g, "")
          index++

          await db.from("source_items").upsert({
            source_type: "kw_academic",
            source_id: sourceId,
            source_url: pageUrl,
            content_type: "json",
            raw_content: JSON.stringify({ year, month, date: dateText, event: content })
          }, { onConflict: "source_type, source_id" })

          savedCount++
        } catch (itemError) {
          console.error(`Error processing academic item:`, itemError)
        }
      }

      console.log(`KW Academic ${year}-${month}: ${savedCount} items upserted.`)
    } catch (monthError) {
      console.error(`Error processing KW Academic ${year}-${month}:`, monthError)
    }
  }
}

export async function crawlInstagram({
  db = getSupabase(),
  fetchFn = globalThis.fetch as typeof fetch,
  igBusinessId = IG_BUSINESS_ID,
  igAccessToken = IG_ACCESS_TOKEN,
  skipExisting = true,
}: {
  db?: any
  fetchFn?: typeof fetch
  igBusinessId?: string
  igAccessToken?: string
  skipExisting?: boolean
} = {}) {
  if (!igBusinessId || !igAccessToken) {
    console.warn("Instagram crawl skipped: Missing credentials.")
    return
  }

  console.log("Crawling Instagram...")
  const TARGET_USERNAMES = ["kw_software_"]

  for (const username of TARGET_USERNAMES) {
    try {
      await crawlInstagramAccount(username, db, fetchFn, igBusinessId, igAccessToken, { skipExisting })
    } catch (e) {
      console.error(`Error crawling Instagram account ${username}:`, e)
    }
  }
}

export async function crawlInstagramAccount(
  username: string,
  db: any,
  fetchFn: typeof fetch,
  igBusinessId: string,
  igAccessToken: string,
  { skipExisting = true }: { skipExisting?: boolean } = {},
) {
  const mediaFields = "id,caption,media_url,permalink,timestamp,media_type"
  const fields = `business_discovery.username(${username}){media.limit(50){${mediaFields}}}`
  const baseUrl = `https://graph.facebook.com/v25.0/${igBusinessId}`

  const params = new URLSearchParams({ fields, access_token: igAccessToken })
  let nextUrl: string | null = `${baseUrl}?${params}`

  let processedCount = 0
  let pageCount = 0
  const MAX_PAGES = 10

  while (nextUrl && pageCount < MAX_PAGES) {
    const response: Response = await fetchFn(nextUrl)
    if (!response.ok) {
      const body = await response.text()
      throw new Error(`Instagram API error! status: ${response.status}, body: ${body}`)
    }

    const data: any = await response.json()
    const mediaPage: any = data?.business_discovery?.media
    const posts: any[] = mediaPage?.data || []

    for (const post of posts) {
      try {
        if (skipExisting) {
          const { data: existing } = await db
            .from("source_items")
            .select("id")
            .eq("source_type", "instagram")
            .eq("source_id", post.id)
            .maybeSingle()

          if (existing) {
            nextUrl = null
            break
          }
        }

        await db.from("source_items").upsert({
          source_type: "instagram",
          source_id: post.id,
          source_url: post.permalink,
          content_type: "json",
          raw_content: JSON.stringify({ ...post, username })
        }, { onConflict: "source_type, source_id" })

        processedCount++
      } catch (itemError) {
        console.error(`Error processing instagram post ${post.id}:`, itemError)
      }
    }

    nextUrl = nextUrl !== null ? (mediaPage?.paging?.next ?? null) : null
    pageCount++
  }

  console.log(`Instagram @${username}: ${processedCount} items saved (${pageCount} page(s) fetched).`)
}
