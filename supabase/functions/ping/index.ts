import "@supabase/functions-js"

Deno.serve(() => {
  return new Response(
    JSON.stringify({ message: "pong" }),
    { headers: { "Content-Type": "application/json" } },
  )
})
