import { createClient } from "@supabase/supabase-js"

export const SUPABASE_URL = Deno.env.get("SUPABASE_URL") ?? ""
export const SUPABASE_SERVICE_ROLE_KEY = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY") ?? ""
export const IG_BUSINESS_ID = Deno.env.get("IG_BUSINESS_ID")
export const IG_ACCESS_TOKEN = Deno.env.get("IG_ACCESS_TOKEN")

// Lazy singleton — deferred so test imports don't trigger createClient with empty URL
let _instance: ReturnType<typeof createClient> | null = null
export function getSupabase() {
  if (!_instance) _instance = createClient(SUPABASE_URL, SUPABASE_SERVICE_ROLE_KEY)
  return _instance
}
