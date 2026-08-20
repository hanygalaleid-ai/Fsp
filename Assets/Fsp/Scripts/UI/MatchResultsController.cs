using Fsp.Backend;
using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Input;
using Fsp.Player;
using Fsp.Vehicles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.UI
{
    public sealed class MatchResultsController : MonoBehaviour
    {
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private MatchParticipant localPlayer;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text placementText;
        [SerializeField] private Text killsText;
        [SerializeField] private Text xpText;
        [SerializeField] private Button returnButton;

        private bool shown;
        private bool subscribed;

        private void Awake()
        {
            ResolveRuntimeSources();
            if (panel != null) panel.SetActive(false);
        }

        private void OnEnable()
        {
            KillFeedBus.ResetForMatch();
            ResolveRuntimeSources();
            Subscribe();
        }

        private void OnDisable() => Unsubscribe();

        private void ResolveRuntimeSources()
        {
            if (matchManager == null) matchManager = FindFirstObjectByType<MatchManager>();
            if (localPlayer != null && localPlayer.IsLocalPlayer) return;

            localPlayer = null;
            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                if (participant != null && participant.IsLocalPlayer)
                {
                    localPlayer = participant;
                    break;
                }
            }
        }

        public void RebindRuntime()
        {
            Unsubscribe();
            matchManager = FindFirstObjectByType<MatchManager>();
            localPlayer = null;
            ResolveRuntimeSources();
            if (isActiveAndEnabled) Subscribe();
        }

        private void Subscribe()
        {
            if (subscribed) return;
            if (matchManager != null)
            {
                matchManager.PhaseChanged += OnPhaseChanged;
                matchManager.ParticipantEliminated += OnParticipantEliminated;
            }
            if (returnButton != null) returnButton.onClick.AddListener(ReturnToLobby);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            if (matchManager != null)
            {
                matchManager.PhaseChanged -= OnPhaseChanged;
                matchManager.ParticipantEliminated -= OnParticipantEliminated;
            }
            if (returnButton != null) returnButton.onClick.RemoveListener(ReturnToLobby);
            subscribed = false;
        }

        private void OnParticipantEliminated(MatchParticipant participant, int placement)
        {
            if (participant != null && participant.IsLocalPlayer) Show(placement);
        }

        private void OnPhaseChanged(MatchManager.MatchPhase phase)
        {
            if (phase != MatchManager.MatchPhase.Finished || shown) return;
            ResolveRuntimeSources();
            Show(ResolveFinishedPlacement());
        }

        private int ResolveFinishedPlacement()
        {
            if (matchManager != null && matchManager.NetworkAuthoritative)
            {
                string winnerId = matchManager.AuthoritativeWinnerId;
                if (!string.IsNullOrWhiteSpace(winnerId) && winnerId == SupabaseSession.UserId)
                    return 1;

                // In authoritative online matches local MatchParticipant.Placement is not assigned by
                // the local manager. Never default an online loser to #1 just because placement is 0.
                if (localPlayer != null && localPlayer.Placement > 1)
                    return localPlayer.Placement;
                return Mathf.Max(2, matchManager.AliveCount + 1);
            }

            return localPlayer != null && localPlayer.Placement > 0 ? localPlayer.Placement : 1;
        }

        private void Show(int placement)
        {
            if (shown) return;
            shown = true;
            int kills = KillFeedBus.LocalPlayerKills;
            int xp = CalculateXp(placement, kills);

            if (panel != null) panel.SetActive(true);
            if (titleText != null) titleText.text = placement == 1 ? "VICTORY" : "MATCH COMPLETE";
            if (placementText != null) placementText.text = $"PLACE #{placement}";
            if (killsText != null) killsText.text = $"KILLS {kills}";
            if (xpText != null) xpText.text = $"XP +{xp}";
            DisableGameplayInput();
        }

        private void DisableGameplayInput()
        {
            ResolveRuntimeSources();
            if (localPlayer != null)
            {
                SetEnabled(localPlayer.GetComponent<StarterCombatInput>(), false);
                SetEnabled(localPlayer.GetComponent<StarterThirdPersonRig>(), false);
                SetEnabled(localPlayer.GetComponent<StarterVehicleInput>(), false);
                SetEnabled(localPlayer.GetComponent<MobileGameplayAdapter>(), false);
                SetEnabled(localPlayer.GetComponentInChildren<ThirdPersonMotor>(), false);
            }

            GameObject mobileHud = GameObject.Find("MobileCombatHUD");
            if (mobileHud != null) mobileHud.SetActive(false);

            MobileInputBridge.Instance?.ResetAll();
        }

        private static void SetEnabled(Behaviour behaviour, bool value)
        {
            if (behaviour != null) behaviour.enabled = value;
        }

        private static int CalculateXp(int placement, int kills)
        {
            placement = Mathf.Max(1, placement);
            kills = Mathf.Max(0, kills);
            int placementXp = Mathf.Max(10, 120 - (placement - 1) * 4);
            return 40 + kills * 25 + placementXp + (placement == 1 ? 200 : 0);
        }

        public void ReturnToLobby()
        {
            MobileInputBridge.Instance?.ResetAll();

            if (MatchRoomState.Instance != null)
                MatchRoomState.Instance.Clear();

            SceneManager.LoadScene("Lobby");
        }

        public void Configure(GameObject panelRoot, Text title, Text placement, Text kills, Text xp, Button returnToLobby)
        {
            Unsubscribe();
            panel = panelRoot;
            titleText = title;
            placementText = placement;
            killsText = kills;
            xpText = xp;
            returnButton = returnToLobby;
            if (panel != null) panel.SetActive(false);
            ResolveRuntimeSources();
            if (isActiveAndEnabled) Subscribe();
        }
    }
}
