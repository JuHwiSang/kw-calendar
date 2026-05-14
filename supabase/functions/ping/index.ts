import "@supabase/functions-js"
import { handler } from "./handler.ts"

Deno.serve(handler)
