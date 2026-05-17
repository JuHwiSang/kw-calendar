import type { LlmParsedEvent } from "./types.ts"

export async function parseEventsWithLlm(
    content: string,
    sourceItemId: number,
): Promise<LlmParsedEvent[]> {
    const useMockLlm = Deno.env.get("USE_MOCK_LLM") === "true"

    if (useMockLlm) {
        console.log("[LLM_MOCK] parseEventsWithLlm called")

        return [
            {
                source_item_id: sourceItemId,
                title: content.replace(/<[^>]*>/g, " ").trim().slice(0, 50),
                body: `${content} 테스트 이벤트입니다.`,
                start_dt: new Date().toISOString(),
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
                extraction_notes: "Mock response for local test.",
                error_reason: null,
            },
        ]
    }

    console.log("[LLM_REAL] Gemini call started")

    const apiKey = Deno.env.get("GEMINI_API_KEY")

    if (!apiKey) {
        throw new Error("GEMINI_API_KEY is missing")
    }

    // 여기는 기존 Gemini 실제 호출 코드 유지
}
