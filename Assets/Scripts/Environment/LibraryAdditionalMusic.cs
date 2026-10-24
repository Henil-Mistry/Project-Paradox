using UnityEngine;

namespace Paradox
{
    // Scene-owned additional layer. Never touches the shared music or dimension events.
    [DisallowMultipleComponent]
    public sealed class LibraryAdditionalMusic : MonoBehaviour
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0, 1)] private float volume = 0.12f;
        private AudioSource voice;
        private void Awake()
        {
            if (!clip) { Debug.LogWarning("Library additional music clip is missing.", this); return; }
            var child = new GameObject("Library additional music");
            child.transform.SetParent(transform, false);
            voice = child.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.clip = clip;
            voice.volume = volume;
            voice.spatialBlend = 0;
            voice.dopplerLevel = 0;
            voice.loop = true;
            voice.Play();
        }
        private void OnDestroy() { if (voice) Destroy(voice.gameObject); }
    }
}
