import type { EventInsertRow } from "./types.ts"

function normalizeTitle(title: string): string {
    return title
        .toLowerCase()
        .replace(/\s+/g, "")
        .replace(/[^\p{L}\p{N}]/gu, "")
}

function isSameDate(a: string, b: string): boolean {
    const dateA = new Date(a).toISOString().slice(0, 10)
    const dateB = new Date(b).toISOString().slice(0, 10)

    return dateA === dateB
}

export function isDuplicateEvent(
    newEvent: EventInsertRow,
    existingEvent: {
        title: string
        start_dt: string
        end_dt: string | null
        category_id: number
    },
): boolean {
    const newTitle = normalizeTitle(newEvent.title)
    const existingTitle = normalizeTitle(existingEvent.title)

    const titleSimilar =
        newTitle.includes(existingTitle) ||
        existingTitle.includes(newTitle)

    const sameCategory = newEvent.category_id === existingEvent.category_id
    const sameStartDate = isSameDate(newEvent.start_dt, existingEvent.start_dt)

    return titleSimilar && sameCategory && sameStartDate
}

export function filterDuplicateEvents(
    newEvents: EventInsertRow[],
    existingEvents: {
        title: string
        start_dt: string
        end_dt: string | null
        category_id: number
    }[],
): EventInsertRow[] {
    return newEvents.filter((newEvent) => {
        const duplicated = existingEvents.some((existingEvent) =>
            isDuplicateEvent(newEvent, existingEvent)
        )

        if (duplicated) {
            console.log("[DUPLICATE_EVENT_SKIPPED]", {
                title: newEvent.title,
                start_dt: newEvent.start_dt,
                category_id: newEvent.category_id,
            })
        }

        return !duplicated
    })
}
