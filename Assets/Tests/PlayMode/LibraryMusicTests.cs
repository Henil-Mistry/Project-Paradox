using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Paradox.Tests
{
    public sealed class LibraryMusicTests
    {
        [UnityTest] public IEnumerator AdditionalLayerIsLocalAndContinuous()
        {
            for (int visit = 0; visit < 2; visit++)
            {
                SceneManager.LoadScene("Level02_Library");
                yield return null;
                yield return new WaitForSecondsRealtime(1.4f);
                var layers = Object.FindObjectsByType<LibraryAdditionalMusic>(FindObjectsSortMode.None);
                Assert.AreEqual(1, layers.Length);
                var voice = layers[0].transform.Find("Library additional music").GetComponent<AudioSource>();
                var music = Object.FindFirstObjectByType<JourneyMusic>().GetComponent<AudioSource>();
                Assert.IsTrue(voice.isPlaying);
                Assert.IsTrue(voice.loop);
                Assert.AreEqual(.12f, voice.volume);
                Assert.AreEqual(.25f, music.volume);
                Assert.AreNotSame(voice, music);
                var manager = Object.FindFirstObjectByType<DimensionManager>();
                for (int i = 0; i < 2; i++)
                {
                    double started = AudioSettings.dspTime;
                    float before = voice.time;
                    float musicBefore = music.time;
                    Assert.IsTrue(manager.RequestSwitch());
                    yield return new WaitForSecondsRealtime(1.8f);
                    float expected = (before + (float)(AudioSettings.dspTime - started)) % voice.clip.length;
                    float difference = Mathf.Abs(expected - voice.time);
                    Assert.Less(Mathf.Min(difference, voice.clip.length - difference), .15f, "Track must not restart on dimension changes");
                    Assert.IsTrue(voice.isPlaying);
                    Assert.Greater(music.time, musicBefore);
                    Assert.AreEqual(.25f, music.volume);
                    Assert.AreEqual(1, Object.FindObjectsByType<LibraryAdditionalMusic>(FindObjectsSortMode.None).Length);
                }
                foreach (string scene in new[] { "MainMenu", "Level01", "Level03_MirrorLake", "Level04_Observatory", "Level05_Heart" })
                {
                    SceneManager.LoadScene(scene);
                    yield return null;
                    yield return null;
                    Assert.IsTrue(voice == null);
                    Assert.AreEqual(0, Object.FindObjectsByType<LibraryAdditionalMusic>(FindObjectsSortMode.None).Length);
                }
            }
            LogAssert.NoUnexpectedReceived();
        }
    }
}
