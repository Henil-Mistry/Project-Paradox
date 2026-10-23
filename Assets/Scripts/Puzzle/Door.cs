using UnityEngine;
namespace Paradox
{
    // Put on the moving door slab. The collider moves with the visible slab.
    public sealed class Door : MonoBehaviour
    {
        [SerializeField] private Vector3 openOffset = new Vector3(0, 4.5f, 0);
        [SerializeField, Min(0.1f)] private float duration = 1f;
        private Vector3 closedPosition;
        private float progress;
        public bool IsOpen { get; private set; }
        private void Awake() { closedPosition = transform.localPosition; }
        public void Open() { IsOpen = true; }
        public void Close() { IsOpen = false; }
        private void Update()
        {
            progress = Mathf.MoveTowards(progress, IsOpen ? 1 : 0, Time.deltaTime / duration);
            transform.localPosition = closedPosition + openOffset * Mathf.SmoothStep(0, 1, progress);
        }
    }
}
