using System;
using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.UI
{
    /// <summary>Mobile-safe overhead minimap that follows the local participant.</summary>
    public sealed class RuntimeMiniMapInstaller : MonoBehaviour
    {
        private Camera mapCamera;
        private Transform target;
        private RenderTexture mapTexture;
        private float nextRender;
        private float renderInterval;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<RuntimeMiniMapInstaller>() == null)
                new GameObject("Fsp_RuntimeMiniMap").AddComponent<RuntimeMiniMapInstaller>();
        }

        private void Awake()
        {
            int quality = PlayerPrefs.GetInt("fsp_quality", 1);
            int resolution = quality <= 0 ? 128 : quality == 1 ? 192 : 256;
            renderInterval = quality <= 0 ? .5f : quality == 1 ? .25f : .15f;
            mapTexture = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.RGB565)
            {
                name = "FSP_MinimapTexture",
                filterMode = FilterMode.Bilinear,
                useMipMap = false
            };
            mapTexture.Create();

            GameObject cameraObject = new("MinimapCamera");
            cameraObject.transform.SetParent(transform, false);
            mapCamera = cameraObject.AddComponent<Camera>();
            mapCamera.orthographic = true;
            mapCamera.orthographicSize = 58f;
            mapCamera.nearClipPlane = 2f;
            // Keep the terrain visible while the local participant is still aboard the
            // transport plane, not only after landing.
            mapCamera.farClipPlane = 320f;
            mapCamera.clearFlags = CameraClearFlags.SolidColor;
            mapCamera.backgroundColor = new Color(.025f, .045f, .055f, 1f);
            mapCamera.targetTexture = mapTexture;
            mapCamera.depth = -10f;
            mapCamera.enabled = false;
            mapCamera.allowHDR = false;
            mapCamera.allowMSAA = false;

            GameObject canvasObject = new("MinimapCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(MobileSafeArea));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 92;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;

            GameObject frame = new("MinimapFrame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(canvasObject.transform, false);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(.415f, .72f);
            frameRect.anchorMax = new Vector2(.585f, .895f);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            frame.GetComponent<Image>().color = new Color(.015f, .03f, .045f, .82f);

            GameObject map = new("Map", typeof(RectTransform), typeof(RawImage));
            map.transform.SetParent(frame.transform, false);
            RectTransform mapRect = map.GetComponent<RectTransform>();
            mapRect.anchorMin = new Vector2(.035f, .035f);
            mapRect.anchorMax = new Vector2(.965f, .965f);
            mapRect.offsetMin = Vector2.zero;
            mapRect.offsetMax = Vector2.zero;
            RawImage image = map.GetComponent<RawImage>();
            image.texture = mapTexture;
            image.raycastTarget = false;

            GameObject marker = new("PlayerMarker", typeof(RectTransform), typeof(Image));
            marker.transform.SetParent(frame.transform, false);
            RectTransform markerRect = marker.GetComponent<RectTransform>();
            markerRect.anchorMin = markerRect.anchorMax = new Vector2(.5f, .5f);
            markerRect.sizeDelta = new Vector2(15f, 15f);
            marker.GetComponent<Image>().color = new Color(1f, .36f, .015f, 1f);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                    if (participant != null && participant.IsLocalPlayer) { target = participant.transform; break; }
            }
            if (target == null || mapCamera == null) return;
            Vector3 p = target.position;
            mapCamera.transform.position = new Vector3(p.x, p.y + 95f, p.z);
            mapCamera.transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
            if (Time.unscaledTime >= nextRender)
            {
                nextRender = Time.unscaledTime + renderInterval;
                mapCamera.Render();
            }
        }

        private void OnDestroy()
        {
            if (mapTexture == null) return;
            mapTexture.Release();
            Destroy(mapTexture);
        }
    }
}
