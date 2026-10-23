using UnityEngine;
namespace Paradox
{
    // Released objects in the configured dimension hold the plate, even while that world is hidden.
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PressurePlate : MonoBehaviour
    {
        [SerializeField] private DimensionManager manager;
        [SerializeField] private PuzzleManager puzzle;
        [SerializeField] private Transform placement;
        [SerializeField] private Renderer indicator;
        [SerializeField] private Material readyMaterial;
        private BoxCollider zone;
        private Material idleMaterial;
        [SerializeField] private Dimension dimension = Dimension.White;
        public Dimension Dimension => dimension;
        public void SetDimension(Dimension value) { dimension = value; GetComponent<DimensionObject>().SetMembership(value); }
        public bool IsAvailable => manager && manager.Current == dimension;
        public bool IsSatisfied { get; private set; }
        public Vector3 Placement => placement ? placement.position : transform.position + Vector3.up * 0.7f;
        public void Configure(DimensionManager owner, PuzzleManager state, Transform cubeSpot, Renderer lamp, Material lit)
        { manager = owner; puzzle = state; placement = cubeSpot; indicator = lamp; readyMaterial = lit; }
        private void Awake() { zone = GetComponent<BoxCollider>(); if (indicator) idleMaterial = indicator.sharedMaterial; }
        private void Start() { if (!manager || !puzzle || !placement) Debug.LogWarning("PressurePlate needs manager, puzzle and placement references.", this); }
        private void FixedUpdate() { Evaluate(); }
        public void Evaluate()
        {
            IsSatisfied = false;
            if (zone)
                foreach (TransferableObject cube in Object.FindObjectsByType<TransferableObject>(FindObjectsSortMode.None))
                {
                    if (cube.IsHeld || cube.Dimension != dimension) continue;
                    foreach (Collider shape in cube.GetComponentsInChildren<Collider>())
                    {
                        if (shape.isTrigger) continue;
                        bool zoneEnabled = zone.enabled, shapeEnabled = shape.enabled;
                        try
                        {
                            zone.enabled = shape.enabled = true;
                            if (Physics.ComputePenetration(zone, zone.transform.position, zone.transform.rotation,
                                shape, shape.transform.position, shape.transform.rotation, out _, out float depth) && depth > 0)
                            { IsSatisfied = true; break; }
                        }
                        finally { zone.enabled = zoneEnabled; shape.enabled = shapeEnabled; }
                    }
                    if (IsSatisfied) break;
                }
            if (indicator) indicator.sharedMaterial = IsSatisfied && readyMaterial ? readyMaterial : idleMaterial;
            if (puzzle)
            {
                if (IsSatisfied) puzzle.TryOpenDoor(this);
                else puzzle.CloseDoor(this);
            }
        }
    }
}
