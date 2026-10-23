using UnityEngine;
namespace Paradox
{
    // Decorative objects only: never put this on a collider or a puzzle mechanism.
    // Each brief disturbance is deterministic; completion restores the exact authored pose.
    public sealed class VisualGlitch : MonoBehaviour
    {
        [SerializeField] private PuzzleManager puzzle;
        [SerializeField] private float interval = 4;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private float offset = 0.1f;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private bool flicker;
        private Vector3 home;
        private Quaternion rotation;
        private Renderer surface;
        public bool IsCalm { get; private set; }
        public void Configure(PuzzleManager state, float frequency, float length, float distance, float spin, bool blink)
        { puzzle = state; interval = frequency; duration = length; offset = distance; rotationSpeed = spin; flicker = blink; }
        private void Awake() { home = transform.localPosition; rotation = transform.localRotation; surface = GetComponent<Renderer>(); }
        private void Update()
        {
            if (puzzle && puzzle.Completed)
            {
                transform.localPosition = home; transform.localRotation = rotation;
                if (surface) surface.enabled = true;
                IsCalm = true; return;
            }
            float time = Time.timeSinceLevelLoad;
            bool pulse = time % Mathf.Max(0.5f, interval) < duration;
            float stepTime = pulse ? Mathf.Floor(time * 5) / 5 : time;
            transform.localPosition = home + (pulse ? new Vector3(Mathf.Sin(stepTime * 37), Mathf.Cos(stepTime * 23), 0) * offset : Vector3.zero);
            transform.localRotation = rotation * Quaternion.Euler(0, 0, stepTime * rotationSpeed);
            if (surface) surface.enabled = !flicker || !pulse || ((int)(time * 20) % 2 == 0);
        }
    }
}
