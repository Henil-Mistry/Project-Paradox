using UnityEngine;
using UnityEngine.SceneManagement;
namespace Paradox
{
    // Put on Systems and wire the Restart button here. A scene reload resets all state.
    public sealed class GameFlow : MonoBehaviour
    {
        private bool restarting;
        private void Update() { if (PlayerInput.Restart) Restart(); }
        public void Restart()
        {
            if (restarting) return;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0) { Debug.LogWarning("Add Level01 to the build scene list before restarting.", this); return; }
            restarting = true;
            FirstPersonController.LockCursor(true);
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
