import { assertEquals } from "@std/assert"
import { handler } from "./handler.ts"

Deno.test("ping: returns pong message", async () => {
  const res = handler(new Request("http://localhost/ping"))
  const body = await res.json()
  assertEquals(body.message, "pong")
})

Deno.test("ping: returns application/json content type", () => {
  const res = handler(new Request("http://localhost/ping"))
  assertEquals(res.headers.get("Content-Type"), "application/json")
})
