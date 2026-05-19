import { assertEquals } from "@std/assert"
import { toEventInsertRows } from "./parser.ts"
import type { LlmParsedEvent } from "./types.ts"

Deno.test("toEventInsertRows converts valid LLM event to DB insert row", () => {
    const llmEvents: LlmParsedEvent[] = [
        {
            source_item_id: 1,
            title: "AI 세미나",
            body: "AI 세미나 안내",
            start_dt: "2026-05-20T19:00:00+09:00",
            end_dt: null,
            is_all_day: false,
            notice_dt: null,
            external_link: null,
            category: "ACTIVITY",
            confidence: 0.9,
            needs_review: false,
            should_register: true,
            is_recurring: false,
            recurrence_rule: null,
            recurrence_end_at: null,
            extraction_notes: null,
            error_reason: null,
        },
    ]

    const rows = toEventInsertRows(llmEvents)

    assertEquals(rows.length, 1)
    assertEquals(rows[0].source_item_id, 1)
    assertEquals(rows[0].title, "AI 세미나")
    assertEquals(rows[0].category_id, 6)
})

Deno.test("toEventInsertRows filters out event when should_register is false", () => {
    const llmEvents: LlmParsedEvent[] = [
        {
            source_item_id: 1,
            title: "불확실한 일정",
            body: null,
            start_dt: "2026-05-20T19:00:00+09:00",
            end_dt: null,
            is_all_day: false,
            notice_dt: null,
            external_link: null,
            category: "OTHER",
            confidence: 0.4,
            needs_review: false,
            should_register: false,
            is_recurring: false,
            recurrence_rule: null,
            recurrence_end_at: null,
            extraction_notes: "낮은 신뢰도",
            error_reason: null,
        },
    ]

    const rows = toEventInsertRows(llmEvents)

    assertEquals(rows.length, 0)
})
