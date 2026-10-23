using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Paradox.Tests
{
    // Integration tests load the actual generated scene and exercise real physics/state.
    public sealed class LevelTests
    {
        private DimensionManager dimensions;
        private PuzzleManager puzzle;
        private TransferableObject cube;
        private PlayerInteraction hands;
        private FirstPersonController player;
        private PressurePlate plate;
        private WarpPad whitePad;
        private WarpPad blackPad;
        private Door door;
        private static T One<T>() where T : Object => Object.FindFirstObjectByType<T>();
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene("Level01"); yield return null; yield return new WaitForFixedUpdate();
            dimensions = One<DimensionManager>(); puzzle = One<PuzzleManager>(); cube = One<TransferableObject>();
            hands = One<PlayerInteraction>(); player = One<FirstPersonController>(); plate = One<PressurePlate>(); door = One<Door>();
            foreach (WarpPad pad in Object.FindObjectsByType<WarpPad>(FindObjectsSortMode.None))
                if (pad.Dimension == Dimension.White) whitePad = pad; else blackPad = pad;
        }
        private void PositionPlayer(Vector3 position)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            controller.enabled = false; player.transform.position = position; controller.enabled = true; Physics.SyncTransforms();
        }
        private IEnumerator Switch()
        { yield return new WaitForSeconds(0.32f); Assert.IsTrue(dimensions.TrySwitch()); yield return new WaitForFixedUpdate(); }
        private IEnumerator TransferToWhite()
        {
            yield return Switch();
            Assert.IsTrue(hands.TryPickUp(cube));
            PositionPlayer(new Vector3(-3, 0.1f, 12.2f));
            Assert.IsTrue(hands.TryTransfer(blackPad));
            yield return Switch();
        }
        [UnityTest]
        public IEnumerator InitialWorldAndDimensionVisibility()
        {
            Assert.AreEqual(Dimension.White, dimensions.Current);
            Assert.IsFalse(cube.GetComponent<Renderer>().enabled); Assert.IsFalse(cube.GetComponent<Collider>().enabled);
            Assert.IsTrue(cube.gameObject.activeSelf); Assert.IsTrue(plate.GetComponent<Collider>().enabled);
            Assert.IsFalse(puzzle.DoorOpened); Assert.IsFalse(puzzle.Completed);
            Assert.IsFalse(Physics.Raycast(new Vector3(0, 2, 8), Vector3.down, 4));
            yield return Switch();
            Assert.IsTrue(cube.GetComponent<Renderer>().enabled); Assert.IsTrue(cube.GetComponent<Collider>().enabled);
            Assert.IsFalse(plate.GetComponent<Collider>().enabled);
            Assert.IsTrue(Physics.Raycast(new Vector3(0, 2, 8), Vector3.down, 4));
        }
        [UnityTest]
        public IEnumerator TransferMovesOnlyObjectAndBlocksHeldSwitch()
        {
            yield return Switch(); Assert.IsTrue(hands.TryPickUp(cube));
            yield return new WaitForSeconds(0.32f);
            Vector3 playerPosition = player.transform.position;
            Assert.IsFalse(dimensions.TrySwitch());
            Assert.IsTrue(hands.TryTransfer(blackPad));
            Assert.AreEqual(Dimension.Black, dimensions.Current); Assert.AreEqual(Dimension.White, cube.Dimension);
            Assert.Less(Vector3.Distance(cube.transform.position, whitePad.Arrival), 0.01f);
            Assert.AreEqual(playerPosition, player.transform.position); Assert.IsNull(hands.HeldObject);
            Assert.IsTrue(cube.GetComponent<Rigidbody>().isKinematic); Assert.IsFalse(cube.IsAvailable);
            yield return Switch(); Assert.IsTrue(cube.IsAvailable);
        }
        [UnityTest]
        public IEnumerator DoorRequiresPlateThenLatchesAndCompletes()
        {
            Assert.IsFalse(puzzle.TryOpenDoor(plate)); Assert.IsFalse(puzzle.Complete());
            yield return TransferToWhite();
            Assert.IsFalse(puzzle.DoorOpened);
            Assert.IsTrue(hands.TryPickUp(cube));
            hands.PlaceHeld(plate.Placement); yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsTrue(plate.IsSatisfied); Assert.IsTrue(puzzle.DoorOpened); Assert.IsTrue(door.IsOpen);
            Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(new Vector3(0, 0.5f, 15));
            yield return Switch(); yield return Switch();
            Assert.IsTrue(puzzle.DoorOpened); Assert.IsTrue(door.IsOpen);
            yield return new WaitForSeconds(1.1f);
            Assert.Greater(door.transform.position.y, 5);
            PositionPlayer(new Vector3(0, 0.1f, 22.8f));
            player.GetComponent<CharacterController>().Move(Vector3.forward * 0.1f);
            yield return new WaitForFixedUpdate(); yield return null;
            Assert.IsTrue(puzzle.Completed); Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            bool completeVisible = false;
            foreach (Text text in Object.FindObjectsByType<Text>(FindObjectsSortMode.None)) if (text.text == "Puzzle complete") completeVisible = true;
            Assert.IsTrue(completeVisible);
        }
        [UnityTest]
        public IEnumerator BlackCubeCannotOpenWhitePlate()
        {
            yield return Switch(); Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(plate.Placement);
            yield return new WaitForFixedUpdate(); Assert.IsFalse(puzzle.DoorOpened);
            yield return Switch(); yield return new WaitForFixedUpdate();
            Assert.IsFalse(puzzle.DoorOpened); Assert.IsFalse(plate.IsSatisfied);
        }
        [UnityTest]
        public IEnumerator SwitchBlockedByTargetGeometry()
        {
            // The hidden starting Cube would overlap this player's body in Black.
            PositionPlayer(new Vector3(-3, 0.72f, 1));
            Assert.IsFalse(dimensions.TrySwitch()); Assert.AreEqual(Dimension.White, dimensions.Current);
            PositionPlayer(new Vector3(0, 0.1f, -2)); yield return Switch();
        }
        [UnityTest]
        public IEnumerator FallAndCubeRecoveryKeepProgressAndMembership()
        {
            yield return TransferToWhite();
            cube.transform.position = new Vector3(0, -9, 8); yield return null;
            Assert.AreEqual(Dimension.White, cube.Dimension); Assert.Less(Vector3.Distance(cube.transform.position, whitePad.Arrival), 0.1f);
            Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(plate.Placement);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate(); Assert.IsTrue(puzzle.DoorOpened);
            Assert.IsTrue(hands.TryPickUp(cube)); player.Respawn();
            Assert.IsNull(hands.HeldObject); Assert.AreEqual(Dimension.White, dimensions.Current);
            Assert.Less(Vector3.Distance(cube.transform.position, whitePad.Arrival), 0.1f); Assert.IsTrue(puzzle.DoorOpened);
            yield return Switch();
            cube.MoveToDimension(Dimension.Black, new Vector3(0, -9, 8)); yield return null;
            Assert.AreEqual(Dimension.Black, cube.Dimension); Assert.Less(Vector3.Distance(cube.transform.position, new Vector3(-3, 1.15f, 1)), 0.1f);
        }
        [UnityTest]
        public IEnumerator BridgeSwitchFallsAndRespawns()
        {
            yield return Switch(); PositionPlayer(new Vector3(0, 0.1f, 8));
            yield return Switch(); yield return new WaitForSeconds(1.6f);
            Assert.Less(player.transform.position.z, 0); Assert.Greater(player.transform.position.y, -1);
            Assert.AreEqual(Dimension.White, dimensions.Current);
        }
        [UnityTest]
        public IEnumerator SpamDoesNotDuplicateTransfersOrSwitches()
        {
            Assert.IsTrue(dimensions.TrySwitch());
            for (int i = 0; i < 30; i++) Assert.IsFalse(dimensions.TrySwitch());
            Assert.IsTrue(hands.TryPickUp(cube)); Assert.IsFalse(hands.TryPickUp(cube));
            Assert.IsTrue(hands.TryTransfer(blackPad));
            for (int i = 0; i < 30; i++) Assert.IsFalse(hands.TryTransfer(blackPad));
            Assert.AreEqual(Dimension.White, cube.Dimension); Assert.AreEqual(Dimension.Black, dimensions.Current);
            yield return null;
        }
        [UnityTest]
        public IEnumerator RestartClearsCompleteStateAndRepeatedReloadRequests()
        {
            yield return TransferToWhite(); Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(plate.Placement);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate(); Assert.IsTrue(puzzle.Complete());
            GameFlow flow = One<GameFlow>(); for (int i = 0; i < 20; i++) flow.Restart();
            yield return null; yield return new WaitForFixedUpdate();
            Assert.AreEqual(Dimension.White, One<DimensionManager>().Current); Assert.AreEqual(Dimension.Black, One<TransferableObject>().Dimension);
            Assert.IsFalse(One<PuzzleManager>().DoorOpened); Assert.IsFalse(One<PuzzleManager>().Completed); Assert.IsFalse(One<Door>().IsOpen);
            Assert.Less(Vector3.Distance(One<TransferableObject>().transform.position, new Vector3(-3, 1.15f, 1)), 0.1f);
            Assert.Less(One<FirstPersonController>().transform.position.z, 0);
            foreach (Text text in Object.FindObjectsByType<Text>(FindObjectsSortMode.None)) Assert.AreNotEqual("Puzzle complete", text.text);
        }
        [UnityTest]
        public IEnumerator CubeLeftBehindAndDroppedAtStartRemainReachable()
        {
            yield return Switch(); Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(new Vector3(-2, 0.5f, 2));
            yield return new WaitForSeconds(0.2f); PositionPlayer(new Vector3(0, 0.1f, 12));
            yield return Switch(); yield return Switch();
            Assert.IsTrue(Physics.Raycast(new Vector3(0, 2, 8), Vector3.down, 4));
            Assert.IsTrue(cube.IsAvailable); Assert.Less(cube.transform.position.z, 5);
            PositionPlayer(new Vector3(-2, 0.1f, 1)); Assert.IsTrue(hands.TryPickUp(cube));
        }
        [UnityTest]
        public IEnumerator CharacterControllerCrossesBridgeAndClosedExitBlocks()
        {
            yield return Switch(); PositionPlayer(new Vector3(0, 0.1f, 3));
            CharacterController controller = player.GetComponent<CharacterController>();
            for (int i = 0; i < 120; i++) { controller.Move(new Vector3(0, -0.04f, 0.08f)); yield return new WaitForFixedUpdate(); }
            Assert.Greater(player.transform.position.z, 11); Assert.Greater(player.transform.position.y, -0.2f);
            PositionPlayer(new Vector3(0, 0.1f, 19));
            for (int i = 0; i < 40; i++) { controller.Move(new Vector3(0, -0.04f, 0.08f)); yield return new WaitForFixedUpdate(); }
            Assert.Less(player.transform.position.z, 21); Assert.IsFalse(puzzle.Completed);
        }
        [UnityTest]
        public IEnumerator CameraTargetedInteractionSolvesPuzzle()
        {
            yield return Switch();
            PositionPlayer(new Vector3(-3, 0.1f, -0.8f));
            Camera camera = player.GetComponentInChildren<Camera>();
            camera.transform.LookAt(cube.transform.position);
            hands.ProcessInteraction(true); Assert.AreEqual(cube, hands.HeldObject);
            yield return new WaitForSeconds(0.22f);
            PositionPlayer(new Vector3(-3, 0.1f, 12.2f));
            camera.transform.LookAt(blackPad.Arrival);
            hands.ProcessInteraction(true); Assert.IsNull(hands.HeldObject); Assert.AreEqual(Dimension.White, cube.Dimension);
            yield return Switch(); yield return new WaitForSeconds(0.22f);
            camera.transform.LookAt(cube.transform.position);
            hands.ProcessInteraction(true); Assert.AreEqual(cube, hands.HeldObject);
            yield return new WaitForSeconds(0.22f);
            PositionPlayer(new Vector3(3, 0.1f, 15.5f)); camera.transform.LookAt(plate.Placement);
            hands.ProcessInteraction(true); Assert.IsNull(hands.HeldObject);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsTrue(puzzle.DoorOpened);
        }
        [UnityTest]
        public IEnumerator NormalDropAndWallObstructionUseCameraInteraction()
        {
            yield return Switch(); Assert.IsTrue(hands.TryPickUp(cube));
            PositionPlayer(new Vector3(0, 0.1f, -5.4f)); player.transform.rotation = Quaternion.Euler(0, 180, 0);
            hands.ProcessInteraction(true); Assert.AreEqual(cube, hands.HeldObject);
            yield return new WaitForSeconds(0.22f);
            PositionPlayer(new Vector3(0, 0.1f, 1)); player.transform.rotation = Quaternion.identity;
            hands.ProcessInteraction(true); Assert.IsNull(hands.HeldObject); Assert.IsTrue(cube.GetComponent<Collider>().enabled);
            yield return new WaitForSeconds(0.3f); Assert.IsTrue(cube.IsAvailable); Assert.Greater(cube.transform.position.y, 0);
        }
        [UnityTest]
        public IEnumerator ReturnTransferAndRestartButtonWork()
        {
            yield return TransferToWhite(); Assert.IsTrue(hands.TryPickUp(cube));
            Assert.IsTrue(hands.TryTransfer(whitePad)); Assert.AreEqual(Dimension.White, dimensions.Current);
            Assert.AreEqual(Dimension.Black, cube.Dimension); Assert.Less(Vector3.Distance(cube.transform.position, blackPad.Arrival), 0.01f);
            yield return Switch(); Assert.IsTrue(hands.TryPickUp(cube)); Assert.IsTrue(hands.TryTransfer(blackPad));
            yield return Switch(); Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(plate.Placement);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate(); Assert.IsTrue(puzzle.Complete());
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                if (button.name == "Restart") button.onClick.Invoke(); yield return null; yield return new WaitForFixedUpdate();
            Assert.IsFalse(One<PuzzleManager>().Completed); Assert.IsFalse(One<PuzzleManager>().DoorOpened);
            Assert.AreEqual(Dimension.Black, One<TransferableObject>().Dimension);
        }
        [UnityTest]
        public IEnumerator HeldCubeFollowsCameraBetweenPhysicsSteps()
        {
            yield return Switch();
            player.enabled = false;
            Camera camera = player.GetComponentInChildren<Camera>();
            Assert.IsTrue(hands.TryPickUp(cube));
            Transform grip = cube.transform.parent;
            float previousStep = Time.fixedDeltaTime;
            // Deliberately separate render and physics timing to expose interpolation drift.
            Time.fixedDeltaTime = 0.08f;
            try
            {
                for (int frame = 0; frame < 50; frame++)
                {
                    player.transform.position = new Vector3(Mathf.Sin(frame * 0.23f), 0.12f, -2);
                    player.transform.rotation = Quaternion.Euler(0, frame * 9, 0);
                    camera.transform.localRotation = Quaternion.Euler(Mathf.Sin(frame * 0.37f) * 55, 0, 0);
                    yield return null;
                    Assert.Less(Vector3.Distance(cube.transform.position, grip.position), 0.001f,
                        "Held Cube drifted from its grip at rendered frame " + frame);
                    Assert.Less(Quaternion.Angle(cube.transform.rotation, grip.rotation), 0.05f,
                        "Held Cube lagged behind camera rotation at rendered frame " + frame);
                }
            }
            finally { Time.fixedDeltaTime = previousStep; }
        }

        [UnityTest]
        public IEnumerator CarryReleaseTransferAndRecoveryRestorePhysics()
        {
            yield return Switch();
            Rigidbody body = cube.GetComponent<Rigidbody>();
            RigidbodyInterpolation restingMode = body.interpolation;
            for (int cycle = 0; cycle < 3; cycle++)
            {
                Assert.IsTrue(hands.TryPickUp(cube));
                Assert.AreEqual(RigidbodyInterpolation.None, body.interpolation);
                hands.PlaceHeld(new Vector3(1, 1.5f, 2));
                Assert.AreEqual(restingMode, body.interpolation);
                yield return null;
                Assert.Less(Vector3.Distance(cube.transform.position, new Vector3(1, 1.5f, 2)), 0.2f);
                Assert.IsFalse(body.isKinematic); Assert.IsTrue(cube.GetComponent<Collider>().enabled);
            }
            Assert.IsTrue(hands.TryPickUp(cube)); Assert.IsTrue(hands.TryTransfer(blackPad));
            Assert.AreEqual(restingMode, body.interpolation);
            yield return Switch();
            Assert.Less(Vector3.Distance(cube.transform.position, whitePad.Arrival), 0.2f);
            Assert.IsTrue(hands.TryPickUp(cube)); player.Respawn();
            Assert.AreEqual(restingMode, body.interpolation);
            yield return null;
            Assert.Less(Vector3.Distance(cube.transform.position, whitePad.Arrival), 0.2f);
        }

        [UnityTest]
        public IEnumerator ExpandedFloorAndPerimeterWorkInBothDimensions()
        {
            for (int world = 0; world < 2; world++)
            {
                foreach (float x in new[] { -7.05f, 7.05f })
                {
                    foreach (float z in new[] { -5.55f, 0f, 4f, 12f, 18f, 20f, 24.5f })
                    {
                        PositionPlayer(new Vector3(x, 0.1f, z));
                        for (int step = 0; step < 5; step++)
                        {
                            player.GetComponent<CharacterController>().Move(new Vector3(Mathf.Sign(x) * 0.2f, -0.08f, 0));
                            yield return new WaitForFixedUpdate();
                        }
                        Assert.Less(Mathf.Abs(player.transform.position.x), 7.3f);
                        Assert.Greater(player.transform.position.y, -0.1f);
                    }
                    PositionPlayer(new Vector3(x, 0.1f, 20));
                    player.GetComponent<CharacterController>().Move(Vector3.forward * 2);
                    Assert.Less(player.transform.position.z, 21, "Exit partition must meet widened wall");
                    Assert.IsFalse(puzzle.Completed);
                    Assert.IsFalse(Physics.Raycast(new Vector3(x, 2, 8), Vector3.down, 3), "No new chasm bypass");
                }
                PositionPlayer(new Vector3(0, 0.1f, -5.55f));
                player.GetComponent<CharacterController>().Move(Vector3.back * 2);
                Assert.Greater(player.transform.position.z, -5.8f);
                PositionPlayer(new Vector3(0, 0.1f, 24.5f));
                player.GetComponent<CharacterController>().Move(Vector3.forward * 2);
                Assert.Less(player.transform.position.z, 24.8f);
                PositionPlayer(new Vector3(0, 0.1f, -2));
                if (world == 0) yield return Switch();
            }
        }

        [UnityTest]
        public IEnumerator PreviewFrames()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) Assert.Ignore("Preview capture requires a graphics device.");
            yield return new WaitForSeconds(0.4f); Capture("White");
            yield return Switch(); yield return new WaitForSeconds(0.4f); Capture("Black");
            Assert.IsTrue(hands.TryPickUp(cube)); PositionPlayer(new Vector3(-3, 0.1f, 12.2f));
            Assert.IsTrue(hands.TryTransfer(blackPad)); yield return Switch();
            Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(plate.Placement);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(1.1f);
            PositionPlayer(new Vector3(0, 0.1f, 13)); Capture("Solved");
            Assert.IsTrue(puzzle.Complete()); Capture("Complete");
        }
        private void Capture(string name)
        {
            Camera camera = player.GetComponentInChildren<Camera>();
            Canvas canvas = One<HUD>().GetComponent<Canvas>();
            RenderTexture target = new RenderTexture(1280, 720, 24);
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 0.3f;
            Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
            Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
            string folder = System.IO.Path.Combine(Application.dataPath, "../Docs/Previews");
            System.IO.Directory.CreateDirectory(folder); System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder, name + ".png"), image.EncodeToPNG());
            camera.targetTexture = null; RenderTexture.active = previous; canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Object.Destroy(image); Object.Destroy(target);
        }
    }
}
