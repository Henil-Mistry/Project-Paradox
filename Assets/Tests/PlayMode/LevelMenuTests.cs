using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
namespace Paradox.Tests
{
 public sealed class LevelMenuTests
 {
  static T One<T>() where T:Object=>Object.FindFirstObjectByType<T>();
  static void Capture(string name){
   var camera=One<FirstPersonController>().GetComponentInChildren<Camera>();var canvas=One<HUD>().GetComponent<Canvas>();
   var rt=new RenderTexture(1600,900,24);camera.targetTexture=rt;var old=RenderTexture.active;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.3f;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
   var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();System.IO.File.WriteAllBytes("/tmp/paradox-feature-previews/"+name+".png",texture.EncodeToPNG());
   camera.targetTexture=null;RenderTexture.active=old;canvas.renderMode=RenderMode.ScreenSpaceOverlay;Object.Destroy(texture);Object.Destroy(rt);
  }
  static void Click(string name){
   foreach(var b in Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None))if(b.name==name){Canvas.ForceUpdateCanvases();var data=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,b.transform.position),button=PointerEventData.InputButton.Left};var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Assert.IsTrue(hits.Exists(h=>h.gameObject==b.gameObject));ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerClickHandler);return;}Assert.Fail("Missing button "+name);
  }
  [UnityTest] public IEnumerator PauseNavigationAndGenuineCompletionAllLevels()
  {
   System.IO.Directory.CreateDirectory("/tmp/paradox-feature-previews");
   for(int i=0;i<5;i++){
    SceneManager.LoadScene(MainMenu.LevelScenes[i]);yield return null;yield return null;
    var menu=One<LevelMenus>();Assert.NotNull(menu);Assert.AreEqual(1,Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length);
    var position=One<FirstPersonController>().transform.position;
    menu.TogglePause();Assert.AreEqual(0,Time.timeScale);Assert.IsTrue(LevelMenus.InputBlocked);yield return null;
    if(i==0){Capture("pause");yield return null;}
    Click("RESUME");yield return null;Assert.AreEqual(1,Time.timeScale);Assert.IsFalse(LevelMenus.InputBlocked);Assert.Less(Vector3.Distance(position,One<FirstPersonController>().transform.position),.15f);
    menu.TogglePause();yield return null;Click("RETRY");yield return null;yield return null;Assert.AreEqual(MainMenu.LevelScenes[i],SceneManager.GetActiveScene().name);Assert.AreEqual(1,Time.timeScale);
    menu=One<LevelMenus>();menu.TogglePause();yield return null;Click("HOME");yield return null;yield return null;Assert.AreEqual("MainMenu",SceneManager.GetActiveScene().name);Assert.AreEqual(1,Time.timeScale);
    SceneManager.LoadScene(MainMenu.LevelScenes[i]);yield return null;yield return null;
    var player=One<FirstPersonController>();player.enabled=false;var camera=player.GetComponentInChildren<Camera>();var gun=One<TransferGun>();gun.ToggleEquipped();yield return null;
    var assembly=camera.transform.Find("Transfer Gun Model/Precision transfer assembly");Assert.NotNull(assembly);Assert.AreEqual(0,assembly.GetComponentsInChildren<Collider>().Length);
    if(i==0){Capture("gun-board");camera.transform.LookAt(GameObject.Find("Garden controls tablet").transform.position);Capture("tablet");yield return null;}
    var state=One<PuzzleManager>();Assert.IsFalse(state.Complete());Assert.IsFalse(One<LevelMenus>().Open);
    var config=One<PuzzleConfigurationSelector>().ActiveConfiguration;var dimensions=One<DimensionManager>();var cube=One<TransferableObject>();var plate=One<PressurePlate>();
    cube.MoveToDimension(config.plateDimension,plate.Placement);if(dimensions.Current!=config.plateDimension)Assert.IsTrue(dimensions.TrySwitch());yield return new WaitForSeconds(1.2f);
    Assert.IsTrue(state.DoorOpened);if(dimensions.Current!=config.gateDimension)Assert.IsTrue(dimensions.TrySwitch());yield return new WaitForFixedUpdate();
    // Enter the existing exit trigger with the real player controller after satisfying its real plate.
    var controller=player.GetComponent<CharacterController>();controller.enabled=false;player.transform.position=new Vector3(0,0,23);controller.enabled=true;Physics.SyncTransforms();
    float until=Time.realtimeSinceStartup+3;while(!state.Completed&&Time.realtimeSinceStartup<until){controller.Move(Vector3.down*.01f);yield return null;}
    Assert.IsTrue(state.Completed);menu=One<LevelMenus>();Assert.IsTrue(menu.Completed);Assert.IsTrue(menu.Open);Assert.AreEqual(0,Time.timeScale);
    menu.TogglePause();Assert.IsTrue(menu.Completed);Assert.IsTrue(menu.Open);
    if(i==4){Capture("completed");yield return null;}
    yield return null;Click(i==4?"MAIN MENU":"CONTINUE");yield return null;yield return null;Assert.AreEqual(i==4?"MainMenu":MainMenu.LevelScenes[i+1],SceneManager.GetActiveScene().name);Assert.AreEqual(1,Time.timeScale);
   }
  }
 }
}
