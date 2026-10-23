using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Paradox.Editor
{
    // Editor-only primitive scene generator. No scene YAML or settings are hand-authored.
    public static class LevelBuilder
    {
        private const string ScenePath = "Assets/Scenes/Level01.unity";
        [MenuItem("Paradox/Build Level 01")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Rebuild Level01?", "This replaces the generated Level01 scene. Save your custom scene under a different name first.", "Rebuild", "Cancel")) return;
            Build();
        }
        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes"); Directory.CreateDirectory("Assets/Materials");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Material pale = Material("PaleStone", new Color(0.74f, 0.77f, 0.8f));
            Material dark = Material("NightStone", new Color(0.065f, 0.075f, 0.15f));
            Material warm = Material("WarmTrim", new Color(0.66f, 0.58f, 0.4f));
            Material violet = Material("VioletGlow", new Color(0.42f, 0.23f, 0.8f), new Color(0.25f, 0.1f, 0.65f));
            Material blue = Material("CubeBlue", new Color(0.23f, 0.65f, 0.9f), new Color(0.05f, 0.25f, 0.4f));
            Material mint = Material("PlateMint", new Color(0.3f, 0.8f, 0.7f), new Color(0.1f, 0.5f, 0.3f));
            Material doorMaterial = Material("DoorSlate", new Color(0.15f, 0.22f, 0.3f));
            GameObject systems = new GameObject("Systems");
            DimensionManager dimensions = systems.AddComponent<DimensionManager>();
            PuzzleManager puzzle = systems.AddComponent<PuzzleManager>();
            GameFlow flow = systems.AddComponent<GameFlow>();
            Light sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(48, -25, 0); sun.shadows = LightShadows.Soft;
            Transform spawn = Marker("Player Spawn", new Vector3(0, 0.12f, -2.5f));
            GameObject player = new GameObject("Player"); player.transform.position = spawn.position;
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f; controller.radius = 0.32f; controller.center = new Vector3(0, 0.9f, 0); controller.stepOffset = 0.25f; controller.skinWidth = 0.03f;
            CapsuleCollider probe = player.AddComponent<CapsuleCollider>();
            probe.height = 1.74f; probe.radius = 0.30f; probe.center = controller.center; probe.enabled = false;
            FirstPersonController movement = player.AddComponent<FirstPersonController>();
            PlayerInteraction hands = player.AddComponent<PlayerInteraction>();
            Camera camera = new GameObject("Player Camera").AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.transform.SetParent(player.transform, false); camera.transform.localPosition = new Vector3(0, 1.6f, 0);
            camera.nearClipPlane = 0.08f; camera.farClipPlane = 90; camera.fieldOfView = 70; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<AudioListener>();
            Transform grip = Marker("Hold Point", Vector3.zero); grip.SetParent(camera.transform, false); grip.localPosition = new Vector3(0.35f, -0.35f, 1.15f);
            HUD hud = BuildHUD(flow);
            dimensions.Configure(sun, camera, hud, hands, probe, puzzle);
            movement.Configure(camera.transform, spawn, hands, puzzle);

            // Six metres of empty floor cannot be crossed with the default jump.
            Shared("Start Floor", new Vector3(0, -0.5f, 0), new Vector3(14, 1, 10), dimensions, pale, dark);
            Shared("Far Floor", new Vector3(0, -0.5f, 18), new Vector3(14, 1, 14), dimensions, pale, dark);
            Shared("West Wall", new Vector3(-7.25f, 2, 10), new Vector3(0.5f, 4, 30), dimensions, pale, dark);
            Shared("East Wall", new Vector3(7.25f, 2, 10), new Vector3(0.5f, 4, 30), dimensions, pale, dark);
            Shared("Start Wall", new Vector3(0, 2, -5.25f), new Vector3(15, 4, 0.5f), dimensions, pale, dark);
            Shared("End Wall", new Vector3(0, 2, 25.25f), new Vector3(15, 4, 0.5f), dimensions, pale, dark);
            GameObject bridge = Box("Black Bridge", new Vector3(0, -0.2f, 8), new Vector3(3, 0.4f, 6), violet);
            bridge.AddComponent<DimensionObject>().Configure(dimensions, DimensionMembership.Black);
            Shared("Cube Pedestal", new Vector3(-3, 0.35f, 1), new Vector3(1.5f, 0.7f, 1.5f), dimensions, pale, dark);
            Transform blackHome = Marker("Black Cube Home", new Vector3(-3, 1.15f, 1));

            // Pad meshes differ slightly, but their arrival positions match in both worlds.
            Vector3 padPosition = new Vector3(-3, 0.12f, 14);
            GameObject whitePadObject = Primitive("White Warp Pad", PrimitiveType.Cylinder, padPosition, new Vector3(2.1f, 0.12f, 2.1f), warm);
            GameObject blackPadObject = Primitive("Black Warp Pad", PrimitiveType.Cylinder, padPosition, new Vector3(2.1f, 0.12f, 2.1f), violet);
            WarpPad whitePad = whitePadObject.AddComponent<WarpPad>(); WarpPad blackPad = blackPadObject.AddComponent<WarpPad>();
            Transform whiteArrival = Marker("White Cube Home", padPosition + Vector3.up * 0.62f);
            Transform blackArrival = Marker("Black Pad Arrival", whiteArrival.position);
            whitePad.Configure(dimensions, Dimension.White, blackPad, whiteArrival, Burst("White Transfer Burst", whiteArrival.position, new Color(0.4f, 1, 0.9f)));
            blackPad.Configure(dimensions, Dimension.Black, whitePad, blackArrival, Burst("Black Transfer Burst", blackArrival.position, new Color(0.65f, 0.3f, 1)));
            whitePadObject.AddComponent<DimensionObject>().Configure(dimensions, DimensionMembership.White);
            blackPadObject.AddComponent<DimensionObject>().Configure(dimensions, DimensionMembership.Black);
            // Shared arches identify the device without adding another mechanic.
            for (int side = -1; side <= 1; side += 2)
                Box("Warp Upright", padPosition + new Vector3(side * 1.3f, 1.1f, 0.65f), new Vector3(0.16f, 2.3f, 0.16f), blue);
            Box("Warp Crown", padPosition + new Vector3(0, 2.25f, 0.65f), new Vector3(2.75f, 0.16f, 0.16f), blue);

            GameObject cubeObject = Box("Cube", blackHome.position, Vector3.one * 0.8f, blue);
            Rigidbody body = cubeObject.AddComponent<Rigidbody>(); body.mass = 2; body.constraints = RigidbodyConstraints.FreezeRotation; body.interpolation = RigidbodyInterpolation.Interpolate; body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            DimensionObject cubeVisibility = cubeObject.AddComponent<DimensionObject>(); cubeVisibility.Configure(dimensions, DimensionMembership.Black);
            TransferableObject cube = cubeObject.AddComponent<TransferableObject>();
            cube.Configure(dimensions, cubeVisibility, blackHome, whiteArrival, Burst("Cube Recovery Burst", blackHome.position, Color.cyan));

            GameObject plateObject = new GameObject("White Pressure Plate"); plateObject.transform.position = new Vector3(3, 0, 17);
            BoxCollider plateZone = plateObject.AddComponent<BoxCollider>(); plateZone.isTrigger = true; plateZone.center = new Vector3(0, 0.45f, 0); plateZone.size = new Vector3(1.7f, 0.7f, 1.7f);
            GameObject plateBase = Box("Plate Base", new Vector3(3, 0.1f, 17), new Vector3(2, 0.2f, 2), warm); plateBase.transform.SetParent(plateObject.transform, true);
            GameObject plateInset = Box("Plate Inset", new Vector3(3, 0.22f, 17), new Vector3(1.3f, 0.04f, 1.3f), blue); plateInset.transform.SetParent(plateObject.transform, true);
            Transform placement = Marker("Plate Cube Position", new Vector3(3, 0.66f, 17)); placement.SetParent(plateObject.transform, true);
            PressurePlate plate = plateObject.AddComponent<PressurePlate>();
            plate.Configure(dimensions, puzzle, placement, plateInset.GetComponent<Renderer>(), mint);
            plateObject.AddComponent<DimensionObject>().Configure(dimensions, DimensionMembership.White);

            Shared("Exit Partition Left", new Vector3(-4.25f, 2, 21), new Vector3(5.5f, 4, 0.6f), dimensions, pale, dark);
            Shared("Exit Partition Right", new Vector3(4.25f, 2, 21), new Vector3(5.5f, 4, 0.6f), dimensions, pale, dark);
            Shared("Exit Lintel", new Vector3(0, 3.6f, 21), new Vector3(3, 0.8f, 0.6f), dimensions, pale, dark);
            GameObject doorObject = Box("Exit Door", new Vector3(0, 1.6f, 21), new Vector3(3, 3.2f, 0.4f), doorMaterial);
            Door door = doorObject.AddComponent<Door>();
            // A small square sitting on a bar is the essential cube-on-plate clue.
            GameObject glyphCube = Box("Door Cube Glyph", new Vector3(0, 1.9f, 20.76f), new Vector3(0.55f, 0.55f, 0.06f), blue);
            glyphCube.transform.SetParent(doorObject.transform, true); Object.DestroyImmediate(glyphCube.GetComponent<Collider>());
            GameObject glyphPlate = Box("Door Plate Glyph", new Vector3(0, 1.5f, 20.76f), new Vector3(1, 0.12f, 0.06f), warm);
            glyphPlate.transform.SetParent(doorObject.transform, true); Object.DestroyImmediate(glyphPlate.GetComponent<Collider>());
            GameObject exit = new GameObject("Exit Zone"); exit.transform.position = new Vector3(0, 1.5f, 23.5f);
            BoxCollider exitTrigger = exit.AddComponent<BoxCollider>(); exitTrigger.isTrigger = true; exitTrigger.size = new Vector3(13, 3, 2);
            exit.AddComponent<ExitZone>().Configure(puzzle);
            GameObject kill = new GameObject("Chasm Recovery"); kill.transform.position = new Vector3(0, -4, 8);
            BoxCollider killTrigger = kill.AddComponent<BoxCollider>(); killTrigger.isTrigger = true; killTrigger.size = new Vector3(18, 1, 12); kill.AddComponent<RespawnZone>();
            puzzle.Configure(door, plate, hud);
            hands.Configure(camera, grip, hud, puzzle, new[] { whitePad, blackPad }, plate);
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.45f, 0.48f, 0.53f);
            camera.backgroundColor = new Color(0.82f, 0.87f, 0.91f);
            EditorSceneManager.SaveScene(scene, ScenePath);
            // Preserve other build entries, while ensuring reload and builds can load this scene.
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int existing = scenes.FindIndex(entry => entry.path == ScenePath);
            if (existing >= 0) scenes[existing] = new EditorBuildSettingsScene(ScenePath, true);
            else scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("PARADOX: Level01 generated and saved with all references assigned.");
        }
        private static Material Material(string name, Color color, Color? emission = null)
        {
            string path = "Assets/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material) return material;
            material = new Material(Shader.Find("Standard")); material.name = name; material.color = color;
            material.SetFloat("_Glossiness", 0.2f);
            if (emission.HasValue) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", emission.Value); }
            AssetDatabase.CreateAsset(material, path); return material;
        }
        private static GameObject Primitive(string name, PrimitiveType kind, Vector3 position, Vector3 scale, Material material)
        {
            GameObject item = GameObject.CreatePrimitive(kind); item.name = name; item.transform.position = position; item.transform.localScale = scale; item.GetComponent<Renderer>().sharedMaterial = material; return item;
        }
        private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material) => Primitive(name, PrimitiveType.Cube, position, scale, material);
        private static void Shared(string name, Vector3 position, Vector3 scale, DimensionManager manager, Material white, Material black)
        { Box(name, position, scale, white).AddComponent<DimensionTint>().Configure(manager, white, black); }
        private static Transform Marker(string name, Vector3 position) { var marker = new GameObject(name); marker.transform.position = position; return marker.transform; }
        private static ParticleSystem Burst(string name, Vector3 position, Color color)
        {
            ParticleSystem effect = new GameObject(name).AddComponent<ParticleSystem>(); effect.transform.position = position;
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = effect.main; main.playOnAwake = false; main.loop = false; main.duration = 0.6f; main.startLifetime = 0.6f; main.startSpeed = 2; main.startSize = 0.14f; main.startColor = color; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = effect.emission; emission.rateOverTime = 0; emission.SetBursts(new[] { new ParticleSystem.Burst(0, 35) });
            var shape = effect.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.3f;
            effect.GetComponent<ParticleSystemRenderer>().sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
            return effect;
        }
        private static HUD BuildHUD(GameFlow flow)
        {
            GameObject canvasObject = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
            HUD hud = canvasObject.AddComponent<HUD>();
            new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
            GameObject play = Panel("Play", canvasObject.transform, Color.clear, false);
            HUDStrip("Header Backing", play.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -104), Vector2.zero);
            HUDStrip("Footer Backing", play.transform, Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, 74));
            Image promptBackground = HUDStrip("Prompt Backing", play.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260, -82), new Vector2(260, -38));
            Text title = Label("Title", play.transform, "PROJECT PARADOX", 25, new Vector2(28, -24), new Vector2(460, 40), new Vector2(0, 1), TextAnchor.MiddleLeft);
            Label("Tagline", play.transform, "It's all in your perspective.", 16, new Vector2(30, -63), new Vector2(460, 32), new Vector2(0, 1), TextAnchor.MiddleLeft);
            Text dimension = Label("Dimension", play.transform, "WHITE DIMENSION", 18, new Vector2(-28, -28), new Vector2(300, 36), new Vector2(1, 1), TextAnchor.MiddleRight);
            Label("Crosshair", play.transform, "+", 22, Vector2.zero, new Vector2(30, 30), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            Text prompt = Label("Prompt", play.transform, "", 20, new Vector2(0, -60), new Vector2(600, 40), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            Text feedback = Label("Message", play.transform, "", 20, new Vector2(0, 110), new Vector2(900, 40), new Vector2(0.5f, 0), TextAnchor.MiddleCenter);
            Label("Controls", play.transform, "WASD  Move    Mouse  Look    Space  Jump    E  Interact    Q  Switch    R  Restart", 17, new Vector2(0, 35), new Vector2(1200, 35), new Vector2(0.5f, 0), TextAnchor.MiddleCenter);
            GameObject overlay = Panel("Switch Flash", canvasObject.transform, Color.white, false); CanvasGroup fade = overlay.AddComponent<CanvasGroup>(); fade.alpha = 0; fade.blocksRaycasts = false; fade.interactable = false;
            GameObject completion = Panel("Completion", canvasObject.transform, new Color(0.035f, 0.045f, 0.1f, 0.97f), true);
            Label("Complete Title", completion.transform, "Puzzle complete", 44, new Vector2(0, 65), new Vector2(900, 80), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            Label("Complete Tagline", completion.transform, "It's all in your perspective.", 21, new Vector2(0, 0), new Vector2(900, 45), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            GameObject buttonObject = new GameObject("Restart", typeof(RectTransform), typeof(Image), typeof(Button)); buttonObject.transform.SetParent(completion.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f); rect.anchoredPosition = new Vector2(0, -90); rect.sizeDelta = new Vector2(240, 58);
            buttonObject.GetComponent<Image>().color = new Color(0.17f, 0.29f, 0.38f);
            Button button = buttonObject.GetComponent<Button>(); UnityEventTools.AddPersistentListener(button.onClick, flow.Restart);
            Label("Restart Label", buttonObject.transform, "Restart  [R]", 23, Vector2.zero, new Vector2(240, 58), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            completion.SetActive(false);
            hud.Configure(dimension, prompt, feedback, overlay.GetComponent<Image>(), fade, completion, play, promptBackground);
            return hud;
        }
        private static Image HUDStrip(string name, Transform parent, Vector2 minAnchor, Vector2 maxAnchor, Vector2 minOffset, Vector2 maxOffset)
        {
            GameObject strip = Panel(name, parent, new Color(0.025f, 0.035f, 0.065f, 0.72f), false);
            RectTransform rect = strip.GetComponent<RectTransform>(); rect.anchorMin = minAnchor; rect.anchorMax = maxAnchor; rect.offsetMin = minOffset; rect.offsetMax = maxOffset;
            return strip.GetComponent<Image>();
        }
        private static GameObject Panel(string name, Transform parent, Color color, bool raycast)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image)); panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image image = panel.GetComponent<Image>(); image.color = color; image.raycastTarget = raycast; return panel;
        }
        private static Text Label(string name, Transform parent, string value, int size, Vector2 position, Vector2 dimensions, Vector2 anchor, TextAnchor alignment)
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow)); item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            Text text = item.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = alignment; text.color = new Color(0.94f, 0.97f, 1); text.raycastTarget = false;
            item.GetComponent<Shadow>().effectColor = new Color(0.02f, 0.03f, 0.07f, 0.95f); item.GetComponent<Shadow>().effectDistance = new Vector2(1, -1);
            return text;
        }
    }
}
