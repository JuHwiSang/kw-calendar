export function handler(_req: Request): Response {
  return new Response(
    JSON.stringify({ message: "pong" }),
    { headers: { "Content-Type": "application/json" } },
  )
}
