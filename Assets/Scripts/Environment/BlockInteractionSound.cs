using UnityEngine;

namespace Paradox
{
    // Subscribes only to successful pickup/release, not input, recovery or transfer.
    [DisallowMultipleComponent, RequireComponent(typeof(TransferableObject))]
    public sealed class BlockInteractionSound : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float volume = 0.35f;
        private TransferableObject block;
        private AudioSource voice;
        private void Awake()
        {
            block = GetComponent<TransferableObject>();
            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0;
            voice.volume = volume;
            voice.clip = Resources.Load<AudioClip>("Audio/BlockInteraction");
            if (!voice.clip) Debug.LogWarning("Block interaction sound: Audio/BlockInteraction is missing.", this);
            else if (voice.clip.loadState == AudioDataLoadState.Unloaded) voice.clip.LoadAudioData();
        }
        private void OnEnable() { block.Interacted += Play; }
        private void OnDisable() { block.Interacted -= Play; voice.Stop(); }
        private void Play()
        {
            if (!voice.clip) return;
            // Reuse one voice when pickup/drop occurs faster than this clip's duration.
            voice.Stop();
            voice.Play();
        }
        private void OnDestroy() { if (voice) Destroy(voice); }
    }
}
