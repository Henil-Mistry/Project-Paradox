using UnityEngine;
namespace Paradox
{
    // Put on Systems. Door state follows the plate; completion remains latched.
    public sealed class PuzzleManager : MonoBehaviour
    {
        [SerializeField] private Door door;
        [SerializeField] private PressurePlate plate;
        [SerializeField] private HUD hud;
        private DimensionManager dimensions;
        private Dimension gateDimension;
        public void ConfigureGate(DimensionManager worlds, Dimension gate) { dimensions = worlds; gateDimension = gate; }
        public bool DoorOpened { get; private set; }
        public bool Completed { get; private set; }
        public event System.Action CompletionShown;
        public void Configure(Door exitDoor, PressurePlate requiredPlate, HUD display) { door = exitDoor; plate = requiredPlate; hud = display; }
        private void Start() { if (!GetComponent<LevelCompletionSound>()) gameObject.AddComponent<LevelCompletionSound>(); if (!door || !plate || !hud) Debug.LogWarning("PuzzleManager needs door, plate and HUD.", this); }
        public bool TryOpenDoor(PressurePlate source)
        {
            if (DoorOpened || !source || source != plate || !source.IsSatisfied) return false;
            DoorOpened = true;
            if (door) door.Open();
            if (hud) hud.Message("Door unlocked");
            return true;
        }
        public void CloseDoor(PressurePlate source)
        {
            if (!source || source != plate || source.IsSatisfied) return;
            DoorOpened = false;
            if (door) door.Close();
        }
        public bool Complete()
        {
            if (!DoorOpened || Completed || (dimensions && dimensions.Current != gateDimension)) return false;
            Completed = true;
            FirstPersonController.LockCursor(false);
            if (hud) hud.ShowCompletion();
            CompletionShown?.Invoke();
            return true;
        }
    }
}
