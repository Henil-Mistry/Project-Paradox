using UnityEngine;
namespace Paradox
{
    // Put on the kill trigger below the chasm; coordinates also provide a fallback.
    public sealed class RespawnZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            FirstPersonController player = other.GetComponentInParent<FirstPersonController>();
            if (player) { player.Respawn(); return; }
            TransferableObject cube = other.GetComponentInParent<TransferableObject>();
            if (cube && !cube.IsHeld) cube.Recover();
        }
    }
}
