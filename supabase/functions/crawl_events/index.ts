import { createClient } from "@supabase/supabase-js"
import { parseHTML } from "linkedom"

const SUPABASE_URL = Deno.env.get("SUPABASE_URL") ?? ""
const SUPABASE_SERVICE_ROLE_KEY = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY") ?? ""
const IG_BUSINESS_ID = Deno.env.get("IG_BUSINESS_ID")
const IG_ACCESS_TOKEN = Deno.env.get("IG_ACCESS_TOKEN")

export const supabase = createClient(SUPABASE_URL, SUPABASE_SERVICE_ROLE_KEY)

/**
 * 광운대 공지사항 크롤링
 */
export async function crawlKwNotice() {
  console.log("Crawling KW Notice...")
  const MAX_PAGES = 5
  
  for (let page = 1; page <= MAX_PAGES; page++) {
    try {
      const listUrl = `https://www.kw.ac.kr/ko/life/notice.jsp?cp=${page}`
      const response = await fetch(listUrl)
      if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)
      
      const html = await response.text()
      const { document } = parseHTML(html) as any
      const listItems = document.querySelectorAll(".board-list-box ul li")

      if (listItems.length === 0) break

      let newItemsCount = 0
      for (const li of listItems) {
        try {
          const linkAnchor = li.querySelector("a")
          const href = linkAnchor?.getAttribute("href")
          if (!href) continue
          
          const idMatch = href.match(/(?:articleNo|DUID)=(\d+)/)
          if (!idMatch) continue
          
          const articleNo = idMatch[1]
          
          // Check if already exists
          const { data: existing } = await supabase
            .from("source_items")
            .select("id")
            .eq("source_type", "kw_notice")
            .eq("source_id", articleNo)
            .maybeSingle()

          if (existing) continue

          const detailUrl = href.startsWith("http") ? href : `https://www.kw.ac.kr${href}`
          const category = li.querySelector(".category")?.textContent?.trim() || "일반"
        
          await new Promise(r => setTimeout(r, 500)) // Throttle
          
          const detailRes = await fetch(detailUrl)
          if (!detailRes.ok) {
            console.error(`Failed to fetch detail: ${detailUrl}`)
            continue
          }
          
          const detailHtml = await detailRes.text()
          const { document: detailDoc } = parseHTML(detailHtml) as any
          const contentArea = detailDoc.querySelector(".board-view-box") || detailDoc.querySelector(".board-view-content")
        
          await supabase.from("source_items").upsert({
            source_type: "kw_notice",
            source_id: articleNo,
            source_url: detailUrl,
            content_type: "html",
            raw_content: `<!-- Category: ${category} -->\n${contentArea?.outerHTML || ""}`
          }, { onConflict: "source_type, source_id" })
          
          newItemsCount++
        } catch (itemError) {
          console.error(`Error processing notice item:`, itemError)
        }
      }
      
      console.log(`KW Notice page ${page}: ${newItemsCount} new items saved.`)
      if (newItemsCount === 0) break // All items on this page already exist
    } catch (pageError) {
      console.error(`Error processing KW Notice page ${page}:`, pageError)
      break
    }
  }
}

/**
 * 광운대 학사일정 크롤링
 */
export async function crawlKwAcademic() {
  console.log("Crawling KW Academic...")
  const now = new Date()
  const currentYear = now.getFullYear()
  const currentMonth = now.getMonth() + 1
  
  const targets = [
    { year: currentYear, month: currentMonth },
    { year: currentMonth === 12 ? currentYear + 1 : currentYear, month: (currentMonth % 12) + 1 }
  ]

  for (const { year, month } of targets) {
    try {
      const calendarUrl = `https://www.kw.ac.kr/ko/life/bachelor_calendar.jsp?year=${year}&month=${month}`
      const response = await fetch(calendarUrl)
      if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)
      
      const html = await response.text()
      const { document } = parseHTML(html) as any
      const listItems = document.querySelectorAll(".bachelor-calendar-list li")

      for (const li of listItems) {
        try {
          const dateText = li.querySelector(".date")?.textContent?.trim() || ""
          const content = li.querySelector(".content")?.textContent?.trim()
          if (!content) continue

          // Normalize date for ID (e.g., "05.11 (월)" -> "0511", "1.1" -> "0101")
          const dateMatch = dateText.match(/(\d{1,2})\.(\d{1,2})/)
          const normalizedDate = dateMatch 
            ? `${dateMatch[1].padStart(2, "0")}${dateMatch[2].padStart(2, "0")}` 
            : dateText.replace(/[^0-9]/g, "")
          
          const sourceId = `${year}-${normalizedDate}-${content}`.replace(/\s+/g, "")
          
          await supabase.from("source_items").upsert({
            source_type: "kw_academic",
            source_id: sourceId,
            source_url: calendarUrl,
            content_type: "json",
            raw_content: JSON.stringify({ year, month, date: dateText, event: content })
          }, { onConflict: "source_type, source_id" })
        } catch (itemError) {
          console.error(`Error processing academic item:`, itemError)
        }
      }
    } catch (monthError) {
      console.error(`Error processing KW Academic ${year}-${month}:`, monthError)
    }
  }
}

/**
 * 인스타그램 크롤링 (Business Discovery API)
 */
export async function crawlInstagram() {
  if (!IG_BUSINESS_ID || !IG_ACCESS_TOKEN) {
    console.warn("Instagram crawl skipped: Missing credentials.")
    return
  }

  console.log("Crawling Instagram...")
  const TARGET_USERNAME = "kw_software_"
  const query = `business_discovery.username('${TARGET_USERNAME}'){media{id,caption,media_url,permalink,timestamp,media_type}}`
  const url = `https://graph.facebook.com/v21.0/${IG_BUSINESS_ID}?fields=${query}&access_token=${IG_ACCESS_TOKEN}`

  try {
    const response = await fetch(url)
    if (!response.ok) throw new Error(`Instagram API error! status: ${response.status}`)
    
    const data = await response.json()
    const media = data?.business_discovery?.media?.data || []

    let newItemsCount = 0
    for (const post of media) {
      try {
        const { data: existing } = await supabase
          .from("source_items")
          .select("id")
          .eq("source_type", "instagram")
          .eq("source_id", post.id)
          .maybeSingle()

        if (existing) continue

        await supabase.from("source_items").upsert({
          source_type: "instagram",
          source_id: post.id,
          source_url: post.permalink,
          content_type: "json",
          raw_content: JSON.stringify(post)
        }, { onConflict: "source_type, source_id" })
        
        newItemsCount++
      } catch (itemError) {
        console.error(`Error processing instagram post ${post.id}:`, itemError)
      }
    }
    console.log(`Instagram: ${newItemsCount} new items saved.`)
  } catch (e) {
    console.error("Error crawling Instagram:", e)
  }
}

async function crawlAndSave() {
  console.log("Starting crawl process...")
  
  // Check throttle (1 hour)
  try {
    const { data: lastItems } = await supabase
      .from("source_items")
      .select("crawled_at")
      .order("crawled_at", { ascending: false })
      .limit(1)

    if (lastItems?.[0]) {
      const diffMs = Date.now() - new Date(lastItems[0].crawled_at).getTime()
      if (diffMs < 3600000) {
        console.log("Crawl skipped: last crawl was less than 1 hour ago.")
        return
      }
    }
  } catch (throttleError) {
    console.error("Error checking throttle, proceeding anyway:", throttleError)
  }

  await Promise.allSettled([
    crawlKwNotice(),
    crawlKwAcademic(),
    crawlInstagram()
  ])

  console.log("All crawl tasks finished.")
}

Deno.serve((req) => {
  if (req.method !== "POST") {
    return new Response("Method Not Allowed", { status: 405 })
  }

  const edgeRuntime = (globalThis as any).EdgeRuntime
  if (edgeRuntime?.waitUntil) {
    edgeRuntime.waitUntil(crawlAndSave())
  } else {
    crawlAndSave()
  }

  return new Response(JSON.stringify({ message: "Crawl started" }), {
    status: 202,
    headers: { "Content-Type": "application/json" },
  })
})
