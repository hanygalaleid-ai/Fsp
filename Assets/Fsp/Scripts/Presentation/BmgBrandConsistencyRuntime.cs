using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    public sealed class BmgBrandConsistencyRuntime : MonoBehaviour
    {
        private static BmgBrandConsistencyRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("BMG_BrandConsistencyRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BmgBrandConsistencyRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.ApplyDelayed());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyDelayed());

        private IEnumerator ApplyDelayed()
        {
            yield return null;
            yield return new WaitForSecondsRealtime(.25f);
            Apply();
            yield return new WaitForSecondsRealtime(.75f);
            Apply();
        }

        private static void Apply()
        {
            foreach (var text in FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (text == null) continue;
                string value = text.text ?? string.Empty;
                if (value.Contains("FSP // SUNSCAR") || value.Contains("FSP // Sunscar"))
                    text.text = "BMG // BATTLE ROYALE";
                else if (value == "FSP")
                    text.text = "BMG";
            }
        }
    }
}
