using System.Collections.Generic;
using UnityEngine;
namespace Paradox
{
    // Non-combat device: ray target, validate the corresponding position, then reuse cube transfer.
    public sealed class TransferGun : MonoBehaviour
    {
        [SerializeField] private Camera view;
        [SerializeField] private TransferableObject designatedObject;
        [SerializeField] private PlayerInteraction hands;
        [SerializeField] private PuzzleManager puzzle;
        [SerializeField] private DimensionManager dimensions;
        [SerializeField] private HUD hud;
        [SerializeField] private GameObject model;
        [SerializeField] private float range = 12;
        private float nextShot;
        private bool hadFocus;
        public bool Equipped { get; private set; }
        public void Configure(Camera camera, TransferableObject item, PlayerInteraction interaction, PuzzleManager state,
            DimensionManager worlds, HUD display, GameObject visual)
        { view = camera; designatedObject = item; hands = interaction; puzzle = state; dimensions = worlds; hud = display; model = visual; }
        public void ToggleEquipped()
        {
            if (puzzle.Completed || hands.HeldObject) return;
            Equipped = !Equipped;
            if (model) model.SetActive(Equipped);
            hud.Message(Equipped ? "Transfer gun equipped — aim at the cube and left-click" : "Transfer gun holstered");
        }
        private void Update()
        {
            bool focused = Cursor.lockState == CursorLockMode.Locked;
            if (model) model.SetActive(Equipped && !hands.HeldObject && !puzzle.Completed);
            if (focused && hadFocus && PlayerInput.Click) TryFire();
            hadFocus = focused;
        }
        public bool TryFire()
        {
            if (!Equipped || puzzle.Completed || Time.time < nextShot) return false;
            nextShot = Time.time + 0.3f;
            if (hands.HeldObject) return Fail("Release the cube before transferring");
            if (!Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore)
                || hit.collider.GetComponentInParent<TransferableObject>() != designatedObject || !designatedObject.IsAvailable)
                return Fail("Aim at the puzzle cube");
            Dimension target = dimensions.Current == Dimension.White ? Dimension.Black : Dimension.White;
            if (!DestinationIsSafe(target)) return Fail("Transfer blocked — place the cube on clear, shared ground");
            designatedObject.MoveToDimension(target, designatedObject.transform.position);
            hud.Message("Cube transferred to " + target + " — Q switches your dimension");
            hud.Flash(new Color(0.25f, 0.9f, 1));
            return true;
        }
        private bool Fail(string message) { hud.Message(message); hud.Flash(new Color(1, 0.3f, 0.2f)); return false; }
        private bool DestinationIsSafe(Dimension target)
        {
            var shape = designatedObject.GetComponent<BoxCollider>();
            Vector3 position = designatedObject.transform.position;
            var supports = new List<Collider>();
            // Inspect hidden destination colliders too; restore every enabled flag before returning.
            foreach (Collider obstacle in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if (obstacle.isTrigger || obstacle.transform.IsChildOf(designatedObject.transform)) continue;
                DimensionObject membership = obstacle.GetComponentInParent<DimensionObject>();
                if (membership ? !membership.ExistsIn(target) : !obstacle.enabled) continue;
                bool enabled = obstacle.enabled;
                try
                {
                    obstacle.enabled = true;
                    if (Physics.ComputePenetration(shape, position, designatedObject.transform.rotation,
                        obstacle, obstacle.transform.position, obstacle.transform.rotation, out _, out float depth) && depth > 0.015f) return false;
                    supports.Add(obstacle);
                }
                finally { obstacle.enabled = enabled; }
            }
            // All four corners must have nearby support. In particular, White has no chasm bridge.
            foreach (float x in new[] { -0.3f, 0.3f }) foreach (float z in new[] { -0.3f, 0.3f })
            {
                bool supported = false;
                foreach (Collider floor in supports)
                {
                    bool enabled = floor.enabled;
                    try
                    {
                        floor.enabled = true;
                        if (floor.Raycast(new Ray(position + new Vector3(x, 0, z), Vector3.down), out RaycastHit hit, 1.2f)
                            && hit.normal.y > 0.7f) { supported = true; break; }
                    }
                    finally { floor.enabled = enabled; }
                }
                if (!supported) return false;
            }
            return true;
        }
    }
}
