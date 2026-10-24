using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Paradox.Tests
{
    public sealed class LevelCompletionSoundTests
    {
        private static T One<T>() where T : Object => Object.FindFirstObjectByType<T>();
        [UnityTest]
        public IEnumerator GenuineCompletionAndAllMenuActionsInEveryLevel()
        {
            for (int level = 0; level < 5; level++)
            for (int action = 0; action < 3; action++)
            {
                SceneManager.LoadScene(MainMenu.LevelScenes[level]);
                yield return null;
                yield return null;
                var state = One<PuzzleManager>();
                var menu = One<LevelMenus>();
                var sound = state.transform.Find("Level completion sound").GetComponent<AudioSource>();
                Assert.IsNotNull(sound.clip);
                Assert.AreEqual(AudioDataLoadState.Loaded, sound.clip.loadState);
                Assert.IsFalse(sound.isPlaying);
                Assert.IsFalse(state.Complete(), "Unsolved puzzle must not complete");
                Assert.IsFalse(sound.isPlaying);
                menu.TogglePause();
                Assert.IsFalse(sound.isPlaying, "Pause must not play completion cue");
                menu.Resume();
                var player = One<FirstPersonController>();
                player.enabled = false;
                var config = One<PuzzleConfigurationSelector>().ActiveConfiguration;
                var dimensions = One<DimensionManager>();
                var cube = One<TransferableObject>();
                var plate = One<PressurePlate>();
                cube.MoveToDimension(config.plateDimension, plate.Placement);
                if (dimensions.Current != config.plateDimension) Assert.IsTrue(dimensions.TrySwitch());
                yield return new WaitForSeconds(1.2f);
                Assert.IsTrue(state.DoorOpened);
                if (dimensions.Current != config.gateDimension) Assert.IsTrue(dimensions.TrySwitch());
                yield return new WaitForFixedUpdate();
                int events = 0;
                state.CompletionShown += () =>
                {
                    events++;
                    Assert.IsTrue(menu.Completed);
                    Assert.IsTrue(menu.Open);
                    Assert.IsTrue(sound.isPlaying, "Cue starts in the popup's completion frame");
                    Assert.Less(sound.time, .1f);
                };
                var controller = player.GetComponent<CharacterController>();
                controller.enabled = false;
                player.transform.position = new Vector3(0, 0, 23);
                controller.enabled = true;
                Physics.SyncTransforms();
                float deadline = Time.realtimeSinceStartup + 3;
                while (!state.Completed && Time.realtimeSinceStartup < deadline)
                {
                    controller.Move(Vector3.down * .01f);
                    yield return null;
                }
                Assert.IsTrue(state.Completed);
                Assert.AreEqual(1, events);
                Assert.AreEqual(0, Time.timeScale);
                for (int repeat = 0; repeat < 3; repeat++)
                {
                    Assert.IsFalse(state.Complete());
                    menu.ShowCompletion();
                    yield return new WaitForSecondsRealtime(.1f);
                }
                Assert.AreEqual(1, events);
                Assert.Greater(sound.time, .2f, "Repeated completion checks must not restart the cue");
                Assert.IsTrue(One<JourneyMusic>().GetComponent<AudioSource>().isPlaying);
                yield return new WaitForSecondsRealtime(1.4f);
                Assert.IsFalse(sound.isPlaying, "Completion cue must not loop at timeScale zero");
                if (action == 0) menu.Retry();
                else if (action == 1) menu.Home();
                else menu.Continue();
                yield return null;
                yield return null;
                string expected = action == 0 ? MainMenu.LevelScenes[level] :
                    action == 1 || level == 4 ? "MainMenu" : MainMenu.LevelScenes[level + 1];
                Assert.AreEqual(expected, SceneManager.GetActiveScene().name);
                foreach (var cue in Object.FindObjectsByType<LevelCompletionSound>(FindObjectsSortMode.None))
                    Assert.IsFalse(cue.transform.Find("Level completion sound").GetComponent<AudioSource>().isPlaying,
                        "Navigation must not independently play completion audio");
                LogAssert.NoUnexpectedReceived();
            }
        }
        [TearDown] public void RestoreTime() { Time.timeScale = 1; }
    }
}
