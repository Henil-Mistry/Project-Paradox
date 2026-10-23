using UnityEngine;
namespace Paradox
{
    // Put on Systems. These latched flags survive dimension changes and falls.
    public sealed class PuzzleManager : MonoBehaviour
    {
        [SerializeField] private Door door;
        [SerializeField] private PressurePlate plate;
        [SerializeField] private HUD hud;
        public bool DoorOpened { get; private set; }
        public bool Completed { get; private set; }
        public void Configure(Door exitDoor, PressurePlate requiredPlate, HUD display) { door = exitDoor; plate = requiredPlate; hud = display; }
        private void Start() { if (!door || !plate || !hud) Debug.LogWarning("PuzzleManager needs door, plate and HUD.", this); }
        public bool TryOpenDoor(PressurePlate source)
        {
            if (DoorOpened || !source || source != plate || !source.IsAvailable || !source.IsSatisfied) return false;
            DoorOpened = true;
            if (door) door.Open();
            if (hud) hud.Message("Door unlocked");
            return true;
        }
        public bool Complete()
        {
            if (!DoorOpened || Completed) return false;
            Completed = true;
            FirstPersonController.LockCursor(false);
            if (hud) hud.ShowCompletion();
            return true;
        }
    }
}
