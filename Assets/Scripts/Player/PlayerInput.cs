using UnityEngine;
namespace Paradox
{
    // Legacy Input Manager adapter for this Built-in Unity 6 project.
    public static class PlayerInput
    {
        public static Vector2 Move => LevelMenus.InputBlocked ? Vector2.zero : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        public static Vector2 Look => LevelMenus.InputBlocked ? Vector2.zero : new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        public static bool Jump => !LevelMenus.InputBlocked && Input.GetKeyDown(KeyCode.Space);
        public static bool Interact => !LevelMenus.InputBlocked && Input.GetKeyDown(KeyCode.E);
        public static bool Switch => !LevelMenus.InputBlocked && Input.GetKeyDown(KeyCode.Q);
        public static bool Restart => !LevelMenus.InputBlocked && Input.GetKeyDown(KeyCode.R);
        public static bool Escape => !LevelMenus.Active && Input.GetKeyDown(KeyCode.Escape);
        public static bool Click => !LevelMenus.InputBlocked && Input.GetMouseButtonDown(0);
    }
}
