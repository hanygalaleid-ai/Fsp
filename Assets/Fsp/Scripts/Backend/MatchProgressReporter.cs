using System.Threading;
using Fsp.BattleRoyale;
using Fsp.Lobby;
using Fsp.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Backend
{
    public sealed class MatchProgressReporter : MonoBehaviour
    {
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private MatchParticipant localParticipant;
        [SerializeField] private SupabaseProfileStore profileStore;

        private bool saved;
        private bool saving;
        private bool subscribed;
        private bool preservePendingSaveAcrossSceneExit;

        public bool IsSaving => saving;
        public bool IsSaved => saved;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad() => EnsureInstalled();

        public static MatchProgressReporter EnsureInstalled()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.Equals(scene.name, "Match", System.StringComparison.OrdinalIgnoreCase)) return null;

            MatchProgressReporter existing = FindFirstObjectByType<MatchProgressReporter>();
            if (existing != null)
            {
                existing.ResolveRuntimeSources();
                existing.Subscribe();
                return existing;
            }

            GameObject host = GameObject.Find("MatchProgressReporter") ?? new GameObject("MatchProgressReporter");
            return host.AddComponent<MatchProgressReporter>();
        }

        private void Awake() => ResolveRuntimeSources();

        private void Start()
        {
            ResolveRuntimeSources();
            Subscribe();
        }

        private void OnEnable()
        {
            ResolveRuntimeSources();
            Subscribe();
        }

        private void OnDisable() => Unsubscribe();

        public void PreservePendingSaveForSceneExit()
        {
            if (!saving || saved || preservePendingSaveAcrossSceneExit) return;
            preservePendingSaveAcrossSceneExit = true;
            DontDestroyOnLoad(gameObject);
            Debug.Log("FSP progress: preserving pending save while returning to Lobby.");
        }

        private void ResolveRuntimeSources()
        {
            if (matchManager == null) matchManager = MatchManager.Instance ?? FindFirstObjectByType<MatchManager>();
            if (profileStore == null) profileStore = GetComponent<SupabaseProfileStore>();
            if (profileStore == null) profileStore = gameObject.AddComponent<SupabaseProfileStore>();

            if (localParticipant != null && localParticipant.IsLocalPlayer) return;
            localParticipant = null;
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                if (participant == null || !participant.IsLocalPlayer) continue;
                localParticipant = participant;
                break;
            }
        }

        private void Subscribe()
        {
            if (subscribed) return;
            ResolveRuntimeSources();
            if (matchManager == null) return;
            matchManager.ParticipantEliminated += HandleParticipantEliminated;
            matchManager.MatchWon += HandleMatchWon;
            matchManager.NetworkWinnerDeclared += HandleNetworkWinner;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || matchManager == null) return;
            matchManager.ParticipantEliminated -= HandleParticipantEliminated;
            matchManager.MatchWon -= HandleMatchWon;
            matchManager.NetworkWinnerDeclared -= HandleNetworkWinner;
            subscribed = false;
        }

        private void HandleParticipantEliminated(MatchParticipant participant, int placement)
        {
            if (participant != null && participant.IsLocalPlayer)
                SaveResult(false, Mathf.Max(1, placement));
        }

        private void HandleMatchWon(MatchParticipant winner)
        {
            if (matchManager != null && matchManager.NetworkAuthoritative) return;
            if (winner != null && winner.IsLocalPlayer)
                SaveResult(true, 1);
        }

        private void HandleNetworkWinner(string winnerId)
        {
            if (!SupabaseSession.IsSignedIn) return;
            bool won = !string.IsNullOrWhiteSpace(winnerId) && winnerId == SupabaseSession.UserId;
            int placement = won ? 1 : Mathf.Max(2, localParticipant != null ? localParticipant.Placement : 2);
            SaveResult(won, placement);
        }

        private async void SaveResult(bool won, int placement)
        {
            if (saved || saving || !SupabaseSession.IsSignedIn) return;
            ResolveRuntimeSources();
            if (profileStore == null || localParticipant == null) return;

            saving = true;
            try
            {
                using var timeout = new CancellationTokenSource();
                timeout.CancelAfter(12000);

                string userId = SupabaseSession.UserId;
                int matchKills = KillFeedBus.LocalPlayerKills;
                string displayName = LobbyState.Instance != null ? LobbyState.Instance.DisplayName : "Player";
                string characterId = LobbyState.Instance != null ? LobbyState.Instance.SelectedCharacterId : "soldier_01";

                PlayerProfile profile = await profileStore.LoadAsync(userId, timeout.Token);
                if (profile == null)
                    profile = new PlayerProfile(userId, displayName, characterId);

                profile.ApplyMatchResult(won, matchKills, Mathf.Max(1, placement));
                await profileStore.SaveAsync(profile, timeout.Token);
                saved = true;
                Debug.Log("FSP progress: match result saved successfully.");
            }
            catch (System.OperationCanceledException)
            {
                Debug.LogWarning("FSP progress: save timed out; gameplay will continue without blocking the results screen.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to save match progress: " + ex.Message);
            }
            finally
            {
                saving = false;
                if (preservePendingSaveAcrossSceneExit)
                {
                    preservePendingSaveAcrossSceneExit = false;
                    Destroy(gameObject);
                }
            }
        }
    }
}
