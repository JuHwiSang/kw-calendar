import type { EventInsertRow, LlmParsedEvent } from "./types.ts"
import { CategoryIdMap } from "./types.ts"

type RegisterableLlmParsedEvent = LlmParsedEvent & {
    title: string
    start_dt: string
}

export function parseLlmResponse(
    llmText: string,
    sourceItemId: number,
): LlmParsedEvent[] {
    const parsed = parseJsonArray(llmText)

    return parsed
        .map((event) => normalizeLlmParsedEvent(event, sourceItemId))
        .filter((event): event is LlmParsedEvent => event !== null)
}

function parseJsonArray(llmText: string): Partial<LlmParsedEvent>[] {
    const cleanedText = llmText
        .replace(/^```json\s*/i, "")
        .replace(/^```\s*/i, "")
        .replace(/```$/i, "")
        .trim()

    try {
        const parsed = JSON.parse(cleanedText)

        if (!Array.isArray(parsed)) {
            throw new Error("LLM response is not a JSON array")
        }

        return parsed as Partial<LlmParsedEvent>[]
    } catch (_error) {
        throw new Error(`Failed to parse LLM JSON response: ${cleanedText}`)
    }
}

function normalizeLlmParsedEvent(
    event: Partial<LlmParsedEvent>,
    sourceItemId: number,
): LlmParsedEvent | null {
    if (!event.title || typeof event.title !== "string") {
        return null
    }

    if (!event.start_dt || typeof event.start_dt !== "string") {
        return null
    }

    const confidence = typeof event.confidence === "number"
        ? event.confidence
        : 0.5

    const needsReview = typeof event.needs_review === "boolean"
        ? event.needs_review
        : confidence < 0.75

    const shouldRegister = typeof event.should_register === "boolean"
        ? event.should_register
        : confidence >= 0.5

    return {
        source_item_id: sourceItemId,
        title: event.title.trim(),
        body: event.body ?? "",
        start_dt: event.start_dt,
        end_dt: event.end_dt ?? null,
        is_all_day: event.is_all_day ?? false,
        notice_dt: event.notice_dt ?? null,
        external_link: event.external_link ?? null,
        category: event.category ?? "OTHER",
        confidence,
        needs_review: needsReview,
        should_register: shouldRegister,
        is_recurring: event.is_recurring ?? false,
        recurrence_rule: event.recurrence_rule ?? null,
        recurrence_end_at: event.recurrence_end_at ?? null,
        extraction_notes: event.extraction_notes ?? null,
        error_reason: event.error_reason ?? null,
    }
}

function isRegisterableEvent(
    event: LlmParsedEvent,
): event is RegisterableLlmParsedEvent {
    if (!event.should_register) return false
    if (!event.title) return false
    if (!event.start_dt) return false
    if (event.confidence < 0.5) return false

    return true
}

export function toEventInsertRows(events: LlmParsedEvent[]): EventInsertRow[] {
    return events
        .filter(isRegisterableEvent)
        .map((event) => ({
            source_item_id: event.source_item_id,
            title: event.title,
            body: event.body,
            start_dt: event.start_dt,
            end_dt: event.end_dt,
            is_all_day: event.is_all_day,
            notice_dt: event.notice_dt,
            external_link: event.external_link,
            category_id: CategoryIdMap[event.category],
        }))
}
