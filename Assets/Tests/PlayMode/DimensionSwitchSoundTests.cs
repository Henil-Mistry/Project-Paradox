using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Paradox.Tests
{
    public sealed class DimensionSwitchSoundTests
    {
        [UnityTest] public IEnumerator Garden() { yield return Check("Level01"); }
        [UnityTest] public IEnumerator Library() { yield return Check("Level02_Library"); }
        [UnityTest] public IEnumerator Lake() { yield return Check("Level03_MirrorLake"); }
        [UnityTest] public IEnumerator Observatory() { yield return Check("Level04_Observatory"); }
        [UnityTest] public IEnumerator Heart() { yield return Check("Level05_Heart"); }

        private IEnumerator Check(string scene)
        {
            SceneManager.LoadScene(scene);
            yield return null;
            yield return new WaitForSecondsRealtime(.2f);
            var manager = Object.FindFirstObjectByType<DimensionManager>();
            var transition = manager.GetComponent<RealmTransition>();
            Assert.AreEqual(1, manager.GetComponents<DimensionSwitchSound>().Length);
            Assert.AreEqual(1, manager.GetComponents<AudioSource>().Length);
            var voice = manager.GetComponent<AudioSource>();
            var music = Object.FindFirstObjectByType<JourneyMusic>().GetComponent<AudioSource>();
            Assert.IsNotNull(voice.clip);
            Assert.AreEqual(AudioDataLoadState.Loaded, voice.clip.loadState,
                "Effect must be decoded before the first dimension switch");
            Assert.IsFalse(voice.isPlaying, "No sound at scene startup");
            Assert.IsFalse(voice.loop);
            Assert.AreEqual(0, voice.spatialBlend);
            Assert.AreEqual(.35f, voice.volume);
            int commits = 0;
            manager.Changed += dimension =>
            {
                commits++;
                Assert.AreEqual(1, transition.Coverage);
                Assert.IsTrue(voice.isPlaying, "Sound starts at actual commit");
                Assert.Less(voice.time, .1f);
            };
            for (int i = 0; i < 4; i++)
            {
                var before = manager.Current;
                float musicTime = music.time;
                Assert.IsTrue(manager.RequestSwitch());
                Assert.IsFalse(manager.RequestSwitch(), "Spam request must be rejected");
                float last = voice.time;
                while (transition.Busy)
                {
                    Assert.IsFalse(manager.TrySwitch());
                    yield return null;
                    if (manager.Current == before && voice.isPlaying)
                        Assert.GreaterOrEqual(voice.time + .05f, last, "No restart on input before commit");
                    last = voice.time;
                }
                Assert.AreNotEqual(before, manager.Current);
                Assert.AreEqual(i + 1, commits);
                Assert.IsTrue(voice.isPlaying);
                Assert.Greater(voice.time, .2f, "No per-frame restart during reveal");
                Assert.IsTrue(music.isPlaying);
                Assert.Greater(music.time, musicTime, "Music continues independently");
            }
            yield return new WaitForSecondsRealtime(3f);
            Assert.IsFalse(voice.isPlaying, "One-shot ends naturally");
            // Exercise a failed commit through the real transition while holding a puzzle cube.
            var hands = Object.FindFirstObjectByType<PlayerInteraction>();
            TransferableObject cube = null;
            foreach (var candidate in Object.FindObjectsByType<TransferableObject>(FindObjectsSortMode.None))
                if (candidate.IsAvailable) { cube = candidate; break; }
            if (!cube)
            {
                // Garden may spawn its only cube in the other dimension.
                Assert.IsTrue(manager.RequestSwitch());
                while (transition.Busy) yield return null;
                yield return new WaitForSecondsRealtime(3f);
                foreach (var candidate in Object.FindObjectsByType<TransferableObject>(FindObjectsSortMode.None))
                    if (candidate.IsAvailable) { cube = candidate; break; }
            }
            int beforeBlocked = commits;
            Assert.IsNotNull(cube, "Select a cube present in the current dimension");
            Assert.IsTrue(hands.TryPickUp(cube));
            var unchanged = manager.Current;
            Assert.IsTrue(manager.RequestSwitch());
            while (transition.Busy)
            {
                Assert.IsFalse(voice.isPlaying, "Blocked switch must remain silent");
                yield return null;
            }
            Assert.AreEqual(unchanged, manager.Current);
            Assert.AreEqual(beforeBlocked, commits);
            Assert.IsFalse(voice.isPlaying);
            hands.RecoverHeldObject();
            LogAssert.NoUnexpectedReceived();
        }
    }
}
