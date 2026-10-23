using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Paradox.Editor
{
    public static class InstallConfigurations
    {
        [MenuItem("Paradox/Install Approved Configurations and Gun Once")]
        public static void Apply()
        {
            Vector3[] alternate = { new Vector3(-10,.5f,-14), new Vector3(14,.5f,-12), new Vector3(-18,.5f,-14), new Vector3(3,.5f,-24), new Vector3(4,.5f,-23) };
            int index = 0;
            foreach (var entry in EditorBuildSettings.scenes)
            {
                if (!entry.enabled) continue;
                var scene = EditorSceneManager.OpenScene(entry.path);
                if (UnityEngine.Object.FindFirstObjectByType<PuzzleConfigurationSelector>()) throw new Exception("Already installed: " + entry.path);
                var dimensions = UnityEngine.Object.FindFirstObjectByType<DimensionManager>();
                var state = UnityEngine.Object.FindFirstObjectByType<PuzzleManager>();
                var player = UnityEngine.Object.FindFirstObjectByType<FirstPersonController>();
                var hands = player.GetComponent<PlayerInteraction>();
                var cube = UnityEngine.Object.FindFirstObjectByType<TransferableObject>();
                var plate = UnityEngine.Object.FindFirstObjectByType<PressurePlate>();
                var door = UnityEngine.Object.FindFirstObjectByType<Door>();
                var exit = UnityEngine.Object.FindFirstObjectByType<ExitZone>();
                var hud = UnityEngine.Object.FindFirstObjectByType<HUD>();
                var camera = player.GetComponentInChildren<Camera>();
                var gateMembership = door.gameObject.AddComponent<DimensionObject>(); gateMembership.Configure(dimensions, DimensionMembership.White);
                var exitMembership = exit.gameObject.AddComponent<DimensionObject>(); exitMembership.Configure(dimensions, DimensionMembership.White);
                Transform spawn = GameObject.Find("Player Spawn").transform;
                var choices = new[] {
                    new PuzzleConfiguration { name = "Black gate / White plate", playerSpawn = spawn.position, objectSpawn = cube.transform.position,
                        objectDimension = Dimension.Black, plateDimension = Dimension.White, gateDimension = Dimension.Black },
                    new PuzzleConfiguration { name = "White gate / Black plate", playerSpawn = spawn.position, objectSpawn = alternate[index],
                        objectDimension = Dimension.White, plateDimension = Dimension.Black, gateDimension = Dimension.White }
                };
                dimensions.gameObject.AddComponent<PuzzleConfigurationSelector>().Configure(choices, player, spawn,
                    GameObject.Find("Black Cube Home").transform, GameObject.Find("White Cube Home").transform,
                    cube, plate, gateMembership, exitMembership, state, dimensions);
                GameObject model = new GameObject("Transfer Gun Model"); model.transform.SetParent(camera.transform, false);
                model.transform.localPosition = new Vector3(.48f,-.38f,.7f);
                Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/CubeBlue.mat");
                foreach (bool barrel in new[] { false, true })
                {
                    var part = GameObject.CreatePrimitive(PrimitiveType.Cube); part.name = barrel ? "Emitter" : "Grip";
                    part.transform.SetParent(model.transform, false);
                    part.transform.localPosition = barrel ? new Vector3(0,.08f,.12f) : Vector3.zero;
                    part.transform.localScale = barrel ? new Vector3(.12f,.12f,.35f) : new Vector3(.13f,.25f,.15f);
                    UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>()); part.GetComponent<Renderer>().sharedMaterial = material;
                }
                model.SetActive(false);
                var gun = player.gameObject.AddComponent<TransferGun>(); gun.Configure(camera, cube, hands, state, dimensions, hud, model); hands.ConfigureGun(gun);
                foreach (var pad in UnityEngine.Object.FindObjectsByType<WarpPad>(FindObjectsSortMode.None)) pad.gameObject.SetActive(false);
                foreach (var root in scene.GetRootGameObjects())
                    if (root.name == "Warp Upright" || root.name == "Warp Crown" || root.name == "White Transfer Burst" || root.name == "Black Transfer Burst") root.SetActive(false);
                foreach (var text in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                    if (text.name == "Controls") text.text = "WASD Move • E Pick up / Place / Equip gun • Left-click Transfer • Q Switch • R Restart";
                EditorSceneManager.SaveScene(scene); index++;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
