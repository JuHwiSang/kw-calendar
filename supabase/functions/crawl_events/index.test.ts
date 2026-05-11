import { assertEquals } from "@std/assert"
import { parseHTML } from "linkedom"

Deno.test("linkedom should parse KW Notice structure", () => {
  const html = `
    <div class="board-list-box">
      <ul>
        <li>
          <span class="category">공지</span>
          <a href="?articleNo=12345&DUID=999">제목</a>
        </li>
      </ul>
    </div>
  `
  // deno-lint-ignore no-explicit-any
  const { document } = parseHTML(html) as any
  const li = document.querySelector(".board-list-box ul li")
  const category = li?.querySelector(".category")?.textContent?.trim()
  const href = li?.querySelector("a")?.getAttribute("href")
  const idMatch = href?.match(/(?:articleNo|DUID)=(\d+)/)

  assertEquals(category, "공지")
  assertEquals(idMatch?.[1], "12345")
})

Deno.test("linkedom should parse KW Academic structure", () => {
  const html = `
    <ul class="bachelor-calendar-list">
      <li>
        <span class="date">05.11</span>
        <span class="content">중간고사</span>
      </li>
    </ul>
  `
  // deno-lint-ignore no-explicit-any
  const { document } = parseHTML(html) as any
  const li = document.querySelector(".bachelor-calendar-list li")
  const date = li?.querySelector(".date")?.textContent?.trim()
  const content = li?.querySelector(".content")?.textContent?.trim()

  assertEquals(date, "05.11")
  assertEquals(content, "중간고사")
})
