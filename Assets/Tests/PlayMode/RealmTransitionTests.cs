using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Paradox.Tests {
public sealed class RealmTransitionTests {
[UnityTest] public IEnumerator CoveredSwitchSpamAndInterruption(){yield return CheckScene("Level01");}
[UnityTest] public IEnumerator LibraryTransition(){yield return CheckScene("Level02_Library");}
[UnityTest] public IEnumerator LakeTransition(){yield return CheckScene("Level03_MirrorLake");}
[UnityTest] public IEnumerator ObservatoryTransition(){yield return CheckScene("Level04_Observatory");}
[UnityTest] public IEnumerator HeartTransition(){yield return CheckScene("Level05_Heart");}
private IEnumerator CheckScene(string scene){
SceneManager.LoadScene(scene);yield return null;yield return new WaitForFixedUpdate();
var manager=Object.FindFirstObjectByType<DimensionManager>();var transition=manager.GetComponent<RealmTransition>();var sky=Object.FindFirstObjectByType<GardenSkyBackground>();
Assert.AreEqual(1,Object.FindObjectsByType<RealmTransition>(FindObjectsSortMode.None).Length);
int changes=0;manager.Changed+=d=>{Assert.AreEqual(1,transition.Coverage);changes++;Capture("Covered-"+d,true);};
for(int i=0;i<6;i++){
var before=manager.Current;float start=Time.unscaledTime;float previous=0;bool reveal=false;bool capturedOut=false;bool capturedIn=false;Color previousTint=Color.black;
Assert.IsTrue(manager.RequestSwitch());Assert.IsFalse(manager.RequestSwitch());Assert.IsFalse(manager.TrySwitch());
while(transition.Busy){
Assert.Less(Time.unscaledTime-start,2.2f,"Transition must finish without restarting");
var overlay=GameObject.Find("Realm transition overlay");var fade=overlay.GetComponentInChildren<UnityEngine.UI.Image>();
Assert.AreEqual(1,overlay.GetComponentsInChildren<UnityEngine.UI.Graphic>().Length);
bool whiteReveal=before==Dimension.Black && manager.Current==Dimension.White;
if(!whiteReveal){Assert.AreEqual(0,fade.color.r);Assert.AreEqual(0,fade.color.g);Assert.AreEqual(0,fade.color.b);}
else{
Assert.LessOrEqual(fade.color.r,.921f);Assert.GreaterOrEqual(fade.color.r+.0001f,previousTint.r);
Assert.LessOrEqual(Mathf.Abs(fade.color.r-previousTint.r),4*Time.unscaledDeltaTime+.02f,"Tint must blend without a sudden white cut");
if(fade.color.r>.9f)Assert.LessOrEqual(fade.color.a,.32f,"White wash must have moderate opacity");
}
previousTint=fade.color;
Assert.AreEqual(transition.Coverage,fade.color.a);
if(manager.Current!=before)reveal=true;
if(reveal)Assert.LessOrEqual(transition.Coverage,previous+.0001f,"Reveal opacity must never increase or restart");
else Assert.GreaterOrEqual(transition.Coverage+.0001f,previous,"Fade-out opacity must never reset");
Assert.LessOrEqual(Mathf.Abs(transition.Coverage-previous),4*Time.unscaledDeltaTime+.02f,"Opacity must change smoothly");
previous=transition.Coverage;
if(!reveal&&!capturedOut&&transition.Coverage>=.45f){Capture("Fade-out-from-"+before,false);capturedOut=true;}
if(reveal&&!capturedIn&&(before==Dimension.Black?fade.color.r>.91f:transition.Coverage<=.5f)){Capture("Reveal-to-"+manager.Current,false);capturedIn=true;}
Assert.IsFalse(manager.RequestSwitch());
yield return null;
}
Assert.IsTrue(capturedOut&&capturedIn);
foreach(var item in Object.FindObjectsByType<DimensionObject>(FindObjectsSortMode.None)){
Assert.AreEqual(item.ExistsIn(manager.Current),item.IsPresent);
if(item.Membership!=DimensionMembership.Both)foreach(var renderer in item.GetComponentsInChildren<Renderer>(true))Assert.AreEqual(item.IsPresent,renderer.enabled,"Dimension visual: "+item.name);
}
if(before==Dimension.Black)Assert.That(Time.unscaledTime-start,Is.InRange(1.2f,1.8f));
else Assert.That(Time.unscaledTime-start,Is.InRange(.8f,1.2f));
Assert.AreEqual(0,transition.Coverage);Assert.AreNotEqual(before,manager.Current);Assert.AreEqual(manager.Current==Dimension.White?"Day":"Night",sky.CurrentTexture.name);Assert.IsNull(GameObject.Find("Realm transition overlay"));
}
Assert.AreEqual(6,changes);
Assert.IsTrue(manager.RequestSwitch());yield return new WaitForSecondsRealtime(.1f);transition.enabled=false;yield return null;Assert.IsFalse(transition.Busy);Assert.IsNull(GameObject.Find("Realm transition overlay"));Assert.AreEqual(6,changes);
transition.enabled=true;Assert.IsTrue(manager.RequestSwitch());yield return new WaitForSecondsRealtime(.6f);Assert.AreEqual(7,changes);transition.enabled=false;yield return null;Assert.IsFalse(transition.Busy);Assert.IsNull(GameObject.Find("Realm transition overlay"));
}
[UnityTest] public IEnumerator UniformBlackOverlayAndCleanEndpoints(){
SceneManager.LoadScene("Level01");yield return null;yield return new WaitForFixedUpdate();
var manager=Object.FindFirstObjectByType<DimensionManager>();Assert.IsTrue(manager.RequestSwitch());
var overlay=GameObject.Find("Realm transition overlay");
Assert.AreEqual(1,overlay.GetComponentsInChildren<UnityEngine.UI.Graphic>().Length);
var fade=overlay.GetComponentInChildren<UnityEngine.UI.Image>();
Assert.IsNull(fade.sprite);Assert.AreEqual(fade.defaultMaterial,fade.material);
foreach(float opacity in new[]{0f,.05f,.25f,.5f,.75f,1f}){
fade.color=new Color(0,0,0,opacity);
Capture($"Uniform-{opacity:0.00}",false,true);
}
manager.GetComponent<RealmTransition>().enabled=false;yield return null;
}
private static Color[] Capture(string name,bool covered,bool isolate=false){
var canvas=GameObject.Find("Realm transition overlay").GetComponent<Canvas>();
var camera=Object.FindFirstObjectByType<FirstPersonController>().GetComponentInChildren<Camera>();
if(isolate){camera=new GameObject("Isolated transition capture").AddComponent<Camera>();camera.enabled=false;}
var target=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;
var mask=camera.cullingMask;var clear=camera.clearFlags;var background=camera.backgroundColor;
var raw=canvas.GetComponentInChildren<UnityEngine.UI.Image>();int layer=raw.gameObject.layer;int canvasLayer=canvas.gameObject.layer;
if(isolate){canvas.gameObject.layer=31;raw.gameObject.layer=31;camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;}
canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=camera.nearClipPlane+.1f;
Canvas.ForceUpdateCanvases();camera.targetTexture=target;camera.Render();RenderTexture.active=target;
var image=new Texture2D(1280,720,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
if(covered){foreach(var pixel in image.GetPixels32())if(pixel.r>1||pixel.g>1||pixel.b>1)Assert.Fail("Covered frame must be completely black");var a=image.GetPixel(5,5);foreach(var point in new[]{new Vector2Int(1274,5),new Vector2Int(5,714),new Vector2Int(1274,714),new Vector2Int(640,360)}){var b=image.GetPixel(point.x,point.y);Assert.Less(Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b),.03f,"Full coverage must conceal all world pixels");}}
System.IO.Directory.CreateDirectory("/tmp/fade-previews");System.IO.File.WriteAllBytes("/tmp/fade-previews/"+SceneManager.GetActiveScene().name+"-"+name+".png",image.EncodeToPNG());
var samples=new[]{image.GetPixel(640,360),image.GetPixel(960,360),image.GetPixel(5,5)};
if(isolate){
var pixels=image.GetPixels32();var first=pixels[0];
foreach(var pixel in pixels)if(pixel.r!=0||pixel.g!=0||pixel.b!=0||pixel.a!=first.a)Assert.Fail("Fade must be uniform black across every pixel, with no dot or shape");
if(raw.color.a==0||raw.color.a==1)Assert.AreEqual(raw.color.a==0?0:255,first.a,"Clean fade endpoints");
else Assert.That(first.a,Is.InRange(1,254));
}
camera.targetTexture=null;RenderTexture.active=previous;canvas.renderMode=RenderMode.ScreenSpaceOverlay;camera.cullingMask=mask;camera.clearFlags=clear;camera.backgroundColor=background;raw.gameObject.layer=layer;canvas.gameObject.layer=canvasLayer;Object.Destroy(target);Object.Destroy(image);
if(isolate)Object.Destroy(camera.gameObject);
return samples;
}
}}
