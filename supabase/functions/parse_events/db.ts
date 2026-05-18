import { createClient } from "jsr:@supabase/supabase-js@2"
import type { EventInsertRow, SourceItem } from "./types.ts"

function createSupabaseClient() {
    const supabaseUrl = Deno.env.get("SUPABASE_URL")
    const serviceRoleKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")

    if (!supabaseUrl) {
        throw new Error("SUPABASE_URL is missing")
    }

    if (!serviceRoleKey) {
        throw new Error("SUPABASE_SERVICE_ROLE_KEY is missing")
    }

    return createClient(supabaseUrl, serviceRoleKey)
}

export async function insertEvents(rows: EventInsertRow[]) {
    if (rows.length === 0) {
        console.log("[DB_INSERT_SKIPPED] no rows to insert")
        return []
    }

    const supabase = createSupabaseClient()

    console.log("[DB_INSERT_START]", { count: rows.length })

    const { data, error } = await supabase
        .from("events")
        .insert(rows)
        .select()

    if (error) {
        console.error("[DB_INSERT_FAILED]", error)
        throw error
    }

    console.log("[DB_INSERT_SUCCESS]", { count: data?.length ?? 0 })

    return data ?? []
}
export async function findExistingEventsForDuplicateCheck(rows: EventInsertRow[]) {
    if (rows.length === 0) {
        return []
    }

    const supabase = createSupabaseClient()

    const categoryIds = [...new Set(rows.map((row) => row.category_id))]

    const startDates = rows.map((row) => new Date(row.start_dt).getTime())
    const minDate = new Date(Math.min(...startDates))
    const maxDate = new Date(Math.max(...startDates))

    minDate.setDate(minDate.getDate() - 1)
    maxDate.setDate(maxDate.getDate() + 1)

    console.log("[DUPLICATE_CANDIDATE_QUERY]", {
        categoryIds,
        from: minDate.toISOString(),
        to: maxDate.toISOString(),
    })

    const { data, error } = await supabase
        .from("events")
        .select("title,start_dt,end_dt,category_id")
        .in("category_id", categoryIds)
        .gte("start_dt", minDate.toISOString())
        .lte("start_dt", maxDate.toISOString())

    if (error) {
        console.error("[DUPLICATE_CANDIDATE_QUERY_FAILED]", error)
        throw error
    }

    console.log("[DUPLICATE_CANDIDATES_FOUND]", {
        count: data?.length ?? 0,
    })

    return data ?? []
}
export async function markSourceItemParsed(sourceItemId: number) {
    const supabase = createSupabaseClient()

    const { error } = await supabase
        .from("source_items")
        .update({
            parse_status: "parsed",
            parse_error: null,
        })
        .eq("id", sourceItemId)

    if (error) {
        console.error("[SOURCE_ITEM_MARK_PARSED_FAILED]", error)
        throw error
    }

    console.log("[SOURCE_ITEM_MARK_PARSED]", { sourceItemId })
}

export async function markSourceItemFailed(sourceItemId: number, parseError: string) {
    const supabase = createSupabaseClient()

    const { error } = await supabase
        .from("source_items")
        .update({
            parse_status: "failed",
            parse_error: parseError,
        })
        .eq("id", sourceItemId)

    if (error) {
        console.error("[SOURCE_ITEM_MARK_FAILED_FAILED]", error)
        throw error
    }

    console.log("[SOURCE_ITEM_MARK_FAILED]", {
        sourceItemId,
        parseError,
    })
}
export async function getPendingSourceItems(limit = 10): Promise<SourceItem[]> {
    const supabase = createSupabaseClient()

    console.log("[SOURCE_ITEMS_QUERY_START]", { limit })

    const { data, error } = await supabase
        .from("source_items")
        .select("id, source_type, source_id, source_url, content_type, raw_content, crawled_at")
        .eq("parse_status", "pending")
        .order("crawled_at", { ascending: true })
        .limit(limit)

    if (error) {
        console.error("[SOURCE_ITEMS_QUERY_FAILED]", error)
        throw error
    }

    console.log("[SOURCE_ITEMS_QUERY_SUCCESS]", {
        count: data?.length ?? 0,
    })

    return data ?? []
}
