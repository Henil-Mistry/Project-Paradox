using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Paradox {
// Presentation only; DimensionManager remains the sole owner of switching and safety checks.
public sealed class RealmTransition : MonoBehaviour {
[SerializeField] private DimensionManager manager;
private Image fade;private GameObject overlay;private Coroutine routine;
public bool Busy {get;private set;}
public float Coverage {get;private set;}
public void Configure(DimensionManager owner){manager=owner;}
public bool Begin(){
if(Busy || !isActiveAndEnabled || !manager)return false;
Busy=true;routine=StartCoroutine(Animate());return true;
}
private IEnumerator Animate(){
try {
// Allow a gentler reveal into the bright Garden; preserve White-to-Black timing.
bool enteringWhite=manager.Current==Dimension.Black;
float fadeOutDuration=enteringWhite?.7f:.42f;
float fadeInDuration=enteringWhite?.8f:.42f;
overlay=new GameObject("Realm transition overlay",typeof(Canvas));var canvas=overlay.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
var image=new GameObject("Black fade",typeof(RectTransform),typeof(Image));image.transform.SetParent(overlay.transform,false);var rect=image.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;fade=image.GetComponent<Image>();fade.color=Color.clear;fade.raycastTarget=false;
for(float t=0;t<fadeOutDuration;t+=Time.unscaledDeltaTime){Set(t/fadeOutDuration);yield return null;}
Set(1);yield return null;
bool switched=manager.CommitTransition(this); // Revalidate held object, obstruction and completion at the covered frame.
yield return null;
for(float t=0;t<fadeInDuration;t+=Time.unscaledDeltaTime){
if(switched && enteringWhite)SetWhiteReveal(t/fadeInDuration);
else Set(1-t/fadeInDuration);
yield return null;
}
Set(0);
} finally {Clear();}
}
private void SetWhiteReveal(float progress){
// Blend the same covered overlay into a moderate ivory wash, then clear it.
// Both segments meet with zero slope: no colour cut or opacity reset.
float blend=Mathf.SmoothStep(0,1,Mathf.Clamp01(progress/.55f));
float clear=Mathf.SmoothStep(0,1,Mathf.Clamp01((progress-.55f)/.45f));
Coverage=Mathf.Lerp(1,.3f,blend)*(1-clear);
Color tint=Color.Lerp(Color.black,new Color(.92f,.90f,.85f),blend);
tint.a=Coverage;fade.color=tint;
}
private void Set(float value){Coverage=Mathf.SmoothStep(0,1,Mathf.Clamp01(value));if(fade)fade.color=new Color(0,0,0,Coverage);}
private void Clear(){if(overlay){overlay.SetActive(false);Destroy(overlay);}overlay=null;fade=null;Coverage=0;Busy=false;routine=null;}
private void OnDisable(){if(routine!=null)StopCoroutine(routine);Clear();}
}
}
