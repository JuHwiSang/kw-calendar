import type { EventInsertRow, LlmParsedEvent } from "./types.ts"
import { CategoryIdMap } from "./types.ts"

export function toEventInsertRows(events: LlmParsedEvent[]): EventInsertRow[] {
    return events
        .filter((event) => {
            if (!event.should_register) return false
            if (!event.title) return false
            if (!event.start_dt) return false
            if (event.confidence < 0.5) return false

            return true
        })
        .map((event) => ({
            source_item_id: event.source_item_id,
            title: event.title as string,
            body: event.body,
            start_dt: event.start_dt as string,
            end_dt: event.end_dt,
            is_all_day: event.is_all_day,
            notice_dt: event.notice_dt,
            external_link: event.external_link,
            category_id: CategoryIdMap[event.category],
        }))
}
