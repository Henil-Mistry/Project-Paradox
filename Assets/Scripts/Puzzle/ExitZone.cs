using UnityEngine;
namespace Paradox
{
    // Put on the trigger behind the door, within the enclosed exit corridor.
    public sealed class ExitZone : MonoBehaviour
    {
        [SerializeField] private PuzzleManager puzzle;
        public void Configure(PuzzleManager state) { puzzle = state; }
        private void Start() { if (!puzzle) Debug.LogWarning("ExitZone needs the puzzle reference.", this); }
        private void OnTriggerEnter(Collider other) { if (puzzle && other.GetComponentInParent<FirstPersonController>()) puzzle.Complete(); }
    }
}
