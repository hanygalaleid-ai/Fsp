using System;
using System.Collections;
using Fsp.BattleRoyale;
using Fsp.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Core
{
    public sealed class AndroidVisualRecoveryRuntime : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<AndroidVisualRecoveryRuntime>() != null) return;
            var go = new GameObject("FSP_AndroidVisualRecovery");
            DontDestroyOnLoad(go);
            go.AddComponent<AndroidVisualRecoveryRuntime>();
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(ApplyAfterFrame(SceneManager.GetActiveScene().name));
        }

        private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyAfterFrame(scene.name));

        private IEnumerator ApplyAfterFrame(string scene)
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            if (scene.Equals("Lobby", StringComparison.OrdinalIgnoreCase)) FixLobbyBackdrop();
            else if (scene.Equals("Match", StringComparison.OrdinalIgnoreCase)) FixMatch();
        }

        private static void FixLobbyBackdrop()
        {
            GameObject canvas = GameObject.Find("LobbyCanvas");
            if (canvas == null) return;
            foreach (RawImage raw in canvas.GetComponentsInChildren<RawImage>(true))
            {
                if (raw == null || raw.gameObject.name != "SunscarBackdrop") continue;
                raw.texture = BuildLobbyArt(1280, 720);
                raw.color = Color.white;
                raw.uvRect = new Rect(0f, 0f, 1f, 1f);
                break;
            }
        }

        private static Texture2D BuildLobbyArt(int w, int h)
        {
            Texture2D t = new Texture2D(w, h, TextureFormat.RGB24, false, false);
            Color32[] p = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = y / (float)(h - 1);
                Color c = v < 0.55f
                    ? Color.Lerp(new Color(0.035f, 0.075f, 0.12f), new Color(0.92f, 0.38f, 0.08f), v / 0.55f)
                    : Color.Lerp(new Color(0.32f, 0.18f, 0.09f), new Color(0.025f, 0.03f, 0.04f), (v - 0.55f) / 0.45f);
                for (int x = 0; x < w; x++) p[y * w + x] = c;
            }
            t.SetPixels32(p);
            DrawDisc(t, 980, 275, 72, new Color32(255, 183, 64, 255));
            for (int x = 0; x < w; x++)
            {
                int ridge = 390 + (int)(34f * Mathf.Sin(x * 0.010f) + 20f * Mathf.Sin(x * 0.023f));
                for (int y = ridge; y < Mathf.Min(h, ridge + 85); y++) t.SetPixel(x, y, new Color32(94, 55, 31, 255));
            }
            int cx = 760;
            DrawRing(t, cx, 420, 180, 8, new Color32(255, 136, 24, 210));
            DrawDisc(t, cx, 315, 38, new Color32(20, 24, 28, 255));
            DrawRect(t, cx - 45, 350, 90, 145, new Color32(18, 22, 25, 255));
            DrawRect(t, cx - 68, 365, 22, 125, new Color32(24, 28, 31, 255));
            DrawRect(t, cx + 46, 365, 22, 125, new Color32(24, 28, 31, 255));
            DrawRect(t, cx - 38, 490, 28, 125, new Color32(16, 19, 22, 255));
            DrawRect(t, cx + 10, 490, 28, 125, new Color32(16, 19, 22, 255));
            DrawRect(t, cx + 48, 398, 115, 14, new Color32(12, 15, 17, 255));
            DrawRect(t, cx + 140, 390, 28, 30, new Color32(10, 12, 14, 255));
            t.Apply(false, false);
            t.wrapMode = TextureWrapMode.Clamp;
            return t;
        }

        private static void FixMatch()
        {
            foreach (DropPlaneController plane in FindObjectsByType<DropPlaneController>(FindObjectsSortMode.None))
                if (plane != null) plane.gameObject.SetActive(false);

            foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
            {
                if (participant == null || !participant.IsLocalPlayer) continue;
                participant.transform.SetParent(null, true);
                participant.transform.position = new Vector3(0f, 1.1f, 0f);
                participant.transform.rotation = Quaternion.identity;

                DropPlanePassenger passenger = participant.GetComponent<DropPlanePassenger>();
                if (passenger != null)
                {
                    passenger.enabled = false;
                    Destroy(passenger);
                }
                ParachuteController parachute = participant.GetComponent<ParachuteController>();
                if (parachute != null) parachute.enabled = false;
                ThirdPersonMotor motor = participant.GetComponent<ThirdPersonMotor>();
                if (motor != null) motor.enabled = true;
                StarterThirdPersonRig rig = participant.GetComponent<StarterThirdPersonRig>();
                if (rig != null) rig.enabled = true;
                break;
            }

            Shader unlit = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
            if (unlit != null)
            {
                foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                {
                    if (r == null || !r.enabled) continue;
                    string n = r.gameObject.name;
                    Color color;
                    if (n.IndexOf("Ground", StringComparison.OrdinalIgnoreCase) >= 0) color = new Color(0.43f, 0.27f, 0.12f, 1f);
                    else if (n.IndexOf("Road", StringComparison.OrdinalIgnoreCase) >= 0) color = new Color(0.18f, 0.14f, 0.11f, 1f);
                    else if (n.IndexOf("Rock", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Quarry", StringComparison.OrdinalIgnoreCase) >= 0) color = new Color(0.29f, 0.24f, 0.20f, 1f);
                    else continue;
                    Material m = new Material(unlit) { name = "FSP_ANDROID_SAFE_" + n };
                    if (m.HasProperty("_Color")) m.SetColor("_Color", color);
                    r.material = m;
                }
            }

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.allowHDR = false;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.20f, 0.34f, 0.48f, 1f);
                cam.nearClipPlane = 0.08f;
                cam.farClipPlane = 1800f;
            }
        }

        private static void DrawRect(Texture2D t, int x, int y, int w, int h, Color32 c)
        {
            int x0 = Mathf.Clamp(x, 0, t.width), x1 = Mathf.Clamp(x + w, 0, t.width);
            int y0 = Mathf.Clamp(y, 0, t.height), y1 = Mathf.Clamp(y + h, 0, t.height);
            for (int yy = y0; yy < y1; yy++) for (int xx = x0; xx < x1; xx++) t.SetPixel(xx, yy, c);
        }

        private static void DrawDisc(Texture2D t, int cx, int cy, int r, Color32 c)
        {
            int rr = r * r;
            for (int y = -r; y <= r; y++) for (int x = -r; x <= r; x++)
                if (x*x + y*y <= rr && cx+x >= 0 && cx+x < t.width && cy+y >= 0 && cy+y < t.height) t.SetPixel(cx+x, cy+y, c);
        }

        private static void DrawRing(Texture2D t, int cx, int cy, int r, int thickness, Color32 c)
        {
            int outer = r*r, inner = (r-thickness)*(r-thickness);
            for (int y = -r; y <= r; y++) for (int x = -r; x <= r; x++)
            {
                int d = x*x + y*y;
                if (d <= outer && d >= inner && cx+x >= 0 && cx+x < t.width && cy+y >= 0 && cy+y < t.height) t.SetPixel(cx+x, cy+y, c);
            }
        }
    }
}
