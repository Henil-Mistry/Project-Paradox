using UnityEngine;

namespace Paradox
{
    /// <summary>One non-spatial cue when confirmed completion opens the popup.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(PuzzleManager))]
    public sealed class LevelCompletionSound : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float volume = 0.4f;
        private PuzzleManager puzzle;
        private AudioSource voice;
        private bool played;

        private void Awake()
        {
            puzzle = GetComponent<PuzzleManager>();
            var sound = new GameObject("Level completion sound");
            sound.transform.SetParent(transform, false);
            voice = sound.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0;
            voice.dopplerLevel = 0;
            voice.volume = volume;
            voice.clip = Resources.Load<AudioClip>("Audio/LevelComplete");
            if (!voice.clip) Debug.LogWarning("Level completion sound: Audio/LevelComplete is missing.", this);
            else if (voice.clip.loadState == AudioDataLoadState.Unloaded) voice.clip.LoadAudioData();
        }

        private void OnEnable() { puzzle.CompletionShown += Play; }
        private void OnDisable() { puzzle.CompletionShown -= Play; }
        private void Play()
        {
            if (played || !puzzle.Completed || !voice.clip) return;
            played = true;
            // Audio playback continues while the completion menu sets timeScale to zero.
            voice.Play();
        }
        private void OnDestroy() { if (voice) Destroy(voice.gameObject); }
    }
}
