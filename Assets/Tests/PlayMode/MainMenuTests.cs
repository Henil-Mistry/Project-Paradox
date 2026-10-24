using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Paradox.Tests
{
    public sealed class MainMenuTests
    {
        private IEnumerator Open(){SceneManager.LoadScene("MainMenu");yield return null;yield return null;var intro=Object.FindFirstObjectByType<TitleScreenAnimator>();if(intro)intro.SkipIntro();yield return null;yield return null;Assert.AreEqual(1,Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length);Assert.AreEqual(CursorLockMode.None,Cursor.lockState);}
        private Button Find(string name){foreach(var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))if(b.name==name)return b;Assert.Fail("Missing button "+name);return null;}
        private void Click(string name){var b=Find(name);Canvas.ForceUpdateCanvases();var screen=RectTransformUtility.WorldToScreenPoint(null,b.GetComponent<RectTransform>().position);var data=new PointerEventData(EventSystem.current){position=screen,button=PointerEventData.InputButton.Left};var results=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(data,results);Assert.IsTrue(results.Exists(r=>r.gameObject==b.gameObject),"Mouse cannot hit "+name);ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerClickHandler);}
        private IEnumerator AwaitScene(string scene){float until=Time.realtimeSinceStartup+15;while(SceneManager.GetActiveScene().name!=scene&&Time.realtimeSinceStartup<until)yield return null;yield return null;Assert.AreEqual(scene,SceneManager.GetActiveScene().name);Assert.IsNotNull(Object.FindFirstObjectByType<DimensionManager>());Assert.IsNotNull(Object.FindFirstObjectByType<FirstPersonController>());}
        [UnityTest] public IEnumerator PlayAndAllFiveSelections(){
            yield return Open();Click("PLAY");yield return AwaitScene("Level01");
            for(int i=0;i<5;i++){
                yield return Open();Click("LEVEL SELECT");yield return null;Assert.IsTrue(Object.FindFirstObjectByType<MainMenu>().Selecting);
                Click("BACK");yield return null;Assert.IsFalse(Object.FindFirstObjectByType<MainMenu>().Selecting);Click("LEVEL SELECT");yield return null;
                Click((i+1).ToString("00")+"    "+MainMenu.LevelNames[i]);yield return AwaitScene(MainMenu.LevelScenes[i]);
            }
        }
        [UnityTest] public IEnumerator MenuLayoutAndEditorQuit(){
            yield return Open();Assert.AreEqual("MainMenu",System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(0)));
            foreach(string level in MainMenu.LevelScenes)Assert.IsTrue(Application.CanStreamedLevelBeLoaded(level));
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1024,768),new Vector2Int(2560,1080),new Vector2Int(720,1280)}){
                Capture(size,false);yield return null;Click("LEVEL SELECT");yield return null;Capture(size,true);yield return null;Click("BACK");yield return null;
            }
            Click("QUIT");yield return null;Assert.AreEqual("MainMenu",SceneManager.GetActiveScene().name);
        }
        private void Capture(Vector2Int size,bool picker){
            var camera=Object.FindFirstObjectByType<Camera>();var canvas=Object.FindFirstObjectByType<Canvas>();
            var target=new RenderTexture(size.x,size.y,24);var old=RenderTexture.active;camera.targetTexture=target;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();camera.Render();
            foreach(var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None)){
                var corners=new Vector3[4];b.GetComponent<RectTransform>().GetWorldCorners(corners);
                foreach(var p in corners){Vector3 v=camera.WorldToViewportPoint(p);Assert.That(v.x,Is.InRange(0f,1f));Assert.That(v.y,Is.InRange(0f,1f));}
            }
            RenderTexture.active=target;var texture=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,size.x,size.y),0,0);texture.Apply();
            System.IO.Directory.CreateDirectory("/tmp/menu-previews");System.IO.File.WriteAllBytes($"/tmp/menu-previews/{size.x}x{size.y}-{picker}.png",texture.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=old;canvas.renderMode=RenderMode.ScreenSpaceOverlay;Object.Destroy(texture);Object.Destroy(target);Canvas.ForceUpdateCanvases();
        }
    }
}
