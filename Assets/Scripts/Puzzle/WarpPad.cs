using UnityEngine;
namespace Paradox
{
    // Put on each dimension's pad. The linked pad occupies the same physical location.
    public sealed class WarpPad : MonoBehaviour
    {
        [SerializeField] private DimensionManager manager;
        [SerializeField] private Dimension dimension;
        [SerializeField] private WarpPad linkedPad;
        [SerializeField] private Transform arrival;
        [SerializeField] private ParticleSystem burst;
        public Dimension Dimension => dimension;
        public Vector3 Arrival => arrival ? arrival.position : transform.position + Vector3.up;
        public bool IsAvailable => manager && manager.Current == dimension && linkedPad && linkedPad.dimension != dimension;
        public void Configure(DimensionManager owner, Dimension home, WarpPad linked, Transform landingPoint, ParticleSystem effect)
        { manager = owner; dimension = home; linkedPad = linked; arrival = landingPoint; burst = effect; }
        public bool Transfer(TransferableObject item)
        {
            if (!IsAvailable || !item || !item.IsHeld || item.Dimension != dimension) return false;
            item.MoveToDimension(linkedPad.dimension, linkedPad.Arrival);
            if (burst) burst.Play();
            if (linkedPad.burst) linkedPad.burst.Play();
            return true;
        }
    }
}
