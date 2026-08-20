import { createClient } from "https://esm.sh/@supabase/supabase-js@2.55.0";

Deno.serve(async (request) => {
  if (request.method !== "POST") {
    return new Response(JSON.stringify({ error: "Method not allowed" }), {
      status: 405,
      headers: { "content-type": "application/json" },
    });
  }

  const authorization = request.headers.get("Authorization") ?? "";
  if (!authorization.startsWith("Bearer ")) {
    return new Response(JSON.stringify({ error: "Unauthorized" }), {
      status: 401,
      headers: { "content-type": "application/json" },
    });
  }

  const supabaseUrl = Deno.env.get("SUPABASE_URL");
  const serviceRoleKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY");
  if (!supabaseUrl || !serviceRoleKey) {
    return new Response(JSON.stringify({ error: "Server configuration unavailable" }), {
      status: 500,
      headers: { "content-type": "application/json" },
    });
  }

  const admin = createClient(supabaseUrl, serviceRoleKey, {
    auth: { persistSession: false, autoRefreshToken: false },
  });
  const token = authorization.slice("Bearer ".length).trim();
  const { data: userData, error: userError } = await admin.auth.getUser(token);
  if (userError || !userData.user) {
    return new Response(JSON.stringify({ error: "Invalid session" }), {
      status: 401,
      headers: { "content-type": "application/json" },
    });
  }

  const userId = userData.user.id;

  // Remove squad-owned rows before deleting the leader. This works whether or not the
  // production schema uses cascading foreign keys and prevents orphaned squads/invites.
  const { data: ledSquads, error: ledSquadsError } = await admin
    .from("squads")
    .select("id")
    .eq("leader_user_id", userId);
  if (ledSquadsError && ledSquadsError.code !== "42P01") {
    console.error("Failed to read led squads", ledSquadsError);
    return new Response(JSON.stringify({ error: "Could not delete account data" }), {
      status: 500,
      headers: { "content-type": "application/json" },
    });
  }

  for (const squad of ledSquads ?? []) {
    for (const table of ["squad_invites", "squad_members", "matchmaking_tickets"] as const) {
      const { error } = await admin.from(table).delete().eq("squad_id", squad.id);
      if (error && error.code !== "42P01") {
        console.error(`Failed to remove ${table}.squad_id`, error);
        return new Response(JSON.stringify({ error: "Could not delete account data" }), {
          status: 500,
          headers: { "content-type": "application/json" },
        });
      }
    }
    const { error } = await admin.from("squads").delete().eq("id", squad.id);
    if (error && error.code !== "42P01") {
      console.error("Failed to remove led squad", error);
      return new Response(JSON.stringify({ error: "Could not delete account data" }), {
        status: 500,
        headers: { "content-type": "application/json" },
      });
    }
  }

  const ownedRows = [
    ["squad_invites", "inviter_user_id"],
    ["squad_invites", "invitee_user_id"],
    ["squad_members", "user_id"],
    ["matchmaking_tickets", "user_id"],
    ["match_room_members", "user_id"],
    ["player_cosmetics", "user_id"],
    ["player_directory", "user_id"],
    ["profiles", "user_id"],
  ] as const;

  for (const [table, column] of ownedRows) {
    const { error } = await admin.from(table).delete().eq(column, userId);
    if (error && error.code !== "42P01") {
      console.error(`Failed to remove ${table}.${column}`, error);
      return new Response(JSON.stringify({ error: "Could not delete account data" }), {
        status: 500,
        headers: { "content-type": "application/json" },
      });
    }
  }

  const { error: deleteError } = await admin.auth.admin.deleteUser(userId);
  if (deleteError) {
    return new Response(JSON.stringify({ error: "Could not delete account" }), {
      status: 500,
      headers: { "content-type": "application/json" },
    });
  }

  return new Response(JSON.stringify({ deleted: true }), {
    status: 200,
    headers: { "content-type": "application/json" },
  });
});
