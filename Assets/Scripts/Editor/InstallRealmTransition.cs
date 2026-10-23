using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Paradox.Editor {
public static class InstallRealmTransition {
public static void Apply(){var scene=EditorSceneManager.OpenScene("Assets/Scenes/Level01.unity");
var manager=Object.FindFirstObjectByType<DimensionManager>();var transition=manager.GetComponent<RealmTransition>();if(!transition)transition=manager.gameObject.AddComponent<RealmTransition>();transition.Configure(manager);EditorUtility.SetDirty(transition);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);}
}}
