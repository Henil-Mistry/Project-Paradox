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
                if (delta.magnitude < 0.12f) yield break;
                controller.Move(Vector3.ClampMagnitude(delta, 0.09f) + Vector3.down * 0.04f);
                Assert.Greater(player.transform.position.y, -0.5f, "Route lost floor support");
                yield return new WaitForFixedUpdate();
            }
            Assert.Fail("Route blocked while walking toward " + target + " from " + player.transform.position);
        }
        private IEnumerator InteractWith(Transform target)
        {
            yield return new WaitForSeconds(0.22f);
            One<FirstPersonController>().GetComponentInChildren<Camera>().transform.LookAt(target.position);
            One<PlayerInteraction>().ProcessInteraction(true);
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

        [UnityTest]
        public IEnumerator AllFiveRoutesCompleteRestartAndProgress()
        {
            SceneManager.LoadScene(scenes[0]); yield return null; yield return new WaitForFixedUpdate();
            for (int level = 0; level < scenes.Length; level++)
            {
                Assert.AreEqual(scenes[level], SceneManager.GetActiveScene().name);
                Assert.IsFalse(One<PuzzleManager>().Completed);
                One<GameFlow>().NextLevel(); yield return null;
                Assert.AreEqual(scenes[level], SceneManager.GetActiveScene().name, "Cannot skip an incomplete level");
                One<GameFlow>().Restart(); yield return null; yield return new WaitForFixedUpdate();
                Assert.AreEqual(scenes[level], SceneManager.GetActiveScene().name);
                var state = One<PuzzleManager>(); var dimensions = One<DimensionManager>();
                var hands = One<PlayerInteraction>(); var cube = One<TransferableObject>();
                var player = One<FirstPersonController>(); player.enabled = false;
                // The movement script is disabled only so batch cursor state cannot reset the test camera.
                Assert.AreEqual(Dimension.White, dimensions.Current); Assert.IsFalse(state.DoorOpened);
                Assert.IsTrue(dimensions.TrySwitch());
                Vector3 cubeApproach = cube.transform.position; cubeApproach.z -= 1.8f; cubeApproach.y = 0;
                yield return Walk(cubeApproach); yield return InteractWith(cube.transform);
                Assert.AreEqual(cube, hands.HeldObject);
                float crossingX = level == 2 ? -2.5f : level == 3 ? 2.5f : 0;
                yield return Walk(new Vector3(crossingX, 0, 2));
                yield return Walk(new Vector3(crossingX, 0, 11.8f));
                yield return Walk(new Vector3(-3, 0, 11.8f));
                yield return Walk(new Vector3(-3, 0, 12.2f));
                yield return InteractWith(cube.transform);
                Assert.IsNull(hands.HeldObject); Assert.AreEqual(Dimension.White, cube.Dimension);
                Assert.AreEqual(Dimension.Black, dimensions.Current);
                Assert.IsTrue(dimensions.TrySwitch());
                yield return InteractWith(cube.transform); Assert.AreEqual(cube, hands.HeldObject);
                if (level == 2)
                {
                    yield return Walk(new Vector3(-3, 0, 11.8f));
                    yield return Walk(new Vector3(4.2f, 0, 11.8f));
                    yield return Walk(new Vector3(4.2f, 0, 16));
                }
                else
                {
                    yield return Walk(new Vector3(-1.4f, 0, 12.2f));
                    yield return Walk(new Vector3(-1.4f, 0, 14.5f));
                    yield return Walk(new Vector3(3, 0, 14.5f));
                }
                var plate = One<PressurePlate>();
                yield return InteractWith(plate.transform); Assert.IsNull(hands.HeldObject);
                yield return new WaitForSeconds(1.2f); Assert.IsTrue(state.DoorOpened);
                yield return Walk(new Vector3(3.8f, 0, 19));
                yield return Walk(new Vector3(0, 0, 19));
                yield return Walk(new Vector3(0, 0, 23));
                yield return new WaitForFixedUpdate(); Assert.IsTrue(state.Completed);
                Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
                if (level == 4)
                {
                    yield return null;
                    var effects = Object.FindObjectsByType<VisualGlitch>(FindObjectsSortMode.None);
                    Assert.Greater(effects.Length, 0);
                    var positions = new Vector3[effects.Length]; var rotations = new Quaternion[effects.Length];
                    for (int i = 0; i < effects.Length; i++)
                    {
                        Assert.IsTrue(effects[i].IsCalm); Assert.IsTrue(effects[i].GetComponent<Renderer>().enabled);
                        positions[i] = effects[i].transform.localPosition; rotations[i] = effects[i].transform.localRotation;
                    }
                    yield return new WaitForSeconds(1.3f);
                    for (int i = 0; i < effects.Length; i++)
                    { Assert.AreEqual(positions[i], effects[i].transform.localPosition); Assert.AreEqual(rotations[i], effects[i].transform.localRotation); }
                    One<GameFlow>().NextLevel(); yield return null; Assert.AreEqual(scenes[level], SceneManager.GetActiveScene().name);
                }
                else
                {
                    Button next = null;
                    foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None)) if (button.name == "Next Level") next = button;
                    Assert.IsNotNull(next); next.onClick.Invoke(); yield return null; yield return new WaitForFixedUpdate();
                }
            }
        }
    }
}
