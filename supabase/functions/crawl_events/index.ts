import { createClient } from "@supabase/supabase-js"
import { parseHTML } from "linkedom"

const SUPABASE_URL = Deno.env.get("SUPABASE_URL") ?? ""
const SUPABASE_SERVICE_ROLE_KEY = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY") ?? ""
const IG_BUSINESS_ID = Deno.env.get("IG_BUSINESS_ID")
const IG_ACCESS_TOKEN = Deno.env.get("IG_ACCESS_TOKEN")

const supabase = createClient(SUPABASE_URL, SUPABASE_SERVICE_ROLE_KEY)

async function crawlAndSave() {
  console.log("Starting crawl process...")
  
  // 1. Check last execution time (at least 1 hour)
  const { data: lastItems } = await supabase
    .from("source_items")
    .select("crawled_at")
    .order("crawled_at", { ascending: false })
    .limit(1)

  if (lastItems && lastItems.length > 0) {
    const lastCrawl = new Date(lastItems[0].crawled_at)
    const now = new Date()
    const diffMs = now.getTime() - lastCrawl.getTime()
    if (diffMs < 3600000) { // 1 hour
      console.log("Crawl skipped: last crawl was less than 1 hour ago.")
      return
    }
  }

  // 2. Crawl KW Notice
  try {
    const page = 1
    const listUrl = `https://www.kw.ac.kr/ko/life/notice.jsp?cp=${page}`
    const response = await fetch(listUrl)
    const html = await response.text()
    const { document } = parseHTML(html)
    const listItems = document.querySelectorAll(".board-list-box ul li")

    for (const li of listItems) {
      const linkAnchor = li.querySelector("a")
      const href = linkAnchor?.getAttribute("href")
      if (!href) continue
      
      const idMatch = href.match(/(?:articleNo|DUID)=(\d+)/)
      if (idMatch) {
        const articleNo = idMatch[1]
        const detailUrl = href.startsWith("http") ? href : `https://www.kw.ac.kr${href}`
        const category = li.querySelector(".category")?.textContent?.trim() || "일반"
      
        // Respect delay to avoid blocking
        await new Promise(r => setTimeout(r, 500))
        
        const detailRes = await fetch(detailUrl)
        const detailHtml = await detailRes.text()
        const { document: detailDoc } = parseHTML(detailHtml)
        const contentArea = detailDoc.querySelector(".board-view-box") || detailDoc.querySelector(".board-view-content")
      
        await supabase.from("source_items").upsert({
          source_type: "kw_notice",
          source_id: articleNo,
          source_url: detailUrl,
          content_type: "html",
          raw_content: `<!-- Category: ${category} -->\n${contentArea?.outerHTML || ""}`
        }, { onConflict: "source_type, source_id" })
      }
    }
    console.log("KW Notice crawl completed.")
  } catch (e) {
    console.error("Error crawling KW Notice:", e)
  }

  // 3. Crawl KW Academic
  try {
    const now = new Date()
    const years = [now.getFullYear()]
    const months = [now.getMonth() + 1]
    
    // also crawl next month
    const nextMonthDate = new Date(now.getFullYear(), now.getMonth() + 1, 1)
    if (nextMonthDate.getFullYear() !== now.getFullYear()) {
      years.push(nextMonthDate.getFullYear())
    }
    months.push(nextMonthDate.getMonth() + 1)

    for (const year of years) {
      const targetMonths = year === now.getFullYear() ? months : [nextMonthDate.getMonth() + 1]
      
      for (const month of targetMonths) {
        const calendarUrl = `https://www.kw.ac.kr/ko/life/bachelor_calendar.jsp?year=${year}&month=${month}`
        const response = await fetch(calendarUrl)
        const html = await response.text()
        const { document } = parseHTML(html)
        const listItems = document.querySelectorAll(".bachelor-calendar-list li")

        for (const li of listItems) {
          const dateText = li.querySelector(".date")?.textContent?.trim()
          const content = li.querySelector(".content")?.textContent?.trim()
          if (content) {
            const sourceId = `${year}-${month}-${dateText}-${content}`.replace(/\s+/g, "")
            await supabase.from("source_items").upsert({
              source_type: "kw_academic",
              source_id: sourceId,
              source_url: calendarUrl,
              content_type: "json",
              raw_content: JSON.stringify({ year, month, date: dateText, event: content })
            }, { onConflict: "source_type, source_id" })
          }
        }
      }
    }
    console.log("KW Academic crawl completed.")
  } catch (e) {
    console.error("Error crawling KW Academic:", e)
  }

  // 4. Crawl Instagram
  if (IG_BUSINESS_ID && IG_ACCESS_TOKEN) {
    try {
      const TARGET_USERNAME = "kw_software_"
      const query = `business_discovery.username(${TARGET_USERNAME}){media{id,caption,media_url,permalink,timestamp,media_type}}`
      const url = `https://graph.facebook.com/v21.0/${IG_BUSINESS_ID}?fields=${query}&access_token=${IG_ACCESS_TOKEN}`

      const response = await fetch(url)
      const data = await response.json()
      const media = data?.business_discovery?.media?.data || []

      for (const post of media) {
        await supabase.from("source_items").upsert({
          source_type: "instagram",
          source_id: post.id,
          source_url: post.permalink,
          content_type: "json",
          raw_content: JSON.stringify(post)
        }, { onConflict: "source_type, source_id" })
      }
      console.log("Instagram crawl completed.")
    } catch (e) {
      console.error("Error crawling Instagram:", e)
    }
  }

  console.log("All crawl tasks finished.")
}

Deno.serve(async (req) => {
  if (req.method !== "POST") {
    return new Response("Method Not Allowed", { status: 405 })
  }

  if ((globalThis as any).EdgeRuntime?.waitUntil) {
    (globalThis as any).EdgeRuntime.waitUntil(crawlAndSave())
  } else {
    crawlAndSave()
  }

  return new Response(JSON.stringify({ message: "Crawl started" }), {
    status: 202,
    headers: { "Content-Type": "application/json" },
  })
})
