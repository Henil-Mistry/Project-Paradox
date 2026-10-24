using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Paradox.Tests
{
    public sealed class BlockInteractionSoundTests
    {
        [UnityTest] public IEnumerator PickupReleaseBothDimensionsAllLevels()
        {
            foreach (string scene in MainMenu.LevelScenes)
            {
                SceneManager.LoadScene(scene);
                yield return null;
                yield return new WaitForSecondsRealtime(1.3f);
                var manager = Object.FindFirstObjectByType<DimensionManager>();
                var hands = Object.FindFirstObjectByType<PlayerInteraction>();
                var cube = Object.FindFirstObjectByType<TransferableObject>();
                var voice = cube.GetComponent<AudioSource>();
                var music = Object.FindFirstObjectByType<JourneyMusic>().GetComponent<AudioSource>();
                Assert.AreEqual(1, cube.GetComponents<BlockInteractionSound>().Length);
                Assert.AreEqual(1, cube.GetComponents<AudioSource>().Length);
                Assert.IsFalse(voice.isPlaying);
                Assert.AreEqual(AudioDataLoadState.Loaded, voice.clip.loadState);
                int events = 0;
                cube.Interacted += () => { events++; Assert.IsTrue(voice.isPlaying); Assert.Less(voice.time, .1f); };
                for (int dimension = 0; dimension < 2; dimension++)
                {
                    cube.MoveToDimension(manager.Current, hands.transform.position + Vector3.forward * 2 + Vector3.up);
                    Assert.IsFalse(cube.PickUp(null));
                    Assert.AreEqual(dimension * 2, events);
                    float before = music.time;
                    Assert.IsTrue(hands.TryPickUp(cube));
                    Assert.AreEqual(dimension * 2 + 1, events);
                    Assert.IsFalse(hands.TryPickUp(cube));
                    yield return new WaitForSecondsRealtime(.25f);
                    Assert.Greater(voice.time, .15f);
                    Assert.AreEqual(dimension * 2 + 1, events);
                    hands.PlaceHeld(hands.transform.position + Vector3.forward * 2 + Vector3.up);
                    Assert.AreEqual(dimension * 2 + 2, events);
                    hands.PlaceHeld(cube.transform.position);
                    cube.Release(cube.transform.position); // Non-held release must not emit another cue.
                    yield return new WaitForSecondsRealtime(.25f);
                    Assert.AreEqual(dimension * 2 + 2, events);
                    Assert.Greater(voice.time, .15f);
                    Assert.IsTrue(music.isPlaying);
                    Assert.AreEqual(.25f, music.volume);
                    Assert.Greater(music.time, before);
                    cube.Recover();
                    Assert.AreEqual(dimension * 2 + 2, events);
                    yield return new WaitForSecondsRealtime(3.5f);
                    Assert.IsFalse(voice.isPlaying);
                    if (dimension == 0)
                    {
                        Assert.IsTrue(manager.RequestSwitch());
                        yield return new WaitForSecondsRealtime(1.8f);
                    }
                }
                SceneManager.LoadScene("MainMenu");
                yield return null;
                yield return null;
                Assert.IsTrue(voice == null);
                Assert.AreEqual(0, Object.FindObjectsByType<BlockInteractionSound>(FindObjectsSortMode.None).Length);
            }
            LogAssert.NoUnexpectedReceived();
        }
    }
}
