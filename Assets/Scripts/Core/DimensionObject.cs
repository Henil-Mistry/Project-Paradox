using UnityEngine;
namespace Paradox
{
    // Put on the root of dimension-specific geometry. Scripts stay alive when hidden.
    public sealed class DimensionObject : MonoBehaviour
    {
        [SerializeField] private DimensionManager manager;
        [SerializeField] private DimensionMembership membership = DimensionMembership.Both;
        private Renderer[] visuals;
        private Collider[] shapes;
        private Rigidbody body;
        private bool suppressed;
        public bool IsPresent => !manager || ExistsIn(manager.Current);
        public DimensionMembership Membership => membership;
        public void Configure(DimensionManager owner, DimensionMembership value)
        {
            Unsubscribe(); manager = owner; membership = value; Cache(); Subscribe(); Apply(manager ? manager.Current : Dimension.White);
        }
        private void Cache()
        {
            if (visuals != null) return;
            visuals = GetComponentsInChildren<Renderer>(true);
            shapes = GetComponentsInChildren<Collider>(true);
            body = GetComponent<Rigidbody>();
        }
        private void OnEnable() { Cache(); Subscribe(); Apply(manager ? manager.Current : Dimension.White); }
        private void Start() { if (!manager) Debug.LogWarning("DimensionObject needs a DimensionManager.", this); }
        private void OnDisable() { Unsubscribe(); }
        private void Subscribe() { if (manager) { manager.Changed -= Apply; manager.Changed += Apply; manager.Register(this); } }
        private void Unsubscribe() { if (manager) { manager.Changed -= Apply; manager.Unregister(this); } }
        public bool ExistsIn(Dimension value) => membership == DimensionMembership.Both || (int)membership == (int)value;
        public void SetMembership(Dimension value) { membership = (DimensionMembership)value; Apply(manager ? manager.Current : value); }
        public void SetHeld(bool held) { suppressed = held; Apply(manager ? manager.Current : Dimension.White); }
        private void Apply(Dimension value)
        {
            Cache(); bool present = ExistsIn(value);
            for (int i = 0; i < visuals.Length; i++) if (visuals[i]) visuals[i].enabled = present;
            for (int i = 0; i < shapes.Length; i++) if (shapes[i]) shapes[i].enabled = present && !suppressed;
            if (body)
            {
                bool freeze = !present || suppressed;
                if (freeze && !body.isKinematic) { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
                body.isKinematic = freeze;
                body.useGravity = !freeze;
            }
        }
        public bool Overlaps(CapsuleCollider probe)
        {
            if (!probe) return false;
            Cache();
            for (int i = 0; i < shapes.Length; i++)
            {
                Collider shape = shapes[i];
                if (!shape || shape.isTrigger) continue;
                // Unity may not have created a native shape for a collider saved disabled.
                // Enable only during this synchronous query; no physics step occurs here.
                bool probeEnabled = probe.enabled;
                bool shapeEnabled = shape.enabled;
                bool overlaps;
                try
                {
                    probe.enabled = true;
                    shape.enabled = true;
                    overlaps = Physics.ComputePenetration(probe, probe.transform.position, probe.transform.rotation,
                        shape, shape.transform.position, shape.transform.rotation, out _, out float depth) && depth > 0.025f;
                }
                finally { probe.enabled = probeEnabled; shape.enabled = shapeEnabled; }
                if (overlaps) return true;
            }
            return false;
        }
    }
}
