using UnityEngine;
namespace Paradox
{
    // Put on shared architecture to swap ordinary materials as the atmosphere changes.
    public sealed class DimensionTint : MonoBehaviour
    {
        [SerializeField] private DimensionManager manager;
        [SerializeField] private Material white;
        [SerializeField] private Material black;
        [SerializeField] private Renderer surface;
        public void Configure(DimensionManager owner, Material pale, Material dark)
        { manager = owner; white = pale; black = dark; surface = GetComponent<Renderer>(); }
        private void OnEnable() { if (manager) { manager.Changed += Apply; Apply(manager.Current); } }
        private void OnDisable() { if (manager) manager.Changed -= Apply; }
        private void Start() { if (!manager || !surface || !white || !black) Debug.LogWarning("DimensionTint needs manager, renderer and both materials.", this); }
        private void Apply(Dimension value) { if (surface) surface.sharedMaterial = value == Dimension.White ? white : black; }
    }
}
