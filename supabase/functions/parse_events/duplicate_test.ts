import { assertEquals } from "@std/assert"
import { filterDuplicateEvents } from "./duplicate.ts"
import type { EventInsertRow } from "./types.ts"

Deno.test("filterDuplicateEvents removes duplicate events", () => {
    const insertRows: EventInsertRow[] = [
        {
            source_item_id: 1,
            title: "AI 세미나",
            body: "AI 세미나 안내",
            start_dt: "2026-05-20T19:00:00+09:00",
            end_dt: null,
            is_all_day: false,
            notice_dt: null,
            external_link: null,
            category_id: 6,
        },
    ]

    const existingEvents = [
        {
            title: "AI 세미나",
            start_dt: "2026-05-20T19:00:00+09:00",
            end_dt: null,
            category_id: 6,
        },
    ]

    const result = filterDuplicateEvents(insertRows, existingEvents)

    assertEquals(result.length, 0)
})

Deno.test("filterDuplicateEvents keeps non-duplicate events", () => {
    const insertRows: EventInsertRow[] = [
        {
            source_item_id: 1,
            title: "Database 세미나",
            body: "DB 세미나 안내",
            start_dt: "2026-05-21T14:00:00+09:00",
            end_dt: null,
            is_all_day: false,
            notice_dt: null,
            external_link: null,
            category_id: 6,
        },
    ]

    const existingEvents = [
        {
            title: "AI 세미나",
            start_dt: "2026-05-20T19:00:00+09:00",
            end_dt: null,
            category_id: 6,
        },
    ]

    const result = filterDuplicateEvents(insertRows, existingEvents)

    assertEquals(result.length, 1)
    assertEquals(result[0].title, "Database 세미나")
})
Deno.test("filterDuplicateEvents removes similar title duplicate events", () => {
    const insertRows: EventInsertRow[] = [
        {
            source_item_id: 1,
            title: "2026학년도 1학기 AI 세미나 안내",
            body: "AI 세미나 안내",
            start_dt: "2026-05-20T19:00:00+09:00",
            end_dt: null,
            is_all_day: false,
            notice_dt: null,
            external_link: null,
            category_id: 6,
        },
    ]

    const existingEvents = [
        {
            title: "AI 세미나",
            start_dt: "2026-05-20T19:00:00+09:00",
            end_dt: null,
            category_id: 6,
        },
    ]

    const result = filterDuplicateEvents(insertRows, existingEvents)

    assertEquals(result.length, 0)
})

Deno.test("filterDuplicateEvents keeps same title on different date", () => {
    const insertRows: EventInsertRow[] = [
        {
            source_item_id: 1,
            title: "AI 세미나",
            body: "AI 세미나 안내",
            start_dt: "2026-05-21T19:00:00+09:00",
            end_dt: null,
            is_all_day: false,
            notice_dt: null,
            external_link: null,
            category_id: 6,
        },
    ]

    const existingEvents = [
        {
            title: "AI 세미나",
            start_dt: "2026-05-20T19:00:00+09:00",
            end_dt: null,
            category_id: 6,
        },
    ]

    const result = filterDuplicateEvents(insertRows, existingEvents)

    assertEquals(result.length, 1)
})
