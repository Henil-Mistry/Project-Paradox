using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Paradox.Tests
{
    public sealed class JourneyMusicTests
    {
        [UnityTest]
        public IEnumerator MusicPersistsAcrossScenesDimensionsPauseAndLoop()
        {
            AudioSource voice = null;
            foreach (string scene in new[] { "MainMenu", "Level01", "Level02_Library", "Level03_MirrorLake", "Level04_Observatory", "Level05_Heart" })
            {
                SceneManager.LoadScene(scene);
                yield return null;
                yield return new WaitForSecondsRealtime(1.3f);
                var music = Object.FindObjectsByType<JourneyMusic>(FindObjectsSortMode.None);
                Assert.AreEqual(1, music.Length);
                var current = music[0].GetComponent<AudioSource>();
                if (voice) Assert.AreSame(voice, current);
                voice = current;
                Assert.IsTrue(voice.isPlaying, scene);
                Assert.IsTrue(voice.loop);
                Assert.AreEqual(0, voice.spatialBlend);
                Assert.That(voice.volume, Is.InRange(.24f, .26f));
                Assert.That(voice.clip.length, Is.InRange(65.8f, 66f));
                var manager = Object.FindFirstObjectByType<DimensionManager>();
                if (!manager) continue;
                for (int i = 0; i < 2; i++)
                {
                    float before = voice.time;
                    var dimension = manager.Current;
                    Assert.IsTrue(manager.RequestSwitch());
                    yield return new WaitForSecondsRealtime(1.8f);
                    Assert.AreNotEqual(dimension, manager.Current);
                    Assert.IsTrue(voice.isPlaying);
                    Assert.Greater(voice.time, before, "Dimension switching must not restart music");
                }
            }
            float position = voice.time;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(.4f);
            Assert.Greater(voice.time, position);
            Time.timeScale = 1;
            voice.time = voice.clip.length - .3f;
            yield return new WaitForSecondsRealtime(.8f);
            Assert.IsTrue(voice.isPlaying);
            Assert.Less(voice.time, 2f, "Clip should loop");
            SceneManager.LoadScene("MainMenu");
            yield return new WaitForSecondsRealtime(1.5f);
            Assert.IsTrue(voice.isPlaying);
            Assert.AreEqual(1, Object.FindObjectsByType<JourneyMusic>(FindObjectsSortMode.None).Length);
            LogAssert.NoUnexpectedReceived();
        }

        [TearDown] public void RestoreTime() { Time.timeScale = 1; }
    }
}
