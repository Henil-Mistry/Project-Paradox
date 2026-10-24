using UnityEngine;
using UnityEngine.SceneManagement;

namespace Paradox
{
    /// <summary>One non-spatial music voice shared by the menu and all five levels.</summary>
    public sealed class JourneyMusic : MonoBehaviour
    {
        private static JourneyMusic instance;
        [SerializeField, Range(0, 1)] private float volume = 0.25f;
        [SerializeField, Min(0.1f)] private float fadeSeconds = 1.2f;
        private AudioSource source;
        private bool wanted;
        private float gain;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { instance = null; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (instance) return;
            new GameObject("Journey music").AddComponent<JourneyMusic>();
        }

        private void Awake()
        {
            if (instance && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0;
            source.dopplerLevel = 0;
            source.volume = 0;
            source.clip = Resources.Load<AudioClip>("Music/TillLevel3");
            if (!source.clip) Debug.LogWarning("Journey music: Music/TillLevel3 audio clip is missing.", this);
            SceneManager.activeSceneChanged += SceneChanged;
            SceneChanged(default, SceneManager.GetActiveScene());
        }

        private void SceneChanged(Scene previous, Scene current)
        {
            wanted = current.name == "MainMenu" || current.name == "Level01" ||
                     current.name == "Level02_Library" || current.name == "Level03_MirrorLake" ||
                     current.name == "Level04_Observatory" || current.name == "Level05_Heart";
            // Neither scene reloads nor dimension switches restart an already playing voice.
            if (wanted && source.clip && !source.isPlaying) source.Play();
        }

        private void Update()
        {
            gain = Mathf.MoveTowards(gain, wanted ? 1 : 0, Time.unscaledDeltaTime / fadeSeconds);
            source.volume = volume * Mathf.SmoothStep(0, 1, gain);
            if (!wanted && gain == 0 && source.isPlaying) source.Stop();
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneChanged;
            if (instance == this) instance = null;
        }
    }
}
