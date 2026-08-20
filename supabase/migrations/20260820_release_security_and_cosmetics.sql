-- Release hardening applied to izjdvlkwuqgtawwbksun on 2026-08-20.

alter policy profiles_insert_own on public.profiles
with check (
  (select auth.uid()) = user_id
  and head_item_id in ('head_default','head_sand','head_night')
  and face_item_id in ('face_none','face_amber','face_ice')
  and torso_item_id in ('torso_default','torso_sand','torso_night')
  and legs_item_id in ('legs_default','legs_sand','legs_night')
  and backpack_item_id in ('backpack_default','backpack_sand','backpack_rescue')
  and parachute_item_id in ('parachute_default','parachute_sand','parachute_night')
);

alter policy profiles_update_own on public.profiles
using ((select auth.uid()) = user_id)
with check (
  (select auth.uid()) = user_id
  and head_item_id in ('head_default','head_sand','head_night')
  and face_item_id in ('face_none','face_amber','face_ice')
  and torso_item_id in ('torso_default','torso_sand','torso_night')
  and legs_item_id in ('legs_default','legs_sand','legs_night')
  and backpack_item_id in ('backpack_default','backpack_sand','backpack_rescue')
  and parachute_item_id in ('parachute_default','parachute_sand','parachute_night')
);

-- Remove older permissive policies that bypassed squad-leader checks.
drop policy if exists tickets_insert_own on public.matchmaking_tickets;
drop policy if exists tickets_update_own on public.matchmaking_tickets;

-- Preserve invitee accept/decline and inviter cancel behavior with one policy.
drop policy if exists invites_update_invitee on public.squad_invites;
drop policy if exists invites_update_inviter_cancel on public.squad_invites;
drop policy if exists invites_update_related on public.squad_invites;
create policy invites_update_related on public.squad_invites
for update to authenticated
using (invitee_user_id = (select auth.uid()) or inviter_user_id = (select auth.uid()))
with check (
  (invitee_user_id = (select auth.uid()) and status in ('accepted','declined'))
  or (inviter_user_id = (select auth.uid()) and status = 'cancelled')
);

create index if not exists match_room_members_user_id_idx on public.match_room_members(user_id);
create index if not exists matchmaking_tickets_squad_id_idx on public.matchmaking_tickets(squad_id);
create index if not exists squad_invites_invitee_user_id_idx on public.squad_invites(invitee_user_id);
create index if not exists squad_invites_inviter_user_id_idx on public.squad_invites(inviter_user_id);
create index if not exists squad_invites_squad_id_idx on public.squad_invites(squad_id);
create index if not exists squads_leader_user_id_idx on public.squads(leader_user_id);
