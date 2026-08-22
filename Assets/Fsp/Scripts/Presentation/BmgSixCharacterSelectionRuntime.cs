using System.Collections;
using Fsp.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>Overrides the old three-character arrows so the approved 3 male + 3 female set cycles as soldier_01..soldier_06.</summary>
    public sealed class BmgSixCharacterSelectionRuntime : MonoBehaviour
    {
        private static BmgSixCharacterSelectionRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var host = new GameObject("BMG_SixCharacterSelectionRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgSixCharacterSelectionRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.BindDelayed());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(BindDelayed());

        private IEnumerator BindDelayed()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) yield break;
            for (int i = 0; i < 12; i++) yield return null;
            BindArrow("Prev", -1);
            BindArrow("Next", 1);
        }

        private static void BindArrow(string name, int delta)
        {
            GameObject modePanel = GameObject.Find("ModePanel");
            if (modePanel == null) return;
            Transform child = modePanel.transform.Find(name);
            Button button = child != null ? child.GetComponent<Button>() : null;
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Cycle(delta));
        }

        private static void Cycle(int delta)
        {
            LobbyState state = LobbyState.Instance;
            if (state == null) return;
            int current = ParseIndex(state.SelectedCharacterId);
            int next = ((current - 1 + delta) % 6 + 6) % 6 + 1;
            state.SetCharacter("soldier_" + next.ToString("00"));
        }

        private static int ParseIndex(string id)
        {
            if (!string.IsNullOrEmpty(id) && id.StartsWith("soldier_", System.StringComparison.OrdinalIgnoreCase))
            {
                string tail = id.Substring("soldier_".Length);
                if (int.TryParse(tail, out int value) && value >= 1 && value <= 6) return value;
            }
            return 1;
        }
    }
}
