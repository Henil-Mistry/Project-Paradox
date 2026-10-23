using UnityEngine;
namespace Paradox
{
    // Put on the White plate's trigger volume. Only a released White Cube can satisfy it.
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PressurePlate : MonoBehaviour
    {
        [SerializeField] private DimensionManager manager;
        [SerializeField] private PuzzleManager puzzle;
        [SerializeField] private Transform placement;
        [SerializeField] private Renderer indicator;
        [SerializeField] private Material readyMaterial;
        private BoxCollider zone;
        private readonly Collider[] contacts = new Collider[16];
        public bool IsAvailable => manager && manager.Current == Dimension.White;
        public bool IsSatisfied { get; private set; }
        public Vector3 Placement => placement ? placement.position : transform.position + Vector3.up * 0.7f;
        public void Configure(DimensionManager owner, PuzzleManager state, Transform cubeSpot, Renderer lamp, Material lit)
        { manager = owner; puzzle = state; placement = cubeSpot; indicator = lamp; readyMaterial = lit; }
        private void Awake() { zone = GetComponent<BoxCollider>(); }
        private void Start() { if (!manager || !puzzle || !placement) Debug.LogWarning("PressurePlate needs manager, puzzle and placement references.", this); }
        private void FixedUpdate() { Evaluate(); }
        public void Evaluate()
        {
            IsSatisfied = false;
            if (!IsAvailable || !zone || !zone.enabled) return;
            // Query the physical volume so a dimension switch cannot leave stale trigger contacts.
            int count = Physics.OverlapBoxNonAlloc(transform.TransformPoint(zone.center), Vector3.Scale(zone.size * 0.5f, transform.lossyScale), contacts, transform.rotation, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                TransferableObject cube = contacts[i].GetComponentInParent<TransferableObject>();
                if (cube && !cube.IsHeld && cube.Dimension == Dimension.White)
                {
                    IsSatisfied = true;
                    if (indicator && readyMaterial) indicator.sharedMaterial = readyMaterial;
                    if (puzzle) puzzle.TryOpenDoor(this);
                    return;
                }
            }
        }
    }
}
