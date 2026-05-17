import type { EventInsertRow } from "./types.ts"

type ExistingEventForDuplicateCheck = {
    title: string
    start_dt: string
    end_dt: string | null
    category_id: number
}

function normalizeTitle(title: string): string {
    return title
        .toLowerCase()
        .replace(/\[[^\]]*\]/g, "")
        .replace(/\([^)]*\)/g, "")
        .replace(/공지|안내|모집|신청|접수|마감/g, "")
        .replace(/\s+/g, "")
        .replace(/[^\p{L}\p{N}]/gu, "")
}

function tokenizeTitle(title: string): Set<string> {
    const normalized = title
        .toLowerCase()
        .replace(/[^\p{L}\p{N}\s]/gu, " ")
        .replace(/\s+/g, " ")
        .trim()

    if (!normalized) {
        return new Set()
    }

    return new Set(
        normalized
            .split(" ")
            .map((token) => token.trim())
            .filter((token) => token.length >= 2),
    )
}

function calculateJaccardSimilarity(a: Set<string>, b: Set<string>): number {
    if (a.size === 0 || b.size === 0) {
        return 0
    }

    let intersectionCount = 0

    for (const token of a) {
        if (b.has(token)) {
            intersectionCount += 1
        }
    }

    const unionCount = new Set([...a, ...b]).size

    return intersectionCount / unionCount
}

function isSameDate(a: string, b: string): boolean {
    const dateA = new Date(a).toISOString().slice(0, 10)
    const dateB = new Date(b).toISOString().slice(0, 10)

    return dateA === dateB
}

function isSameEndDate(
    newEndDt: string | null,
    existingEndDt: string | null,
): boolean {
    if (!newEndDt && !existingEndDt) {
        return true
    }

    if (!newEndDt || !existingEndDt) {
        return false
    }

    return isSameDate(newEndDt, existingEndDt)
}

function isTitleSimilar(newTitle: string, existingTitle: string): boolean {
    const normalizedNewTitle = normalizeTitle(newTitle)
    const normalizedExistingTitle = normalizeTitle(existingTitle)

    if (!normalizedNewTitle || !normalizedExistingTitle) {
        return false
    }

    if (
        normalizedNewTitle.includes(normalizedExistingTitle) ||
        normalizedExistingTitle.includes(normalizedNewTitle)
    ) {
        return true
    }

    const newTokens = tokenizeTitle(newTitle)
    const existingTokens = tokenizeTitle(existingTitle)
    const similarity = calculateJaccardSimilarity(newTokens, existingTokens)

    return similarity >= 0.6
}

export function isDuplicateEvent(
    newEvent: EventInsertRow,
    existingEvent: ExistingEventForDuplicateCheck,
): boolean {
    const sameCategory = newEvent.category_id === existingEvent.category_id
    const sameStartDate = isSameDate(newEvent.start_dt, existingEvent.start_dt)
    const sameEndDate = isSameEndDate(newEvent.end_dt, existingEvent.end_dt)
    const titleSimilar = isTitleSimilar(newEvent.title, existingEvent.title)

    return titleSimilar && sameCategory && sameStartDate && sameEndDate
}

export function filterDuplicateEvents(
    newEvents: EventInsertRow[],
    existingEvents: ExistingEventForDuplicateCheck[],
): EventInsertRow[] {
    return newEvents.filter((newEvent) => {
        const duplicated = existingEvents.some((existingEvent) =>
            isDuplicateEvent(newEvent, existingEvent)
        )

        if (duplicated) {
            console.log("[DUPLICATE_EVENT_SKIPPED]", {
                title: newEvent.title,
                start_dt: newEvent.start_dt,
                end_dt: newEvent.end_dt,
                category_id: newEvent.category_id,
            })
        }

        return !duplicated
    })
}
