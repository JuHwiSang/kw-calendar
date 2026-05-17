export type SourceType = "instagram" | "kw_notice" | "kw_academic" | "user_input"

export type ContentType = "html" | "json" | "text"

export type EventCategory =
    | "ACADEMIC"
    | "CLASS"
    | "EXAM"
    | "EVENT"
    | "CAREER"
    | "FINANCE"
    | "ACTIVITY"
    | "PERSONAL"
    | "OTHER"

export interface SourceItem {
    id: number
    source_type: SourceType
    source_id?: string
    source_url: string | null
    content_type: ContentType
    raw_content: string
    crawled_at: string
}

export interface ParseEventsRequest {
    content?: string
    sourceItemId?: number
    sourceItem?: SourceItem
}

export interface LlmParsedEvent {
    source_item_id: number
    title: string | null
    body: string | null
    start_dt: string | null
    end_dt: string | null
    is_all_day: boolean
    notice_dt: string | null
    external_link: string | null
    category: EventCategory
    confidence: number
    needs_review: boolean
    should_register: boolean
    is_recurring: boolean
    recurrence_rule: string | null
    recurrence_end_at: string | null
    extraction_notes: string | null
    error_reason: string | null
}

export interface ParseEventsResponse {
    events: LlmParsedEvent[]
}

export interface EventInsertRow {
    source_item_id: number
    title: string
    body: string | null
    start_dt: string
    end_dt: string | null
    is_all_day: boolean
    notice_dt: string | null
    external_link: string | null
    category_id: number
}

export const CategoryIdMap: Record<EventCategory, number> = {
    ACADEMIC: 1,
    CLASS: 1,
    EXAM: 1,
    EVENT: 2,
    FINANCE: 3,
    CAREER: 4,
    ACTIVITY: 6,
    PERSONAL: 6,
    OTHER: 7,
}
