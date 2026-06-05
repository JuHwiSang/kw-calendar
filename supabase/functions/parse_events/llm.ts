import {
    PARSE_EVENTS_SYSTEM_PROMPT,
    buildParseEventsUserPrompt,
} from "./prompts.ts"
import { parseLlmResponse } from "./parser.ts"
import type { LlmParsedEvent, SourceItem } from "./types.ts"

const GEMINI_MODEL = "gemini-3.5-flash"
const MAX_GEMINI_RETRIES = 3

type GeminiGenerateContentResponse = {
    candidates?: Array<{
        content?: {
            parts?: Array<{
                text?: string
            }>
        }
    }>
}
export class GeminiRateLimitError extends Error {
    constructor(message: string) {
        super(message)
        this.name = "GeminiRateLimitError"
    }
}

function sleep(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms))
}

function getGeminiRetryDelayMs(attempt: number): number {
    return 15_000 * (attempt + 1)
}

function isGeminiRateLimitResponse(status: number, errorText: string): boolean {
    const normalized = errorText.toLowerCase()

    return (
        status === 429 ||
        normalized.includes("rate limit") ||
        normalized.includes("quota")
    )
}

export async function parseEventsWithLlm(
    sourceItem: SourceItem,
): Promise<LlmParsedEvent[]> {
    const useMockLlm = Deno.env.get("USE_MOCK_LLM") === "true"

    if (useMockLlm) {
        console.log("[LLM_MOCK] parseEventsWithLlm called")

        return [
            {
                source_item_id: sourceItem.id,
                title: sourceItem.raw_content
                    .replace(/<[^>]*>/g, " ")
                    .replace(/\s+/g, " ")
                    .trim()
                    .slice(0, 50),
                body: `${sourceItem.raw_content} 테스트 이벤트입니다.`,
                start_dt: new Date().toISOString(),
                end_dt: null,
                is_all_day: false,
                notice_dt: null,
                external_link: sourceItem.source_url,
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

    console.log("[LLM_REAL] Gemini call started", {
        sourceItemId: sourceItem.id,
        sourceType: sourceItem.source_type,
    })

    const apiKey = Deno.env.get("GEMINI_API_KEY")

    if (!apiKey) {
        throw new Error("GEMINI_API_KEY is missing")
    }

    const userPrompt = buildParseEventsUserPrompt({
        id: sourceItem.id,
        source_type: sourceItem.source_type,
        content_type: sourceItem.content_type,
        crawled_at: sourceItem.crawled_at,
        source_url: sourceItem.source_url,
        raw_text: sourceItem.raw_content,
    })

    const llmText = await callGeminiApi(
        PARSE_EVENTS_SYSTEM_PROMPT,
        userPrompt,
        apiKey,
    )

    return parseLlmResponse(llmText, sourceItem.id)
}

async function callGeminiApi(
    systemPrompt: string,
    userPrompt: string,
    apiKey: string,
): Promise<string> {
    const url =
        `https://generativelanguage.googleapis.com/v1beta/models/${GEMINI_MODEL}:generateContent?key=${apiKey}`

    for (let attempt = 0; attempt <= MAX_GEMINI_RETRIES; attempt++) {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                system_instruction: {
                    parts: [
                        {
                            text: systemPrompt,
                        },
                    ],
                },
                contents: [
                    {
                        role: "user",
                        parts: [
                            {
                                text: userPrompt,
                            },
                        ],
                    },
                ],
                generationConfig: {
                    temperature: 0.2,
                    topP: 0.9,
                    maxOutputTokens: 4096,
                    responseMimeType: "application/json",
                },
            }),
        })

        const responseText = await response.text()

        if (response.ok) {
            const data = JSON.parse(responseText) as GeminiGenerateContentResponse
            const text = data.candidates?.[0]?.content?.parts
                ?.map((part) => part.text ?? "")
                .join("")
                .trim()

            if (!text) {
                throw new Error("Gemini API response does not contain text")
            }

            return text
        }

        if (isGeminiRateLimitResponse(response.status, responseText)) {
            if (attempt < MAX_GEMINI_RETRIES) {
                const delayMs = getGeminiRetryDelayMs(attempt)

                console.warn("[GEMINI_RATE_LIMIT_RETRY]", {
                    attempt: attempt + 1,
                    maxRetries: MAX_GEMINI_RETRIES,
                    delayMs,
                    status: response.status,
                })

                await sleep(delayMs)
                continue
            }

            throw new GeminiRateLimitError(
                `Gemini API rate limit exceeded after retries: ${response.status} ${responseText}`,
            )
        }

        throw new Error(
            `Gemini API request failed: ${response.status} ${responseText}`,
        )
    }

    throw new Error("Gemini API request failed after retries")
}

 
