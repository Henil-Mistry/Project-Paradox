using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Paradox.Tests
{
    // End-to-end scene suite: real controller movement along the authored routes,
    // camera-targeted interactions, completion triggers, restart and next-button events.
    public sealed class FiveLevelTests
    {
        private readonly string[] scenes = { "Level01", "Level02_Library", "Level03_MirrorLake", "Level04_Observatory", "Level05_Heart" };
        private static T One<T>() where T : Object => Object.FindFirstObjectByType<T>();
        private IEnumerator Walk(Vector3 target)
        {
            var player = One<FirstPersonController>();
            var controller = player.GetComponent<CharacterController>();
            for (int frame = 0; frame < 400; frame++)
            {
                Vector3 delta = target - player.transform.position; delta.y = 0;
                if (delta.magnitude < 0.12f || One<PuzzleManager>().Completed) yield break;
                controller.Move(Vector3.ClampMagnitude(delta, 0.09f) + Vector3.down * 0.04f);
                Assert.Greater(player.transform.position.y, -0.5f, "Route lost floor support");
                yield return new WaitForSecondsRealtime(.02f);
            }
            Assert.Fail(SceneManager.GetActiveScene().name + " route blocked while walking toward " + target + " from " + player.transform.position);
        }
        private IEnumerator InteractWith(Transform target)
        {
            yield return new WaitForSeconds(0.22f);
            One<FirstPersonController>().GetComponentInChildren<Camera>().transform.LookAt(target.position);
            One<PlayerInteraction>().ProcessInteraction(true);
        }
        [UnityTest]
        public IEnumerator SpaciousAreasHaveSupportBoundariesAndCubeRecoveryBounds()
        {
            foreach (string level in scenes)
            {
                SceneManager.LoadScene(level); yield return null; yield return new WaitForFixedUpdate();
                var player = One<FirstPersonController>(); player.enabled = false;
                var controller = player.GetComponent<CharacterController>();
                for (int world = 0; world < 2; world++)
                {
                    int supported = 0, boundaries = 0;
                    foreach (BoxCollider shape in Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
                    {
                        if (shape.name == "Exploration ground")
                        {
                            Assert.IsTrue(shape.enabled); Assert.IsFalse(shape.isTrigger);
                            Vector3 point = shape.bounds.center; point.y = 0.1f;
                            Assert.IsTrue(Physics.Raycast(point, Vector3.down, 0.3f, ~0, QueryTriggerInteraction.Ignore)); supported++;
                        }
                        if (shape.name != "Landscape edge" && shape.name != "Library perimeter") continue;
                        boundaries++;
                        if (boundaries % 7 != 0) continue;
                        Vector3 normal = shape.bounds.size.x < shape.bounds.size.z ? Vector3.right : Vector3.forward;
                        Vector3 centre = shape.bounds.center; centre.y = 0.12f;
                        // Choose the supported side of this outer edge, then try walking through it.
                        foreach (int sign in new[] { -1, 1 })
                        {
                            Vector3 start = centre + normal * sign * 0.7f;
                            if (!Physics.Raycast(start, Vector3.down, 0.3f, ~0, QueryTriggerInteraction.Ignore)) continue;
                            controller.enabled = false;
                            // A ground-level probe cannot start inside a terrace stair or other solid.
                            bool obstructed = Physics.CheckCapsule(start + Vector3.up * 0.35f,
                                start + Vector3.up * 1.45f, 0.3f, ~0, QueryTriggerInteraction.Ignore);
                            controller.enabled = true;
                            if (obstructed) continue;
                            controller.enabled = false; player.transform.position = start; controller.enabled = true;
                            controller.Move(-normal * sign * 1.4f);
                            Assert.Greater(Vector3.Dot(player.transform.position - centre, normal * sign), 0,
                                level + " boundary leaked at " + centre);
                            break;
                        }
                    }
                    Assert.Greater(supported, 30); Assert.Greater(boundaries, 60);
                    controller.enabled = false; player.transform.position = new Vector3(0, 0.12f, -2.5f); controller.enabled = true;
                    if (world == 0) { Assert.IsTrue(One<DimensionManager>().TrySwitch()); yield return new WaitForFixedUpdate(); }
                }
                var cube = One<TransferableObject>(); cube.MoveToDimension(Dimension.Black, new Vector3(8, 0.8f, -8));
                yield return new WaitForSeconds(0.3f);
                Assert.Less(Vector3.Distance(new Vector3(cube.transform.position.x, 0, cube.transform.position.z), new Vector3(8, 0, -8)), 0.1f,
                    "Cube incorrectly recovered from valid exploration space");
                // The shared enlarged footprint must not create a walkable route across the gap.
                foreach (float x in new[] { -18f, -10f, 10f, 18f })
                    Assert.IsFalse(Physics.Raycast(new Vector3(x, 1, 7), Vector3.down, 1.05f, ~0, QueryTriggerInteraction.Ignore));
            }
        }

        [UnityTest]
        public IEnumerator RenderAllGreyboxLayouts()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) Assert.Ignore("Graphics device required");
            foreach (string level in scenes)
            {
                SceneManager.LoadScene(level); yield return null; yield return new WaitForFixedUpdate();
                One<FirstPersonController>().enabled = false;
                Assert.IsTrue(One<DimensionManager>().TrySwitch());
                yield return new WaitForSeconds(0.4f);
                Camera camera = One<FirstPersonController>().GetComponentInChildren<Camera>();
                camera.transform.position = new Vector3(6.8f, 6, -3);
                camera.transform.LookAt(new Vector3(0, 0.6f, 12));
                var target = new RenderTexture(1280, 720, 24);
                RenderTexture previous = RenderTexture.active;
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
                string folder = System.IO.Path.Combine(Application.dataPath, "../Docs/GreyboxPreviews");
                System.IO.Directory.CreateDirectory(folder);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder, level + ".png"), image.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = previous;
                Object.Destroy(image); Object.Destroy(target);
            }
        }

        [UnityTest] public IEnumerator ApprovedGardenConfigurations() { yield return CheckConfigurations(0); }
        [UnityTest] public IEnumerator ApprovedLibraryConfigurations() { yield return CheckConfigurations(1); }
        [UnityTest] public IEnumerator ApprovedLakeConfigurations() { yield return CheckConfigurations(2); }
        [UnityTest] public IEnumerator ApprovedObservatoryConfigurations() { yield return CheckConfigurations(3); }
        [UnityTest] public IEnumerator ApprovedHeartConfigurations() { yield return CheckConfigurations(4); }
        private IEnumerator CheckConfigurations(int level)
        {
            for (int configuration = 0; configuration < 2; configuration++)
            {
                PuzzleConfigurationSelector.DebugSeed = configuration - SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/"+scenes[level]+".unity");
                SceneManager.LoadScene(scenes[level]); yield return null; yield return new WaitForFixedUpdate();
                var selected = One<PuzzleConfigurationSelector>();
                Assert.AreEqual(configuration, selected.SelectedIndex);
                var initial = selected.ActiveConfiguration;
                One<GameFlow>().NextLevel(); yield return null;
                Assert.AreEqual(scenes[level], SceneManager.GetActiveScene().name);
                One<GameFlow>().Restart(); yield return null; yield return new WaitForFixedUpdate();
                selected = One<PuzzleConfigurationSelector>(); Assert.AreEqual(configuration, selected.SelectedIndex);
                var state = One<PuzzleManager>(); var dimensions = One<DimensionManager>();
                var hands = One<PlayerInteraction>(); var cube = One<TransferableObject>(); var gun = One<TransferGun>();
                var player = One<FirstPersonController>(); player.enabled = false;
                var camera = player.GetComponentInChildren<Camera>();
                Assert.AreEqual(initial.objectDimension, cube.Dimension);
                Assert.AreEqual(initial.plateDimension, One<PressurePlate>().Dimension);
                Assert.AreEqual((DimensionMembership)initial.gateDimension, One<Door>().GetComponent<DimensionObject>().Membership);
                Assert.Greater(Vector3.Distance(player.transform.position, cube.transform.position), 12);
                Assert.IsFalse(state.Completed); Assert.IsFalse(state.DoorOpened);
                // E equips from empty space, without replacing the existing cube pickup convention.
                camera.transform.LookAt(camera.transform.position + Vector3.up);
                hands.ProcessInteraction(true); Assert.IsTrue(gun.Equipped);
                if (dimensions.Current != cube.Dimension) { Assert.IsTrue(dimensions.TrySwitch()); yield return new WaitForFixedUpdate(); }
                Vector3 home = initial.objectSpawn;
                cube.MoveToDimension(cube.Dimension, new Vector3(0,-9,0)); yield return null;
                Assert.Less(Vector3.Distance(cube.transform.position, home), .12f);
                Vector3[][] retrievalRoutes = {
                    new[] { new Vector3(0,0,-5), new Vector3(-8,0,-5), new Vector3(-8,0,home.z+1.8f), new Vector3(home.x,0,home.z+1.8f) },
                    new[] { new Vector3(0,0,-4), new Vector3(12,0,-4), new Vector3(14,0,-4), new Vector3(home.x,0,home.z+1.8f) },
                    new[] { new Vector3(-6,0,-4), new Vector3(-6,0,-12.2f), new Vector3(home.x,0,home.z+1.8f) },
                    new[] { new Vector3(3,0,-4), new Vector3(3,0,home.z+1.8f), new Vector3(home.x,0,home.z+1.8f) },
                    new[] { new Vector3(home.x,0,-4), new Vector3(home.x,0,home.z+1.8f) }
                };
                foreach (var waypoint in retrievalRoutes[level]) yield return Walk(waypoint);
                if (cube.Dimension == Dimension.White)
                {
                    camera.transform.LookAt(cube.transform.position); Vector3 before = cube.transform.position;
                    Assert.IsTrue(gun.TryFire()); Assert.IsFalse(gun.TryFire());
                    Assert.Less(Vector3.Distance(before, cube.transform.position), .01f);
                    Assert.AreEqual(Dimension.White, dimensions.Current);
                    Assert.IsTrue(dimensions.TrySwitch()); yield return new WaitForFixedUpdate();
                }
                yield return InteractWith(cube.transform); Assert.AreEqual(cube, hands.HeldObject);
                for (int i = retrievalRoutes[level].Length - 2; i >= 0; i--) yield return Walk(retrievalRoutes[level][i]);
                yield return Walk(new Vector3(0,0,-2.5f));
                float crossingX = level == 2 ? -2.5f : level == 3 ? 2.5f : 0;
                yield return Walk(new Vector3(crossingX,0,2)); yield return Walk(new Vector3(crossingX,0,11.8f));
                yield return Walk(new Vector3(-3,0,11.8f)); yield return Walk(new Vector3(-3,0,12.2f));
                if (initial.plateDimension == Dimension.White)
                {
                    hands.PlaceHeld(new Vector3(-3,.5f,14)); yield return new WaitForFixedUpdate();
                    camera.transform.LookAt(cube.transform.position); Vector3 before = cube.transform.position;
                    Assert.IsTrue(gun.TryFire()); Assert.IsFalse(gun.TryFire());
                    Assert.AreEqual(Dimension.Black, dimensions.Current);
                    Assert.Less(Vector3.Distance(before,cube.transform.position),.01f);
                    Assert.IsTrue(dimensions.TrySwitch()); yield return new WaitForFixedUpdate();
                    yield return InteractWith(cube.transform); Assert.AreEqual(cube,hands.HeldObject);
                }
                if (level == 2)
                { yield return Walk(new Vector3(-3,0,11.8f)); yield return Walk(new Vector3(4.2f,0,11.8f)); yield return Walk(new Vector3(4.2f,0,16)); }
                else
                { yield return Walk(new Vector3(-1.4f,0,12)); yield return Walk(new Vector3(-1.4f,0,14.5f)); yield return Walk(new Vector3(3,0,14.5f)); }
                var plate = One<PressurePlate>();
                yield return InteractWith(plate.transform); Assert.IsNull(hands.HeldObject);
                yield return new WaitForSeconds(1.1f); Assert.IsTrue(state.DoorOpened);
                // Exercise non-latching gate behavior in every approved configuration.
                Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(new Vector3(0,.5f,15));
                yield return new WaitForSeconds(1.1f); Assert.IsFalse(state.DoorOpened); Assert.IsFalse(One<Door>().IsOpen);
                Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(plate.Placement);
                yield return new WaitForSeconds(1.1f); Assert.IsTrue(state.DoorOpened);
                Assert.IsFalse(state.Complete(), "Wrong-dimension exit cannot complete the level");
                yield return Walk(new Vector3(3.8f,0,19)); yield return Walk(new Vector3(0,0,19));
                Assert.IsTrue(dimensions.TrySwitch()); yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
                Assert.AreEqual(initial.gateDimension,dimensions.Current); Assert.IsTrue(plate.IsSatisfied); Assert.IsTrue(state.DoorOpened);
                Assert.AreEqual(configuration,selected.SelectedIndex, "Switching must not reroll the configuration");
                yield return Walk(new Vector3(0,0,23)); yield return null;
                Assert.IsTrue(state.Completed); Assert.AreEqual(CursorLockMode.None,Cursor.lockState);
                if (level == 4)
                {
                    foreach (var effect in Object.FindObjectsByType<VisualGlitch>(FindObjectsSortMode.None)) Assert.IsTrue(effect.IsCalm);
                    One<GameFlow>().NextLevel(); yield return null; Assert.AreEqual(scenes[level],SceneManager.GetActiveScene().name);
                }
                else
                {
                    One<GameFlow>().NextLevel(); yield return null; yield return new WaitForFixedUpdate();
                    Assert.AreEqual(scenes[level+1],SceneManager.GetActiveScene().name); Assert.IsFalse(One<PuzzleManager>().Completed);
                    Assert.Less(One<PuzzleConfigurationSelector>().SelectedIndex,2);
                }
            }
            PuzzleConfigurationSelector.DebugSeed = null;
        }
        [TearDown] public void ClearSeed() { PuzzleConfigurationSelector.DebugSeed = null; }
    }
}
