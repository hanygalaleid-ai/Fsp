# FSP account deletion

Deploy this authenticated Edge Function before the Google Play production release:

```bash
supabase functions deploy delete-account
```

The function validates the caller's access token, removes squads led by the player and their dependent rows, deletes known player-owned profile, directory, matchmaking, match-room, cosmetic, invite and membership rows, then deletes the Supabase Auth user. Never expose `SUPABASE_SERVICE_ROLE_KEY` in the Unity client.
