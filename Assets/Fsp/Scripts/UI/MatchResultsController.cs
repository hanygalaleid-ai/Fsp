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

        private void Awake()
        {
            if (matchManager == null) matchManager = FindObjectOfType<MatchManager>();
            if (localPlayer == null)
            {
                foreach (var participant in FindObjectsOfType<MatchParticipant>())
                {
                    if (participant != null && participant.IsLocalPlayer)
                    {
                        localPlayer = participant;
                        break;
                    }
                }
            }

            if (panel != null) panel.SetActive(false);
            if (returnButton != null) returnButton.onClick.AddListener(ReturnToLobby);
        }

        private void OnEnable()
        {
            KillFeedBus.ResetForMatch();
            if (matchManager != null)
            {
                matchManager.PhaseChanged += OnPhaseChanged;
                matchManager.ParticipantEliminated += OnParticipantEliminated;
            }
        }

        private void OnDisable()
        {
            if (matchManager != null)
            {
                matchManager.PhaseChanged -= OnPhaseChanged;
                matchManager.ParticipantEliminated -= OnParticipantEliminated;
            }
            if (returnButton != null) returnButton.onClick.RemoveListener(ReturnToLobby);
        }

        private void OnParticipantEliminated(MatchParticipant participant, int placement)
        {
            if (participant != null && participant.IsLocalPlayer)
                Show(placement);
        }

        private void OnPhaseChanged(MatchManager.MatchPhase phase)
        {
            if (phase != MatchManager.MatchPhase.Finished || shown) return;
            int placement = localPlayer != null && localPlayer.Placement > 0 ? localPlayer.Placement : 1;
            Show(placement);
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

            MobileInputBridge input = MobileInputBridge.Instance;
            if (input != null)
            {
                input.SetMove(Vector2.zero);
                input.SetFire(false);
                input.SetAim(false);
                input.SetSprint(false);
            }
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
            SceneManager.LoadScene("Lobby");
        }

        public void Configure(GameObject panelRoot, Text title, Text placement, Text kills, Text xp, Button returnToLobby)
        {
            panel = panelRoot;
            titleText = title;
            placementText = placement;
            killsText = kills;
            xpText = xp;
            if (returnButton != null) returnButton.onClick.RemoveListener(ReturnToLobby);
            returnButton = returnToLobby;
            if (returnButton != null) returnButton.onClick.AddListener(ReturnToLobby);
            if (panel != null) panel.SetActive(false);
        }
    }
}
