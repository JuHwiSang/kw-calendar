import { getSupabase } from "./client.ts"
import { crawlKwNotice, crawlKwAcademic, crawlInstagram } from "./crawlers.ts"

async function crawlAndSave(mode: string) {
  console.log("Starting crawl process...")
  const skipExisting = mode !== "full"

  try {
    const { data } = await getSupabase()
      .from("source_items")
      .select("crawled_at")
      .order("crawled_at", { ascending: false })
      .limit(1)
    const lastItems = data as { crawled_at: string }[] | null

    if (skipExisting && lastItems?.[0]) {
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
    crawlKwNotice({ skipExisting }),
    crawlKwAcademic(),
    crawlInstagram({ skipExisting })
  ])

  console.log("All crawl tasks finished.")
}

Deno.serve(async (req) => {
  if (req.method !== "POST") {
    return new Response("Method Not Allowed", { status: 405 })
  }

  const { mode = "incremental" } = await req.json().catch(() => ({}))

  const edgeRuntime = (globalThis as any).EdgeRuntime
  if (edgeRuntime?.waitUntil) {
    edgeRuntime.waitUntil(crawlAndSave(mode))
  } else {
    crawlAndSave(mode).catch(console.error)
  }

  return new Response(JSON.stringify({ message: "Crawl started" }), {
    status: 202,
    headers: { "Content-Type": "application/json" },
  })
})
