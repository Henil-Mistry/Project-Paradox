using UnityEngine;

namespace Paradox
{
    /// <summary>Listens to committed dimension changes, never input or transition animation.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DimensionManager))]
    public sealed class DimensionSwitchSound : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float volume = 0.35f;
        private DimensionManager manager;
        private AudioSource voice;

        private void Awake()
        {
            manager = GetComponent<DimensionManager>();
            // Own voice: never reuse or stop background music / unrelated effects.
            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0;
            voice.dopplerLevel = 0;
            voice.volume = volume;
            voice.clip = Resources.Load<AudioClip>("Audio/DimensionSwitch");
            // Decode before any switch request, not on the first successful commit.
            // The importer also preloads this short effect; this covers unloaded data on re-entry.
            if (voice.clip && voice.clip.loadState == AudioDataLoadState.Unloaded)
                voice.clip.LoadAudioData();
            if (!voice.clip) Debug.LogWarning("Dimension switch sound: Audio/DimensionSwitch is missing.", this);
        }

        private void OnEnable() { manager.Changed += Play; }
        private void OnDisable()
        {
            manager.Changed -= Play;
            voice.Stop();
        }

        private void Play(Dimension dimension)
        {
            if (!voice.clip) return;
            // A new successful switch replaces the tail, rather than stacking one-shots.
            voice.Stop();
            voice.Play();
        }

        private void OnDestroy() { if (voice) Destroy(voice); }
    }
}
