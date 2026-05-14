import { getSupabase } from "./client.ts"
import { crawlKwNotice, crawlKwAcademic, crawlInstagram } from "./crawlers.ts"

async function crawlAndSave() {
  console.log("Starting crawl process...")

  try {
    const { data } = await getSupabase()
      .from("source_items")
      .select("crawled_at")
      .order("crawled_at", { ascending: false })
      .limit(1)
    const lastItems = data as { crawled_at: string }[] | null

    /*
    if (lastItems?.[0]) {
      const diffMs = Date.now() - new Date(lastItems[0].crawled_at).getTime()
      if (diffMs < 3600000) {
        console.log("Crawl skipped: last crawl was less than 1 hour ago.")
        return
      }
    }
    */
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
