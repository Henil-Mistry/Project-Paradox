using System.Collections.Generic;
using UnityEngine;

namespace Paradox
{
    /// <summary>One display preference path, shared by startup and the pause-menu controls.</summary>
    public static class DisplayPreferences
    {
        private const string WidthKey = "Paradox.Display.Width";
        private const string HeightKey = "Paradox.Display.Height";
        private const string FullscreenKey = "Paradox.Display.Fullscreen";
        public static Vector2Int SelectedSize { get; private set; }
        public static bool Fullscreen { get; private set; }
        public static Vector2Int DesktopSize { get; private set; }
        private static bool initialized;
        private static readonly List<Vector2Int> sizes = new List<Vector2Int>();
        public static IReadOnlyList<Vector2Int> Sizes { get { EnsureInitialized(); return sizes; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { initialized = false; sizes.Clear(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Restore()
        {
            EnsureInitialized();
            // Leave the configured startup mode intact until the player has made a choice.
            if (PlayerPrefs.HasKey(FullscreenKey))
            {
                Fullscreen = PlayerPrefs.GetInt(FullscreenKey) == 1;
                var saved = new Vector2Int(PlayerPrefs.GetInt(WidthKey), PlayerPrefs.GetInt(HeightKey));
                SelectedSize = sizes.Contains(saved) ? saved : DesktopSize;
                ApplyToScreen();
            }
        }

        public static void EnsureInitialized()
        {
            if (initialized) return;
            initialized = true;
            DesktopSize = new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);
            if (DesktopSize.x < 1 || DesktopSize.y < 1)
                DesktopSize = new Vector2Int(Mathf.Max(800, Screen.width), Mathf.Max(600, Screen.height));
            foreach (var resolution in Screen.resolutions) AddSize(new Vector2Int(resolution.width, resolution.height));
            // Borderless fullscreen supports scaled rendering; these also provide useful window sizes.
            foreach (var size in new[] { new Vector2Int(800, 600), new Vector2Int(1024, 768),
                new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080) }) AddSize(size);
            AddSize(DesktopSize);
            SelectedSize = new Vector2Int(Screen.width, Screen.height);
            AddSize(SelectedSize);
            sizes.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            Fullscreen = Screen.fullScreen;
        }

        private static void AddSize(Vector2Int size)
        {
            if (size.x > 0 && size.y > 0 && size.x <= DesktopSize.x && size.y <= DesktopSize.y && !sizes.Contains(size))
                sizes.Add(size);
        }

        public static void Apply(Vector2Int size, bool fullscreen)
        {
            EnsureInitialized();
            if (!sizes.Contains(size)) return;
            SelectedSize = size;
            Fullscreen = fullscreen;
            ApplyToScreen();
            // Editor previews must not overwrite the user's standalone preferences.
            if (!Application.isEditor)
            {
                PlayerPrefs.SetInt(WidthKey, size.x);
                PlayerPrefs.SetInt(HeightKey, size.y);
                PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private static void ApplyToScreen()
        {
            // Borderless fullscreen is supported by both Linux and Windows; exclusive mode is not.
            if (!Application.isEditor)
                Screen.SetResolution(SelectedSize.x, SelectedSize.y,
                    Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
    }
}
