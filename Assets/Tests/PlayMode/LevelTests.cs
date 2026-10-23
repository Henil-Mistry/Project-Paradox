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
        private TransferGun gun;
        private Vector3 Home => GameObject.Find("White Cube Home").transform.position;
        private Door door;
        private static T One<T>() where T : Object => Object.FindFirstObjectByType<T>();
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            PuzzleConfigurationSelector.DebugSeed = 0;
            SceneManager.LoadScene("Level01"); yield return null; yield return new WaitForFixedUpdate();
            dimensions = One<DimensionManager>(); puzzle = One<PuzzleManager>(); cube = One<TransferableObject>();
            hands = One<PlayerInteraction>(); player = One<FirstPersonController>(); plate = One<PressurePlate>(); door = One<Door>();
            gun = One<TransferGun>(); player.enabled = false;
        }
        [TearDown] public void ClearSeed() { PuzzleConfigurationSelector.DebugSeed = null; }
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
            cube.MoveToDimension(Dimension.Black, new Vector3(-3,.5f,14));
            PositionPlayer(new Vector3(-3,.1f,12.2f));
            if (!gun.Equipped) gun.ToggleEquipped();
            player.GetComponentInChildren<Camera>().transform.LookAt(cube.transform.position);
            Assert.IsTrue(gun.TryFire());
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
        public IEnumerator DoorTracksPlateRepeatedlyAndCompletes()
        {
            Assert.IsFalse(puzzle.TryOpenDoor(plate)); Assert.IsFalse(puzzle.Complete());
            yield return TransferToWhite();
            Assert.IsFalse(puzzle.DoorOpened);
            Assert.IsTrue(hands.TryPickUp(cube));
            hands.PlaceHeld(plate.Placement); yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsTrue(plate.IsSatisfied); Assert.IsTrue(puzzle.DoorOpened); Assert.IsTrue(door.IsOpen);
            for (int cycle = 0; cycle < 3; cycle++)
            {
                yield return new WaitForSeconds(1.1f);
                Assert.Greater(door.transform.position.y, 5);
                Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(new Vector3(0, 0.5f, 15));
                yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
                Assert.IsFalse(plate.IsSatisfied); Assert.IsFalse(puzzle.DoorOpened); Assert.IsFalse(door.IsOpen);
                Assert.IsFalse(puzzle.Complete());
                yield return new WaitForSeconds(1.1f);
                Assert.That(door.transform.position.y, Is.EqualTo(1.6f).Within(0.01f));
                Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(plate.Placement);
                yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
                Assert.IsTrue(puzzle.DoorOpened);
            }
            yield return new WaitForSeconds(1.1f);
            yield return Switch();
            PositionPlayer(new Vector3(0, 0.1f, 22.8f));
            player.GetComponent<CharacterController>().Move(Vector3.forward * 0.1f);
            yield return new WaitForFixedUpdate(); yield return null;
            Assert.IsTrue(puzzle.Completed); Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            bool completeVisible = false;
            foreach (Text text in Object.FindObjectsByType<Text>(FindObjectsSortMode.None)) if (text.text == "Puzzle complete") completeVisible = true;
            Assert.IsTrue(completeVisible);
        }
        [UnityTest]
        public IEnumerator PlateReleasesAfterResetTransferDestroyAndLastOccupant()
        {
            cube.MoveToDimension(Dimension.White, plate.Placement);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsTrue(puzzle.DoorOpened);
            cube.Recover();
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsFalse(puzzle.DoorOpened);
            cube.MoveToDimension(Dimension.White, plate.Placement);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsTrue(puzzle.DoorOpened);
            cube.MoveToDimension(Dimension.Black, plate.Placement);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsFalse(puzzle.DoorOpened);
            cube.MoveToDimension(Dimension.White, plate.Placement);
            var second = Object.Instantiate(cube.gameObject).GetComponent<TransferableObject>();
            second.MoveToDimension(Dimension.White, plate.Placement + Vector3.right * 0.6f);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            cube.Recover();
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsTrue(puzzle.DoorOpened, "Remaining valid occupant must keep the door open");
            Object.Destroy(second.gameObject);
            yield return null; yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsFalse(plate.IsSatisfied); Assert.IsFalse(puzzle.DoorOpened);
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
            PositionPlayer(cube.transform.position - Vector3.up * 0.3f);
            Assert.IsFalse(dimensions.TrySwitch()); Assert.AreEqual(Dimension.White, dimensions.Current);
            PositionPlayer(new Vector3(0, 0.1f, -2)); yield return Switch();
        }
        [UnityTest]
        public IEnumerator FallAndCubeRecoveryKeepProgressAndMembership()
        {
            yield return TransferToWhite();
            cube.transform.position = new Vector3(0, -9, 8); yield return null;
            Assert.AreEqual(Dimension.White, cube.Dimension); Assert.Less(Vector3.Distance(cube.transform.position, Home), 0.1f);
            Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(plate.Placement);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate(); Assert.IsTrue(puzzle.DoorOpened);
            Assert.IsTrue(hands.TryPickUp(cube)); player.Respawn();
            Assert.IsNull(hands.HeldObject); Assert.AreEqual(Dimension.White, dimensions.Current);
            Assert.Less(Vector3.Distance(cube.transform.position, Home), 0.1f);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate(); Assert.IsFalse(puzzle.DoorOpened);
            yield return Switch();
            cube.MoveToDimension(Dimension.Black, new Vector3(0, -9, 8)); yield return null;
            Assert.AreEqual(Dimension.Black, cube.Dimension); Assert.Less(Vector3.Distance(cube.transform.position, new Vector3(-10, 0.5f, -12)), 0.1f);
        }
        [UnityTest]
        public IEnumerator BridgeSwitchFallsAndRespawns()
        {
            player.enabled = true;
            yield return Switch(); PositionPlayer(new Vector3(0, 0.1f, 8));
            yield return Switch(); yield return new WaitForSeconds(1.6f);
            Assert.Less(player.transform.position.z, 0); Assert.Greater(player.transform.position.y, -1);
            Assert.AreEqual(Dimension.White, dimensions.Current);
        }
        [UnityTest]
        public IEnumerator RestartClearsCompleteStateAndRepeatedReloadRequests()
        {
            yield return TransferToWhite(); Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(plate.Placement);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate(); yield return Switch(); Assert.IsTrue(puzzle.Complete());
            GameFlow flow = One<GameFlow>(); for (int i = 0; i < 20; i++) flow.Restart();
            yield return null; yield return new WaitForFixedUpdate();
            Assert.AreEqual(Dimension.White, One<DimensionManager>().Current); Assert.AreEqual(Dimension.Black, One<TransferableObject>().Dimension);
            Assert.IsFalse(One<PuzzleManager>().DoorOpened); Assert.IsFalse(One<PuzzleManager>().Completed); Assert.IsFalse(One<Door>().IsOpen);
            Assert.Less(Vector3.Distance(One<TransferableObject>().transform.position, new Vector3(-10, 0.5f, -12)), 0.1f);
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
            PositionPlayer(new Vector3(cube.transform.position.x, 0.1f, cube.transform.position.z + 1.8f));
            Camera camera = player.GetComponentInChildren<Camera>();
            camera.transform.LookAt(cube.transform.position);
            hands.ProcessInteraction(true); Assert.AreEqual(cube, hands.HeldObject);
            yield return new WaitForSeconds(0.22f);
            PositionPlayer(new Vector3(-3, 0.1f, 12.2f));
            hands.PlaceHeld(new Vector3(-3,.5f,14));
            if (!gun.Equipped) gun.ToggleEquipped();
            camera.transform.LookAt(cube.transform.position);
            Assert.IsTrue(gun.TryFire()); Assert.IsNull(hands.HeldObject); Assert.AreEqual(Dimension.White, cube.Dimension);
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
            PositionPlayer(new Vector3(0, 0.1f, -22.4f)); player.transform.rotation = Quaternion.Euler(0, 180, 0);
            hands.ProcessInteraction(true); Assert.AreEqual(cube, hands.HeldObject);
            yield return new WaitForSeconds(0.22f);
            PositionPlayer(new Vector3(0, 0.1f, 1)); player.transform.rotation = Quaternion.identity;
            hands.ProcessInteraction(true); Assert.IsNull(hands.HeldObject); Assert.IsTrue(cube.GetComponent<Collider>().enabled);
            yield return new WaitForSeconds(0.3f); Assert.IsTrue(cube.IsAvailable); Assert.Greater(cube.transform.position.y, 0);
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
            cube.MoveToDimension(Dimension.Black, new Vector3(-3,.5f,14));
            PositionPlayer(new Vector3(-3,.1f,12.2f)); gun.ToggleEquipped();
            player.GetComponentInChildren<Camera>().transform.LookAt(cube.transform.position);
            Assert.IsTrue(gun.TryFire());
            Assert.AreEqual(restingMode, body.interpolation);
            yield return Switch();
            Assert.Less(Vector3.Distance(cube.transform.position, new Vector3(-3,.5f,14)), 0.2f);
            Assert.IsTrue(hands.TryPickUp(cube)); player.Respawn();
            Assert.AreEqual(restingMode, body.interpolation);
            yield return null;
            Assert.Less(Vector3.Distance(cube.transform.position, Home), 0.2f);
        }

        [UnityTest]
        public IEnumerator GunRejectsSceneryHeldObjectsChasmAndHiddenObstructions()
        {
            yield return Switch();
            var camera = player.GetComponentInChildren<Camera>(); gun.ToggleEquipped();
            camera.transform.LookAt(camera.transform.position + Vector3.up);
            Assert.IsFalse(gun.TryFire());
            yield return new WaitForSeconds(.31f);
            Assert.IsTrue(hands.TryPickUp(cube)); Assert.IsFalse(gun.TryFire()); Assert.IsFalse(dimensions.TrySwitch());
            hands.PlaceHeld(new Vector3(0,.5f,8)); PositionPlayer(new Vector3(0,.1f,6));
            yield return new WaitForSeconds(.31f); camera.transform.LookAt(cube.transform.position);
            Assert.IsFalse(gun.TryFire(), "White has no floor under the Black bridge");
            Assert.AreEqual(Dimension.Black,cube.Dimension);
            cube.MoveToDimension(Dimension.Black,new Vector3(0,.5f,14)); PositionPlayer(new Vector3(0,.1f,12));
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube); blocker.transform.position = cube.transform.position;
            var membership = blocker.AddComponent<DimensionObject>(); membership.Configure(dimensions,DimensionMembership.White);
            yield return new WaitForSeconds(.31f); camera.transform.LookAt(cube.transform.position);
            Assert.IsFalse(gun.TryFire(), "Hidden destination obstruction must block transfer");
            Assert.IsFalse(blocker.GetComponent<Collider>().enabled); Assert.AreEqual(Dimension.Black,cube.Dimension);
            Object.Destroy(blocker); yield return null; yield return new WaitForSeconds(.31f);
            Vector3 position = cube.transform.position;
            Assert.IsTrue(gun.TryFire()); Assert.IsFalse(gun.TryFire());
            Assert.AreEqual(Dimension.Black,dimensions.Current); Assert.AreEqual(Dimension.White,cube.Dimension);
            Assert.Less(Vector3.Distance(position,cube.transform.position),.01f);
            yield return Switch(); yield return new WaitForSeconds(.31f); camera.transform.LookAt(cube.transform.position);
            Assert.IsTrue(gun.TryFire()); Assert.AreEqual(Dimension.Black,cube.Dimension);
            yield return Switch();
            Assert.IsTrue(cube.IsAvailable); Assert.IsFalse(cube.GetComponent<Rigidbody>().isKinematic);
            Assert.IsTrue(hands.TryPickUp(cube)); hands.PlaceHeld(new Vector3(1,.5f,14));
            yield return new WaitForFixedUpdate(); Assert.IsTrue(cube.GetComponent<Collider>().enabled);
            foreach (var pad in Object.FindObjectsByType<WarpPad>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Assert.IsFalse(pad.IsAvailable);
        }
    }
}
