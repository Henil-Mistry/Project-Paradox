using UnityEngine;
namespace Paradox
{
    // Put on the Cube with a Rigidbody, BoxCollider and DimensionObject.
    [RequireComponent(typeof(Rigidbody), typeof(DimensionObject))]
    public sealed class TransferableObject : MonoBehaviour
    {
        [SerializeField] private DimensionManager manager;
        [SerializeField] private DimensionObject dimensionObject;
        [SerializeField] private Dimension dimension = Dimension.Black;
        [SerializeField] private Transform blackHome;
        [SerializeField] private Transform whiteHome;
        [SerializeField] private ParticleSystem recoveryEffect;
        private Rigidbody body;
        private RigidbodyInterpolation releasedInterpolation;
        public Dimension Dimension => dimension;
        public bool IsHeld { get; private set; }
        public bool IsAvailable => manager && dimension == manager.Current && !IsHeld;
        public void Configure(DimensionManager owner, DimensionObject visibility, Transform blackSpawn, Transform whiteSpawn, ParticleSystem effect)
        { manager = owner; dimensionObject = visibility; blackHome = blackSpawn; whiteHome = whiteSpawn; recoveryEffect = effect; }
        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            releasedInterpolation = body.interpolation;
        }
        private void Start()
        {
            if (!manager || !dimensionObject || !blackHome || !whiteHome) Debug.LogWarning("Cube needs dimension references and both recovery homes.", this);
            if (dimensionObject) dimensionObject.SetMembership(dimension);
        }
        private void Update()
        {
            if (!IsHeld && (transform.position.y < -6 || Mathf.Abs(transform.position.x) > 24 || transform.position.z < -32 || transform.position.z > 30)) Recover();
        }
        public bool PickUp(Transform holdPoint)
        {
            if (!IsAvailable || !holdPoint) return false;
            // The camera now owns the pose each rendered frame. Physics interpolation
            // would overwrite it with an older world pose and make the Cube trail behind.
            body.interpolation = RigidbodyInterpolation.None;
            IsHeld = true; dimensionObject.SetHeld(true);
            transform.SetParent(holdPoint, false); transform.localPosition = Vector3.zero; transform.localRotation = Quaternion.identity;
            return true;
        }
        public void Release(Vector3 position)
        {
            body.interpolation = RigidbodyInterpolation.None;
            transform.SetParent(null, true); transform.SetPositionAndRotation(position, Quaternion.identity);
            ResetPhysicsPose(position);
            IsHeld = false; dimensionObject.SetHeld(false);
            ClearMotion(); Physics.SyncTransforms();
            body.interpolation = releasedInterpolation;
        }
        public void MoveToDimension(Dimension target, Vector3 position)
        {
            // Keep physics suspended until the final membership and position are set.
            body.interpolation = RigidbodyInterpolation.None;
            dimensionObject.SetHeld(true);
            transform.SetParent(null, true); transform.SetPositionAndRotation(position, Quaternion.identity);
            ResetPhysicsPose(position);
            IsHeld = false; dimension = target;
            dimensionObject.SetMembership(target); dimensionObject.SetHeld(false);
            ClearMotion(); Physics.SyncTransforms();
            body.interpolation = releasedInterpolation;
        }
        public void Recover()
        {
            Transform home = dimension == Dimension.White ? whiteHome : blackHome;
            if (!home) { Debug.LogWarning("Cube recovery home is missing.", this); return; }
            MoveToDimension(dimension, home.position);
            if (recoveryEffect && manager && dimension == manager.Current)
            { recoveryEffect.transform.position = home.position; recoveryEffect.Play(); }
        }
        private void ResetPhysicsPose(Vector3 position)
        {
            // Seed physics at the release/arrival location before interpolation resumes.
            body.position = position;
            body.rotation = Quaternion.identity;
        }
        private void ClearMotion()
        {
            if (!body) body = GetComponent<Rigidbody>();
            if (!body.isKinematic) { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
        }
    }
}
