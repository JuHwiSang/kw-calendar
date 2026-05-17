import "jsr:@supabase/functions-js/edge-runtime.d.ts"
import { insertEvents, getPendingSourceItems, findExistingEventsForDuplicateCheck, markSourceItemFailed, markSourceItemParsed } from "./db.ts"
import { filterDuplicateEvents } from "./duplicate.ts"
import { parseEventsWithLlm } from "./llm.ts"
import { toEventInsertRows } from "./parser.ts"
import type { ParseEventsRequest } from "./types.ts"



Deno.serve(async (req) => {
    try {
        console.log("[PARSE_EVENTS_START]")

        const body = await req.json().catch(() => ({}))
        const limit = typeof body.limit === "number" ? body.limit : 10

        console.log("[REQUEST_BODY]", body)
        console.log("[PARSE_LIMIT]", { limit })

        const sourceItems = await getPendingSourceItems(limit)

        const results = []

        for (const sourceItem of sourceItems) {
            console.log("[SOURCE_ITEM_PARSE_START]", {
                sourceItemId: sourceItem.id,
                sourceType: sourceItem.source_type,
            })

            try {
                const llmEvents = await parseEventsWithLlm(sourceItem.raw_content, sourceItem.id)
                console.log("[LLM_EVENTS]", llmEvents)

                const insertRows = toEventInsertRows(llmEvents)
                console.log("[EVENT_INSERT_ROWS]", insertRows)

                const existingEvents = await findExistingEventsForDuplicateCheck(insertRows)

                const deduplicatedRows = filterDuplicateEvents(insertRows, existingEvents)
                console.log("[DEDUPLICATED_INSERT_ROWS]", deduplicatedRows)

                const insertedEvents = await insertEvents(deduplicatedRows)
                console.log("[INSERTED_EVENTS]", insertedEvents)

                await markSourceItemParsed(sourceItem.id)

                results.push({
                    sourceItemId: sourceItem.id,
                    success: true,
                    parsedCount: llmEvents.length,
                    insertTargetCount: insertRows.length,
                    insertedCount: insertedEvents.length,
                })
            } catch (error) {
                const message = error instanceof Error ? error.message : String(error)

                console.error("[SOURCE_ITEM_PARSE_FAILED]", {
                    sourceItemId: sourceItem.id,
                    error: message,
                })

                await markSourceItemFailed(sourceItem.id, message)

                results.push({
                    sourceItemId: sourceItem.id,
                    success: false,
                    error: message,
                })
            }
        }

        console.log("[PARSE_EVENTS_DONE]", {
            total: results.length,
        })

        return new Response(
            JSON.stringify({
                success: true,
                data: {
                    processedCount: results.length,
                    results,
                },
            }),
            {
                status: 200,
                headers: { "Content-Type": "application/json" },
            },
        )
    } catch (error) {
        const message = error instanceof Error ? error.message : String(error)

        console.error("[PARSE_EVENTS_FAILED]", message)

        return new Response(
            JSON.stringify({
                success: false,
                error: message,
            }),
            {
                status: 500,
                headers: { "Content-Type": "application/json" },
            },
        )
    }
})
