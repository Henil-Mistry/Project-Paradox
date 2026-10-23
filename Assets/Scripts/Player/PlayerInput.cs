using UnityEngine;
namespace Paradox
{
    // Legacy Input Manager adapter for this Built-in Unity 6 project.
    public static class PlayerInput
    {
        public static Vector2 Move => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        public static Vector2 Look => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        public static bool Jump => Input.GetKeyDown(KeyCode.Space);
        public static bool Interact => Input.GetKeyDown(KeyCode.E);
        public static bool Switch => Input.GetKeyDown(KeyCode.Q);
        public static bool Restart => Input.GetKeyDown(KeyCode.R);
        public static bool Escape => Input.GetKeyDown(KeyCode.Escape);
        public static bool Click => Input.GetMouseButtonDown(0);
    }
}
