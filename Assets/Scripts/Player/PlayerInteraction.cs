using UnityEngine;
namespace Paradox
{
    // Put on Player. One E action per press; nearby devices take priority while carrying.
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Camera view;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private HUD hud;
        [SerializeField] private PuzzleManager puzzle;
        [SerializeField] private WarpPad[] pads;
        [SerializeField] private PressurePlate plate;
        [SerializeField, Min(1)] private float reach = 2.7f;
        [SerializeField] private float cooldown = 0.2f;
        private float nextAction;
        private readonly Collider[] clearance = new Collider[24];
        public TransferableObject HeldObject { get; private set; }
        public void Configure(Camera camera, Transform grip, HUD display, PuzzleManager state, WarpPad[] devices, PressurePlate targetPlate)
        { view = camera; holdPoint = grip; hud = display; puzzle = state; pads = devices; plate = targetPlate; }
        private void Start() { if (!view || !holdPoint || !hud || !puzzle || !plate || pads == null) Debug.LogWarning("Assign PlayerInteraction scene references.", this); }
        private bool Near(Vector3 point)
        {
            Vector3 difference = point - transform.position;
            return new Vector2(difference.x, difference.z).sqrMagnitude < reach * reach && Mathf.Abs(difference.y) < 2.5f;
        }
        private bool ClearPath(Vector3 target, Transform device)
        {
            Vector3 delta = target - view.transform.position;
            if (!Physics.Raycast(view.transform.position, delta.normalized, out RaycastHit hit, delta.magnitude, ~0, QueryTriggerInteraction.Ignore)) return true;
            return hit.transform == device || hit.transform.IsChildOf(device);
        }
        private void Update()
        {
            if (!puzzle || puzzle.Completed || !view || !hud) return;
            if (Cursor.lockState != CursorLockMode.Locked) { hud.Prompt("Click to resume • R restart"); return; }
            ProcessInteraction(PlayerInput.Interact);
        }
        // Shared by the E key and integration tests; targeting and cooldown stay identical.
        public void ProcessInteraction(bool pressed)
        {
            if (!puzzle || puzzle.Completed || !view || !hud) return;
            WarpPad pad = null;
            if (HeldObject && pads != null)
                foreach (WarpPad candidate in pads)
                    if (candidate && candidate.IsAvailable && Near(candidate.transform.position) && ClearPath(candidate.Arrival, candidate.transform)) { pad = candidate; break; }
            bool canPlace = HeldObject && plate && plate.IsAvailable && Near(plate.transform.position) && ClearPath(plate.Placement, plate.transform);
            TransferableObject pickup = null;
            if (!HeldObject && Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, reach, ~0, QueryTriggerInteraction.Ignore))
            { pickup = hit.collider.GetComponentInParent<TransferableObject>(); if (pickup && !pickup.IsAvailable) pickup = null; }
            hud.Prompt(HeldObject ? (pad ? "E — Transfer object" : canPlace ? "E — Place on plate" : "E — Drop") : pickup ? "E — Pick up" : "");
            if (!pressed || Time.time < nextAction) return;
            nextAction = Time.time + cooldown;
            if (HeldObject)
            {
                if (pad)
                {
                    if (pad.Transfer(HeldObject)) { HeldObject = null; hud.Message("Object transferred"); hud.Flash(new Color(0.5f, 0.4f, 1)); }
                }
                else if (canPlace) DropAt(plate.Placement);
                else
                {
                    Vector3 drop = transform.position + transform.forward * 1.35f + Vector3.up * 0.8f;
                    if (DropIsClear(drop)) DropAt(drop); else hud.Message("There is no room to release the object");
                }
            }
            else if (pickup && pickup.PickUp(holdPoint)) HeldObject = pickup;
        }
        private bool DropIsClear(Vector3 position)
        {
            int count = Physics.OverlapBoxNonAlloc(position, Vector3.one * 0.41f, clearance, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
            if (count == clearance.Length) return false;
            for (int i = 0; i < count; i++)
                if (!clearance[i].transform.IsChildOf(transform) && clearance[i].transform != transform) return false;
            // A clear endpoint alone would allow dropping through thin walls.
            Vector3 delta = position - view.transform.position;
            return !Physics.SphereCast(view.transform.position, 0.4f, delta.normalized, out _, delta.magnitude, ~0, QueryTriggerInteraction.Ignore);
        }
        private void DropAt(Vector3 position) { HeldObject.Release(position); HeldObject = null; }
        public void RecoverHeldObject() { if (HeldObject) { HeldObject.Recover(); HeldObject = null; } }
        // Also used by automated play checks, through the same pickup operation as E.
        public bool TryPickUp(TransferableObject item)
        { if (HeldObject || !item || !item.PickUp(holdPoint)) return false; HeldObject = item; return true; }
        public bool TryTransfer(WarpPad pad)
        { if (!HeldObject || !pad || !pad.Transfer(HeldObject)) return false; HeldObject = null; return true; }
        public void PlaceHeld(Vector3 position) { if (HeldObject) DropAt(position); }
    }
}
