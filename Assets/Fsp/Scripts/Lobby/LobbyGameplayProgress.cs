using UnityEngine;
using Fsp.Localization;

namespace Fsp.Lobby
{
    /// <summary>Persistent local lobby selections and daily mission progress used by the real match runtime.</summary>
    public static class LobbyGameplayProgress
    {
        private const string LoadoutKey = "fsp.loadout";
        private const string MatchesKey = "fsp.mission.matches";
        private const string KillsKey = "fsp.mission.kills";
        private const string TopTenKey = "fsp.mission.top10";

        public static int LoadoutIndex => Mathf.Clamp(PlayerPrefs.GetInt(LoadoutKey, 0), 0, 2);
        public static int Matches => Mathf.Max(0, PlayerPrefs.GetInt(MatchesKey, 0));
        public static int Kills => Mathf.Max(0, PlayerPrefs.GetInt(KillsKey, 0));
        public static int TopTen => Mathf.Max(0, PlayerPrefs.GetInt(TopTenKey, 0));

        public static string LoadoutName => LoadoutIndex switch
        {
            1 => "SCOUT",
            2 => "HEAVY",
            _ => "ASSAULT"
        };

        public static int CycleLoadout()
        {
            int next = (LoadoutIndex + 1) % 3;
            PlayerPrefs.SetInt(LoadoutKey, next);
            PlayerPrefs.Save();
            return next;
        }

        public static void RecordMatch(int kills, int placement)
        {
            PlayerPrefs.SetInt(MatchesKey, Matches + 1);
            PlayerPrefs.SetInt(KillsKey, Kills + Mathf.Max(0, kills));
            if (placement > 0 && placement <= 10) PlayerPrefs.SetInt(TopTenKey, TopTen + 1);
            PlayerPrefs.Save();
        }

        public static string MissionSummary =>
            $"{FspLocalizationRuntime.T("PLAY MATCHES")}   {Mathf.Min(Matches, 3)} / 3\n" +
            $"{FspLocalizationRuntime.T("ELIMINATE ENEMIES")}   {Mathf.Min(Kills, 5)} / 5\n" +
            $"{FspLocalizationRuntime.T("FINISH TOP 10")}   {Mathf.Min(TopTen, 1)} / 1";
    }
}
