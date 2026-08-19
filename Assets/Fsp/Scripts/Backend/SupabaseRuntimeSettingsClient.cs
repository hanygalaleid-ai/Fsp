using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Fsp.Backend
{
    public sealed class SupabaseRuntimeSettingsClient : MonoBehaviour
    {
        [Serializable] private sealed class RuntimeRow
        {
            public string key;
            public string value;
        }

        [Serializable] private sealed class RuntimeRows
        {
            public RuntimeRow[] items;
        }

        public IEnumerator GetValue(string key, Action<bool, string> done)
        {
            if (!SupabaseSession.IsSignedIn)
            {
                done?.Invoke(false, "Not signed in.");
                yield break;
            }

            string url = SupabaseRuntimeConfig.ProjectUrl
                + "/rest/v1/app_runtime_config?key=eq."
                + UnityWebRequest.EscapeURL(key)
                + "&select=key,value&limit=1";

            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("apikey", SupabaseRuntimeConfig.PublishableKey);
            req.SetRequestHeader("Authorization", "Bearer " + SupabaseSession.AccessToken);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                done?.Invoke(false, req.downloadHandler.text);
                yield break;
            }

            var wrapped = JsonUtility.FromJson<RuntimeRows>("{\"items\":" + req.downloadHandler.text + "}");
            if (wrapped?.items == null || wrapped.items.Length == 0)
            {
                done?.Invoke(false, "Runtime setting not found: " + key);
                yield break;
            }

            done?.Invoke(true, wrapped.items[0].value ?? string.Empty);
        }
    }
}
