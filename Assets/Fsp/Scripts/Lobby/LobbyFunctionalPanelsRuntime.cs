using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Lobby
{
    /// <summary>Turns the runtime-generated lobby navigation and squad slots into functional UI.</summary>
    public sealed class LobbyFunctionalPanelsRuntime : MonoBehaviour
    {
        private Font font;
        private Canvas canvas;
        private GameObject modal;
        private Text modalTitle;
        private Text modalBody;
        private InputField inviteInput;
        private Button inviteSend;
        private float retryUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<LobbyFunctionalPanelsRuntime>() == null)
                new GameObject("LobbyFunctionalPanelsRuntime").AddComponent<LobbyFunctionalPanelsRuntime>();
        }

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            retryUntil = Time.unscaledTime + 10f;
        }

        private void Update()
        {
            if (canvas == null)
            {
                GameObject lobbyCanvas = GameObject.Find("LobbyCanvas");
                if (lobbyCanvas != null) canvas = lobbyCanvas.GetComponent<Canvas>();
            }
            if (canvas == null)
            {
                if (Time.unscaledTime > retryUntil) enabled = false;
                return;
            }

            WireButton("LOADOUT", () => ShowInfo("LOADOUT", "PRIMARY: DUNE AR-4\nAMMO: 30 / 120\nMEDKITS: 2\n\nYour combat loadout is equipped for the next match."));
            WireButton("APPEARANCE", () => ShowInfo("APPEARANCE", "Select your operative with the left/right arrows in the lobby.\n\nCosmetic equipment is saved through the appearance system when backend cosmetics are available."));
            WireButton("CAREER", () => ShowCareer());
            for (int i = 1; i < 4; i++) WireInviteSlot("Slot" + i);
            enabled = false;
        }

        private void WireButton(string name, UnityEngine.Events.UnityAction action)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) return;
            Button button = go.GetComponent<Button>();
            if (button == null) button = go.AddComponent<Button>();
            if (go.GetComponent<LobbyFunctionalMarker>() != null) return;
            go.AddComponent<LobbyFunctionalMarker>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void WireInviteSlot(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null || go.GetComponent<LobbyFunctionalMarker>() != null) return;
            go.AddComponent<LobbyFunctionalMarker>();
            Button button = go.GetComponent<Button>();
            if (button == null) button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(ShowInvite);
        }

        private void ShowCareer()
        {
            PlayerProfile profile = FindFirstObjectByType<PlayerProfile>();
            string body = profile == null
                ? "RANK 01\nMATCHES 0\nKILLS 0\nWINS 0"
                : $"RANK {profile.Level:00}\nMATCHES {profile.MatchesPlayed}\nKILLS {profile.Kills}\nWINS {profile.Wins}";
            ShowInfo("CAREER", body);
        }

        private void ShowInfo(string title, string body)
        {
            EnsureModal();
            inviteInput.gameObject.SetActive(false);
            inviteSend.gameObject.SetActive(false);
            modalTitle.text = title;
            modalBody.text = body;
            modal.SetActive(true);
        }

        private void ShowInvite()
        {
            EnsureModal();
            modalTitle.text = "INVITE PLAYER";
            modalBody.text = "Enter the player name to send a squad invite.";
            inviteInput.text = string.Empty;
            inviteInput.gameObject.SetActive(true);
            inviteSend.gameObject.SetActive(true);
            modal.SetActive(true);
        }

        private void SendInvite()
        {
            string playerName = inviteInput.text.Trim();
            if (string.IsNullOrWhiteSpace(playerName))
            {
                modalBody.text = "Enter a player name first.";
                return;
            }

            SquadLobbyController controller = FindFirstObjectByType<SquadLobbyController>();
            if (controller == null)
            {
                modalBody.text = "Squad service is not connected in this build.";
                return;
            }

            controller.InviteName(playerName);
            modalBody.text = "Sending invite to " + playerName + "...";
        }

        private void EnsureModal()
        {
            if (modal != null) return;
            modal = new GameObject("LobbyModal", typeof(RectTransform), typeof(Image));
            modal.transform.SetParent(canvas.transform, false);
            RectTransform rt = modal.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.30f, 0.22f);
            rt.anchorMax = new Vector2(0.70f, 0.78f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            modal.GetComponent<Image>().color = new Color(0.025f, 0.05f, 0.082f, 0.98f);

            modalTitle = MakeText(modal.transform, "ModalTitle", "", new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.94f), 30);
            modalBody = MakeText(modal.transform, "ModalBody", "", new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.75f), 21);
            modalBody.alignment = TextAnchor.UpperLeft;

            GameObject inputGo = new GameObject("InviteName", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputGo.transform.SetParent(modal.transform, false);
            RectTransform irt = inputGo.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.08f, 0.22f); irt.anchorMax = new Vector2(0.68f, 0.33f);
            irt.offsetMin = Vector2.zero; irt.offsetMax = Vector2.zero;
            inputGo.GetComponent<Image>().color = new Color(0.06f, 0.10f, 0.15f, 1f);
            inviteInput = inputGo.GetComponent<InputField>();
            Text inputText = MakeText(inputGo.transform, "Text", "", new Vector2(0.05f, 0f), new Vector2(0.95f, 1f), 20);
            inviteInput.textComponent = inputText;
            inviteInput.targetGraphic = inputGo.GetComponent<Image>();
            inviteInput.characterLimit = 18;

            inviteSend = MakeButton(modal.transform, "InviteSend", "SEND", new Vector2(0.70f, 0.22f), new Vector2(0.92f, 0.33f));
            inviteSend.onClick.AddListener(SendInvite);
            Button close = MakeButton(modal.transform, "ModalClose", "CLOSE", new Vector2(0.36f, 0.07f), new Vector2(0.64f, 0.17f));
            close.onClick.AddListener(() => modal.SetActive(false));
            modal.SetActive(false);
        }

        private Text MakeText(Transform parent, string name, string value, Vector2 min, Vector2 max, int size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            Text text = go.GetComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size; text.color = new Color(0.96f, 0.94f, 0.89f, 1f);
            text.alignment = TextAnchor.MiddleCenter; text.resizeTextForBestFit = true; text.resizeTextMinSize = 12; text.resizeTextMaxSize = size;
            return text;
        }

        private Button MakeButton(Transform parent, string name, string label, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.78f, 0.45f, 0.17f, 1f);
            MakeText(go.transform, "Label", label, Vector2.zero, Vector2.one, 18);
            return go.GetComponent<Button>();
        }
    }

    public sealed class LobbyFunctionalMarker : MonoBehaviour { }
}
