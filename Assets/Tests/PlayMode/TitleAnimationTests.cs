using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Paradox.Tests
{
 public sealed class TitleAnimationTests
 {
  Button Find(string name){foreach(var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))if(b.name==name)return b;return null;}
  void Capture(string name,int w=1920,int h=1080){
   var camera=Object.FindFirstObjectByType<Camera>();var canvas=Object.FindFirstObjectByType<Canvas>();var target=new RenderTexture(w,h,24);camera.targetTexture=target;var old=RenderTexture.active;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
   var texture=new Texture2D(w,h,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,w,h),0,0);texture.Apply();System.IO.Directory.CreateDirectory("/tmp/title-motion");System.IO.File.WriteAllBytes("/tmp/title-motion/"+name+".png",texture.EncodeToPNG());
   camera.targetTexture=null;RenderTexture.active=old;canvas.renderMode=RenderMode.ScreenSpaceOverlay;Object.Destroy(texture);Object.Destroy(target);
  }
  [UnityTest] public IEnumerator IntroLoopsFeedbackReduceMotionAndPlay(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   var motion=Object.FindFirstObjectByType<TitleScreenAnimator>();Assert.NotNull(motion);var play=Find("PLAY");Assert.IsFalse(play.interactable);
   Time.timeScale=0;yield return new WaitForSecondsRealtime(.65f);Capture("intro");Assert.IsFalse(motion.IntroComplete);
   yield return new WaitForSecondsRealtime(2);yield return null;Assert.IsTrue(motion.IntroComplete);Assert.IsTrue(play.interactable);Capture("rest");
   var data=new PointerEventData(EventSystem.current);ExecuteEvents.Execute(play.gameObject,data,ExecuteEvents.pointerEnterHandler);yield return new WaitForSecondsRealtime(.25f);Assert.Greater(play.transform.localScale.x,1.03f);Capture("hover");
   ExecuteEvents.Execute(play.gameObject,data,ExecuteEvents.pointerDownHandler);yield return new WaitForSecondsRealtime(.18f);Assert.Less(play.transform.localScale.x,1);ExecuteEvents.Execute(play.gameObject,data,ExecuteEvents.pointerUpHandler);
   yield return new WaitForSecondsRealtime(3);Capture("loops-later");Capture("narrow",1024,768);Capture("wide",2560,1080);
   motion.reduceMotion=true;yield return new WaitForSecondsRealtime(.3f);Capture("reduced");Assert.That(play.transform.localScale.x,Is.EqualTo(1).Within(.002f));
   ExecuteEvents.Execute(play.gameObject,data,ExecuteEvents.pointerClickHandler);Assert.IsTrue(motion.Exiting);Assert.AreEqual("MainMenu",SceneManager.GetActiveScene().name);yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual("MainMenu",SceneManager.GetActiveScene().name);Capture("play-fade");
   float end=Time.realtimeSinceStartup+10;while(SceneManager.GetActiveScene().name=="MainMenu"&&Time.realtimeSinceStartup<end)yield return null;
   Assert.AreEqual("Level01",SceneManager.GetActiveScene().name);Time.timeScale=1;
  }
  [UnityTest] public IEnumerator ProceduralMotionHasNoSteadyStateManagedAllocations(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;var motion=Object.FindFirstObjectByType<TitleScreenAnimator>();motion.SkipIntro();yield return null;
   var art=Object.FindFirstObjectByType<MenuAtmosphere>();var method=typeof(MenuAtmosphere).GetMethod("OnPopulateMesh",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.DeclaredOnly);
   var draw=(System.Action<VertexHelper>)System.Delegate.CreateDelegate(typeof(System.Action<VertexHelper>),art,method);
   var update=(System.Action)System.Delegate.CreateDelegate(typeof(System.Action),motion,typeof(TitleScreenAnimator).GetMethod("Update",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.DeclaredOnly));
   using(var vh=new VertexHelper()){
    for(int i=0;i<12;i++){update();draw(vh);}long before=System.GC.GetAllocatedBytesForCurrentThread();
    for(int i=0;i<60;i++){update();draw(vh);}long allocated=System.GC.GetAllocatedBytesForCurrentThread()-before;
    Assert.AreEqual(0,allocated,"Controller and warmed procedural mesh should not allocate each frame");
    var a=default(UIVertex);var b=default(UIVertex);art.SetTitleMotion(1,false,Vector2.zero,1.6f,4,3,3,1.2f,.12f,1,.12f);draw(vh);vh.PopulateUIVertex(ref a,30);
    art.SetTitleMotion(10,false,Vector2.zero,1.6f,4,3,3,1.2f,.12f,1,.12f);draw(vh);vh.PopulateUIVertex(ref b,30);Assert.AreNotEqual(a.position,b.position);
   }
  }
  [UnityTest] public IEnumerator SkipRestoresAuthoredPositions(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;var motion=Object.FindFirstObjectByType<TitleScreenAnimator>();motion.SkipIntro();yield return null;yield return null;
   Assert.IsTrue(motion.IntroComplete);Assert.IsTrue(Find("PLAY").interactable);Assert.AreEqual(new Vector2(0,-45),Find("PLAY").GetComponent<RectTransform>().anchoredPosition);
   Assert.AreEqual(Vector2.zero,GameObject.Find("White sky").GetComponent<RectTransform>().anchoredPosition);Assert.AreEqual(Vector2.zero,GameObject.Find("Black cosmos").GetComponent<RectTransform>().anchoredPosition);
  }
 }
}
