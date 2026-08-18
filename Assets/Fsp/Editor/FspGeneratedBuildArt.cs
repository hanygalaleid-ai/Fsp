#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Fsp.EditorTools
{
    public static class FspGeneratedBuildArt
    {
        public const string LobbyPath = "Assets/Fsp/Art/Resources/Lobby/lobby_reference.jpg";
        public const string JoystickPath = "Assets/Fsp/Art/Resources/UI/joystick_base.png";
        public const string SecondaryButtonPath = "Assets/Fsp/Art/Resources/UI/ui_button_secondary.png";
        public const string ActionIconsPath = "Assets/Fsp/Art/Resources/UI/action_icons.png";
        public const string AppIconPath = "Assets/Fsp/Art/AppIcon/app_icon.jpg";

        public static void EnsureAll()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LobbyPath));
            Directory.CreateDirectory(Path.GetDirectoryName(JoystickPath));
            Directory.CreateDirectory(Path.GetDirectoryName(AppIconPath));

            WriteLobby();
            WriteJoystick();
            WriteSecondaryButton();
            WriteActionIcons();
            WriteAppIcon();

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        private static void WriteLobby()
        {
            const int w = 1280, h = 720;
            Texture2D t = NewTexture(w, h);
            Color32[] p = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = y / (float)(h - 1);
                Color sky = Color.Lerp(new Color(0.06f,0.11f,0.18f), new Color(0.92f,0.55f,0.25f), Mathf.Clamp01(v / 0.68f));
                Color ground = Color.Lerp(new Color(0.40f,0.27f,0.18f), new Color(0.07f,0.06f,0.055f), Mathf.Clamp01((v - 0.62f) / 0.38f));
                Color c = v < 0.62f ? sky : ground;
                for (int x = 0; x < w; x++) p[y * w + x] = c;
            }
            t.SetPixels32(p);
            // Sun and simple fortress silhouettes: deterministic, lightweight and always valid.
            DrawDisc(t, 985, 250, 55, new Color32(255,210,120,255));
            DrawRect(t, 900, 390, 220, 120, new Color32(28,27,27,255));
            DrawRect(t, 930, 340, 48, 170, new Color32(22,22,23,255));
            DrawRect(t, 1045, 325, 48, 185, new Color32(22,22,23,255));
            DrawRect(t, 750, 385, 34, 125, new Color32(34,31,28,255));
            t.Apply(false, false);
            File.WriteAllBytes(LobbyPath, t.EncodeToJPG(82));
            Object.DestroyImmediate(t);
        }

        private static void WriteJoystick()
        {
            const int s = 256;
            Texture2D t = NewTexture(s, s);
            Fill(t, new Color32(0,0,0,0));
            DrawDisc(t, 128, 128, 110, new Color32(7,15,24,150));
            DrawRing(t, 128, 128, 105, 8, new Color32(255,145,28,220));
            DrawDisc(t, 128, 128, 45, new Color32(190,200,208,180));
            t.Apply(false, false);
            File.WriteAllBytes(JoystickPath, t.EncodeToPNG());
            Object.DestroyImmediate(t);
        }

        private static void WriteSecondaryButton()
        {
            const int w = 512, h = 160;
            Texture2D t = NewTexture(w, h);
            Fill(t, new Color32(5,15,25,235));
            DrawRect(t, 0, 0, w, 5, new Color32(185,112,48,255));
            DrawRect(t, 0, h-5, w, 5, new Color32(185,112,48,255));
            DrawRect(t, 0, 0, 5, h, new Color32(185,112,48,255));
            DrawRect(t, w-5, 0, 5, h, new Color32(185,112,48,255));
            t.Apply(false, false);
            File.WriteAllBytes(SecondaryButtonPath, t.EncodeToPNG());
            Object.DestroyImmediate(t);
        }

        private static void WriteActionIcons()
        {
            const int w = 1024, h = 256;
            Texture2D t = NewTexture(w, h);
            Fill(t, new Color32(0,0,0,0));
            for (int i = 0; i < 4; i++)
            {
                int cx = 128 + i * 256;
                DrawDisc(t, cx, 128, 92, new Color32(6,15,24,210));
                DrawRing(t, cx, 128, 91, 7, new Color32(255,145,28,255));
            }
            t.Apply(false, false);
            File.WriteAllBytes(ActionIconsPath, t.EncodeToPNG());
            Object.DestroyImmediate(t);
        }

        private static void WriteAppIcon()
        {
            const int s = 512;
            Texture2D t = NewTexture(s, s);
            Fill(t, new Color32(18,18,19,255));
            DrawDisc(t, 256, 245, 205, new Color32(41,34,29,255));
            DrawRing(t, 256, 245, 208, 22, new Color32(255,153,25,255));
            DrawDisc(t, 256, 250, 82, new Color32(18,20,22,255));
            DrawRect(t, 220, 220, 72, 150, new Color32(20,22,23,255));
            DrawRect(t, 202, 245, 24, 105, new Color32(20,22,23,255));
            DrawRect(t, 286, 245, 24, 105, new Color32(20,22,23,255));
            t.Apply(false, false);
            File.WriteAllBytes(AppIconPath, t.EncodeToJPG(90));
            Object.DestroyImmediate(t);
        }

        private static Texture2D NewTexture(int w, int h)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false, false);
        }

        private static void Fill(Texture2D t, Color32 c)
        {
            Color32[] p = new Color32[t.width * t.height];
            for (int i = 0; i < p.Length; i++) p[i] = c;
            t.SetPixels32(p);
        }

        private static void DrawRect(Texture2D t, int x, int y, int w, int h, Color32 c)
        {
            int x0 = Mathf.Clamp(x, 0, t.width), x1 = Mathf.Clamp(x + w, 0, t.width);
            int y0 = Mathf.Clamp(y, 0, t.height), y1 = Mathf.Clamp(y + h, 0, t.height);
            for (int yy = y0; yy < y1; yy++)
                for (int xx = x0; xx < x1; xx++) t.SetPixel(xx, yy, c);
        }

        private static void DrawDisc(Texture2D t, int cx, int cy, int r, Color32 c)
        {
            int rr = r * r;
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                    if (x*x + y*y <= rr && cx+x >= 0 && cx+x < t.width && cy+y >= 0 && cy+y < t.height)
                        t.SetPixel(cx+x, cy+y, c);
        }

        private static void DrawRing(Texture2D t, int cx, int cy, int r, int thickness, Color32 c)
        {
            int outer = r*r, inner = (r-thickness)*(r-thickness);
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                {
                    int d = x*x + y*y;
                    if (d <= outer && d >= inner && cx+x >= 0 && cx+x < t.width && cy+y >= 0 && cy+y < t.height)
                        t.SetPixel(cx+x, cy+y, c);
                }
        }
    }
}
#endif
