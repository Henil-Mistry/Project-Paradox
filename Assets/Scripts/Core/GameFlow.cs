using UnityEngine;
using UnityEngine.SceneManagement;
namespace Paradox
{
    // Put on Systems and wire the Restart button here. A scene reload resets all state.
    public sealed class GameFlow : MonoBehaviour
    {
        [SerializeField] private string nextScene;
        [SerializeField] private PuzzleManager puzzle;
        private bool restarting;
        public void ConfigureProgression(string next, PuzzleManager state) { nextScene = next; puzzle = state; }
        public void NextLevel()
        {
            if (restarting || !puzzle || !puzzle.Completed || string.IsNullOrEmpty(nextScene)) return;
            if (!Application.CanStreamedLevelBeLoaded(nextScene))
            { Debug.LogWarning("Next level is missing from the build scene list: " + nextScene, this); return; }
            restarting = true;
            FirstPersonController.LockCursor(true);
            SceneManager.LoadScene(nextScene);
        }
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
