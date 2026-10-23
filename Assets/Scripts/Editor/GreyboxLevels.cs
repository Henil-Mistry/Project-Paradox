using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Paradox.Editor
{
    // Adds Garden dressing in-place, then copies its proven gameplay rig to four NEW scenes.
    // Never calls LevelBuilder.Build or replaces existing level scenes.
    public static class GreyboxLevels
    {
        private static readonly string[] Names = { "Garden", "Library", "Mirror Lake", "Observatory", "Heart" };
        private static readonly string[] Paths = { "Assets/Scenes/Level01.unity", "Assets/Scenes/Level02_Library.unity", "Assets/Scenes/Level03_MirrorLake.unity", "Assets/Scenes/Level04_Observatory.unity", "Assets/Scenes/Level05_Heart.unity" };
        private static Scene scene;
        private static Transform decor;
        private static DimensionManager dimensions;
        private static PuzzleManager puzzle;
        private static Material pale, dark, accent;
        public static void Create()
        {
            for (int i = 1; i < Paths.Length; i++) if (File.Exists(Paths[i])) throw new Exception("Preserving existing scene: " + Paths[i]);
            // Preserve the undressed source as a temporary Unity scene, not scene text.
            scene = EditorSceneManager.OpenScene(Paths[0]);
            if (Root("Greybox Scenery", false)) throw new Exception("Garden dressing already exists; edit the saved scenes directly.");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GreyboxSource.unity", true);
            for (int level = 0; level < 5; level++)
            {
                scene = EditorSceneManager.OpenScene(level == 0 ? Paths[0] : "Assets/Scenes/GreyboxSource.unity");
                dimensions = Root("Systems").GetComponent<DimensionManager>(); puzzle = Root("Systems").GetComponent<PuzzleManager>();
                pale = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PaleStone.mat");
                dark = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NightStone.mat");
                accent = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VioletGlow.mat");
                decor = new GameObject("Greybox Scenery").transform;
                AddProgression(level);
                switch (level)
                {
                    case 0: Garden(); break;
                    case 1: Library(); break;
                    case 2: Lake(); break;
                    case 3: Observatory(); break;
                    case 4: Heart(); break;
                }
                EditorSceneManager.SaveScene(scene, Paths[level]);
            }
            AssetDatabase.DeleteAsset("Assets/Scenes/GreyboxSource.unity");
            var entries = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (string path in Paths) entries.Add(new EditorBuildSettingsScene(path, true));
            foreach (var old in EditorBuildSettings.scenes) if (Array.IndexOf(Paths, old.path) < 0) entries.Add(old);
            EditorBuildSettings.scenes = entries.ToArray(); AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Paths[0]);
            Debug.Log("PARADOX: Garden preserved and dressed; four distinct greybox scenes saved with progression.");
        }
        private static GameObject Root(string name, bool required = true)
        {
            foreach (GameObject root in scene.GetRootGameObjects()) if (root.name == name) return root;
            if (required) throw new Exception("Required scene object missing: " + name);
            return null;
        }
        private static void Move(string name, Vector3 position) { Root(name).transform.position = position; }
        private static GameObject Shape(string name, PrimitiveType type, Vector3 pos, Vector3 scale, bool solid = true, bool glow = false)
        {
            GameObject item = GameObject.CreatePrimitive(type); item.name = name; item.transform.SetParent(decor, false);
            item.transform.position = pos; item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = glow ? accent : pale;
            if (!solid) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
            if (!glow) item.AddComponent<DimensionTint>().Configure(dimensions, pale, dark);
            return item;
        }
        private static GameObject Box(string name, Vector3 pos, Vector3 size, bool solid = true) => Shape(name, PrimitiveType.Cube, pos, size, solid);
        private static void Glitch(GameObject item, float interval, float duration, float offset, float spin = 0, bool flicker = false)
        { item.AddComponent<VisualGlitch>().Configure(puzzle, interval, duration, offset, spin, flicker); }
        private static void AddProgression(int level)
        {
            GameFlow flow = Root("Systems").GetComponent<GameFlow>();
            flow.ConfigureProgression(level < 4 ? Path.GetFileNameWithoutExtension(Paths[level + 1]) : "", puzzle);
            HUD hud = Root("HUD").GetComponent<HUD>();
            Text title = null; Button restart = null;
            foreach (Text text in hud.GetComponentsInChildren<Text>(true)) if (text.name == "Title") title = text;
            foreach (Button button in hud.GetComponentsInChildren<Button>(true)) if (button.name == "Restart") restart = button;
            if (!title || !restart) throw new Exception("HUD references missing");
            title.text = (level + 1) + " — " + Names[level].ToUpperInvariant();
            if (level < 4)
            {
                GameObject buttonObject = new GameObject("Next Level", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(restart.transform.parent, false);
                RectTransform rect = buttonObject.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, -165); rect.sizeDelta = new Vector2(300, 58);
                buttonObject.GetComponent<Image>().color = new Color(0.17f, 0.29f, 0.38f);
                Button button = buttonObject.GetComponent<Button>(); UnityEventTools.AddPersistentListener(button.onClick, flow.NextLevel);
                GameObject label = new GameObject("Next Label", typeof(RectTransform), typeof(Text)); label.transform.SetParent(rect, false);
                RectTransform labelRect = label.GetComponent<RectTransform>(); labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one; labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
                Text text = label.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 21;
                text.alignment = TextAnchor.MiddleCenter; text.text = "Next: " + Names[level + 1]; text.raycastTarget = false;

            }
        }
        private static void Garden()
        {
            for (int side = -1; side <= 1; side += 2)
                foreach (float z in new[] { -3f, 2f, 12f, 18f })
                {
                    Box("Garden planter", new Vector3(side * 5.8f, 0.25f, z), new Vector3(1.4f, 0.5f, 1.8f));
                    Shape("Flower stem", PrimitiveType.Cylinder, new Vector3(side * 5.8f, 0.8f, z), new Vector3(0.12f, 0.5f, 0.12f), false);
                    GameObject flower = Shape("Geometric flower", PrimitiveType.Sphere, new Vector3(side * 5.8f, 1.35f, z), Vector3.one * 0.55f, false, true);
                    if (side == -1 && z == -3) Glitch(flower, 9, 0.12f, 0, 0, true);
                }
            Box("Garden bench", new Vector3(4.8f, 0.45f, -4.5f), new Vector3(2.4f, 0.9f, 0.6f));
        }
        private static void Library()
        {
            // Shelves divide the start hall into reading aisles; the crossing stays central.
            for (int side = -1; side <= 1; side += 2)
                foreach (float z in new[] { -1f, 3f, 13f, 18f })
                {
                    Box("Shelf end", new Vector3(side * 5.8f, 1.3f, z), new Vector3(0.3f, 2.6f, 2.4f));
                    for (int shelf = 0; shelf < 3; shelf++)
                    {
                        Box("Shelf", new Vector3(side * 5.8f, 0.4f + shelf * 0.8f, z), new Vector3(1.4f, 0.12f, 2.4f));
                        for (int book = 0; book < 4; book++) Box("Book", new Vector3(side * 5.8f, 0.7f + shelf * 0.8f, z - 0.8f + book * 0.5f), new Vector3(0.65f, 0.5f, 0.16f), false);
                    }
                }
            Box("Reading table", new Vector3(2.8f, 0.6f, 1), new Vector3(2.6f, 1.2f, 1.6f));
            for (int i = 0; i < 5; i++)
            {
                GameObject letter = new GameObject("Escaping page symbol"); letter.transform.SetParent(decor, false); letter.transform.position = new Vector3(2 + i * 0.4f, 1.8f + i * 0.25f, 1);
                TextMesh text = letter.AddComponent<TextMesh>(); text.text = new[] { "A", "?", "X", "+", "O" }[i]; text.characterSize = 0.3f; text.fontSize = 48; text.color = new Color(0.5f, 0.6f, 1); letter.transform.rotation = Quaternion.identity;
                Glitch(letter, 3.5f + i * 0.1f, 0.4f, 0.3f);
            }
            Box("Aisle divider", new Vector3(2.5f, 1, 13), new Vector3(4, 2, 0.5f));
        }
        private static void Lake()
        {
            // Offset crossing is a continuous stepped-width path, no jump-with-Cube requirement.
            Move("Black Bridge", new Vector3(-2.5f, -0.2f, 8));
            Move("Cube Pedestal", new Vector3(2.5f, 0.35f, 1)); Move("Black Cube Home", new Vector3(2.5f, 1.15f, 1)); Move("Cube", new Vector3(2.5f, 1.15f, 1));
            Shape("Lake surface", PrimitiveType.Cube, new Vector3(0, -1.1f, 8), new Vector3(14.8f, 0.05f, 5.8f), false, true);
            for (int i = 0; i < 3; i++)
            {
                GameObject platform = Box("Black ice landing", new Vector3(-2.5f, 0.02f, 6 + i * 2), new Vector3(3.4f, 0.12f, 1.3f));
                platform.AddComponent<DimensionObject>().Configure(dimensions, DimensionMembership.Black);
            }
            foreach (float x in new[] { -5.7f, 4.5f })
            {
                Shape("Lake monolith", PrimitiveType.Cube, new Vector3(x, 1.6f, 3.8f), new Vector3(0.6f, 3.2f, 0.6f));
                GameObject reflection = Shape("Broken mirrored monolith", PrimitiveType.Cube, new Vector3(x, -0.65f, 6.2f), new Vector3(0.6f, 0.6f, 0.6f), false, true);
                reflection.AddComponent<DimensionTint>().Configure(dimensions, pale, AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/CubeBlue.mat"));
                Glitch(reflection, 2.3f, 0.65f, 0.45f);
            }
            Box("Lakeside obstacle", new Vector3(2, 0.4f, 14), new Vector3(2, 0.8f, 3));
        }
        private static void Observatory()
        {
            Move("Black Bridge", new Vector3(2.5f, -0.2f, 8));
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4;
                Shape("Chamber pillar", PrimitiveType.Cylinder, new Vector3(Mathf.Cos(angle) * 5.7f, 1.8f, 16 + Mathf.Sin(angle) * 3.5f), new Vector3(0.5f, 1.8f, 0.5f));
            }
            GameObject face = Shape("Clock face", PrimitiveType.Cylinder, new Vector3(0, 3.2f, 19.8f), new Vector3(3.4f, 0.1f, 3.4f), false); face.transform.rotation = Quaternion.Euler(90, 0, 0);
            GameObject hand = Box("Skipping clock hand", new Vector3(0, 3.2f, 19.6f), new Vector3(0.13f, 2.7f, 0.08f), false); hand.GetComponent<DimensionTint>().Configure(dimensions, accent, accent); Glitch(hand, 1.8f, 0.6f, 0, 32);
            Shape("Eclipse halo", PrimitiveType.Sphere, new Vector3(0, 4.8f, 17.2f), new Vector3(1.7f, 1.7f, 0.15f), false, true);
            GameObject eclipse = Shape("Eclipse disc", PrimitiveType.Sphere, new Vector3(0.4f, 4.8f, 17), new Vector3(1.2f, 1.2f, 0.15f), false); Glitch(eclipse, 1.8f, 0.5f, 0.55f);
            Box("Approach barrier", new Vector3(-1.5f, 0.7f, 3), new Vector3(5, 1.4f, 0.5f));
        }
        private static void Heart()
        {
            Box("Core plinth", new Vector3(0, 0.35f, 16.8f), new Vector3(2.6f, 0.7f, 2.6f));
            for (int i = 0; i < 3; i++)
            {
                GameObject core = Shape("Heart core segment", PrimitiveType.Sphere, new Vector3(i == 0 ? -0.5f : i == 1 ? 0.5f : 0, i == 2 ? 1.3f : 2.1f, 16.8f), Vector3.one * (i == 2 ? 1.2f : 1.5f), false, true);
                Glitch(core, 1.1f, 0.7f, 0.22f, i == 2 ? 25 : 0, true);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Entrance fin", new Vector3(side * 5, 1.6f, 0), new Vector3(0.7f, 3.2f, 6));
                Box("Core chamber fin", new Vector3(side * 5.8f, 1.8f, 17), new Vector3(0.6f, 3.6f, 5));
                GameObject shard = Shape("Core echo", PrimitiveType.Cube, new Vector3(side * 2, 2.8f, 17.8f), new Vector3(0.2f, 1.5f, 0.2f), false, true);
                Glitch(shard, 0.9f, 0.6f, 0.35f, 18, true);
            }
        }
    }
}
