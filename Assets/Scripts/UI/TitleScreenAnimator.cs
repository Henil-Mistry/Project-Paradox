using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Paradox
{
    /// <summary>One owner for title-screen motion. Uses cached UI references and unscaled time.
    /// Existing renderers only draw the supplied state; routing remains in MainMenu.</summary>
    [DefaultExecutionOrder(-50), RequireComponent(typeof(Text))]
    public sealed class TitleScreenAnimator : BaseMeshEffect
    {
        [Header("Accessibility")]
        public bool reduceMotion;
        [Header("Intro and exit (seconds)")]
        [Min(.1f)] public float introDuration=2.5f;
        public float taglineDelay=.4f, buttonStagger=.12f, playFadeDuration=.5f;
        public float introSlide=24, introLetterSpacing=12;
        [Header("Living background")]
        public float ringSpeed=1.6f, ringWobble=4, pillarBob=3;
        public float cloudSpeedNear=3, cloudSpeedFar=1.2f, glowPulse=.12f;
        public float particleSpeed=1, seamSpeed=.12f;
        [Header("Parallax and title")]
        public float parallaxPixels=16, parallaxDamping=5, titleSweepPeriod=7;
        [Header("Button feedback")]
        public float hoverScale=1.05f, pressScale=.97f, feedbackDuration=.15f;
        [SerializeField] MenuAtmosphere atmosphere;
        [SerializeField] RectTransform whiteHalf, blackHalf;
        [SerializeField] Text tagline;
        class ButtonState
        {
            public Button button; public RectTransform rect; public CanvasGroup group;
            public Vector2 position; public Vector3 scale; public MenuButtonGraphic border;
            public Color color; public bool hover,pressed,selected; public float emphasis,sheen=-1,pressUntil; public int order;
        }
        ButtonState[] buttons;
        Outline titleGlow; CanvasGroup titleGroup,taglineGroup; Image cover; Transform fx;
        Vector2 whiteRest,blackRest,parallax; float started,clock,fadeStarted,spacing,sweep;
        bool ready,exiting,invoked,initialized; int enableFrame;
        Button play; Button.ButtonClickedEvent originalPlayEvent;
        public bool IntroComplete=>ready;
        public bool Exiting=>exiting;
        protected override void Start()
        {
            base.Start();
            var canvas=GetComponentInParent<Canvas>();
            if(!canvas){Debug.LogWarning("TitleScreenAnimator: title must be under the menu Canvas.",this);enabled=false;return;}
            if(!atmosphere)atmosphere=canvas.GetComponentInChildren<MenuAtmosphere>(true);
            var all=canvas.GetComponentsInChildren<RectTransform>(true);
            for(int i=0;i<all.Length;i++){
                if(all[i].name=="White sky"&&!whiteHalf)whiteHalf=all[i];
                if(all[i].name=="Black cosmos"&&!blackHalf)blackHalf=all[i];
                var text=all[i].GetComponent<Text>();if(text&&text.text.StartsWith("T W O")&&!tagline)tagline=text;
            }
            if(!atmosphere||!whiteHalf||!blackHalf||!tagline){Debug.LogWarning("TitleScreenAnimator: missing artwork, sky halves or tagline; available elements will still animate.",this);}
            if(whiteHalf)whiteRest=whiteHalf.anchoredPosition;if(blackHalf)blackRest=blackHalf.anchoredPosition;
            titleGlow=gameObject.AddComponent<Outline>();titleGlow.effectColor=new Color(.65f,.55f,1,.08f);titleGlow.effectDistance=new Vector2(2,-2);
            titleGroup=Group(gameObject);if(tagline)taglineGroup=Group(tagline.gameObject);
            fx=canvas.transform.Find("TitleFX");if(!fx){var g=new GameObject("TitleFX",typeof(RectTransform));g.transform.SetParent(canvas.transform,false);fx=g.transform;}
            var fr=(RectTransform)fx;fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.offsetMin=fr.offsetMax=Vector2.zero;fx.SetAsLastSibling();
            var overlay=new GameObject("Intro and Play cover",typeof(RectTransform),typeof(Image));overlay.transform.SetParent(fx,false);
            var r=(RectTransform)overlay.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            cover=overlay.GetComponent<Image>();cover.color=Color.black;cover.raycastTarget=true;
            var found=canvas.GetComponentsInChildren<Button>(true);buttons=new ButtonState[found.Length];int order=0;
            for(int i=0;i<found.Length;i++){
                var b=found[i];var state=new ButtonState{button=b,rect=(RectTransform)b.transform,group=Group(b.gameObject),scale=b.transform.localScale,position=((RectTransform)b.transform).anchoredPosition,border=b.GetComponent<MenuButtonGraphic>(),order=b.transform.parent.name=="Main buttons"?order++:0};
                if(state.border)state.color=state.border.color;buttons[i]=state;b.interactable=false;
                // Bind once. These callbacks allocate only at setup, never during animation.
                var events=b.GetComponent<EventTrigger>();if(!events)events=b.gameObject.AddComponent<EventTrigger>();
                Bind(events,EventTriggerType.PointerEnter,()=>state.hover=true);Bind(events,EventTriggerType.PointerExit,()=>{state.hover=false;state.pressed=false;});
                Bind(events,EventTriggerType.PointerDown,()=>state.pressed=true);Bind(events,EventTriggerType.PointerUp,()=>state.pressed=false);
                Bind(events,EventTriggerType.Select,()=>state.selected=true);Bind(events,EventTriggerType.Deselect,()=>state.selected=false);
                Bind(events,EventTriggerType.Submit,()=>{state.sheen=0;state.pressUntil=clock+.12f;});
                if(b.name=="PLAY"){play=b;originalPlayEvent=b.onClick;b.onClick=new Button.ButtonClickedEvent();b.onClick.AddListener(BeginPlay);}
            }
            if(!play)Debug.LogWarning("TitleScreenAnimator: PLAY button missing; exit fade unavailable.",this);
            started=Time.unscaledTime;initialized=true;ApplyIntro(0);
        }
        static CanvasGroup Group(GameObject g){var c=g.GetComponent<CanvasGroup>();return c?c:g.AddComponent<CanvasGroup>();}
        static void Bind(EventTrigger t,EventTriggerType id,UnityEngine.Events.UnityAction action){var e=new EventTrigger.Entry{eventID=id};e.callback.AddListener(_=>action());t.triggers.Add(e);}
        static float Ease(float t){t=Mathf.Clamp01(t);return 1-(1-t)*(1-t)*(1-t);}
        public void SkipIntro(){if(!initialized||ready)return;ApplyIntro(1);ready=true;enableFrame=Time.frameCount+1;}
        public void BeginPlay(){if(!ready||exiting)return;exiting=true;fadeStarted=Time.unscaledTime;cover.raycastTarget=true;for(int i=0;i<buttons.Length;i++)buttons[i].button.interactable=false;}
        void Update()
        {
            if(!initialized)return;
            float dt=Time.unscaledDeltaTime;clock+=dt;
            if(!ready){float p=(Time.unscaledTime-started)/Mathf.Max(.1f,introDuration);ApplyIntro(p);if(p>=1||Input.anyKeyDown)SkipIntro();}
            else if(!exiting&&Time.frameCount>=enableFrame){cover.raycastTarget=false;for(int i=0;i<buttons.Length;i++)buttons[i].button.interactable=true;}
            if(exiting){float p=Ease((Time.unscaledTime-fadeStarted)/Mathf.Max(.1f,playFadeDuration));cover.color=new Color(0,0,0,p);if(p>=1&&!invoked){invoked=true;originalPlayEvent?.Invoke();}}
            Vector2 target=reduceMotion?Vector2.zero:new Vector2((Input.mousePosition.x/Mathf.Max(1,Screen.width)-.5f)*-2,(Input.mousePosition.y/Mathf.Max(1,Screen.height)-.5f)*-2)*parallaxPixels;
            target=Vector2.ClampMagnitude(target,parallaxPixels);parallax=Vector2.Lerp(parallax,target,1-Mathf.Exp(-parallaxDamping*dt));
            if(atmosphere)atmosphere.SetTitleMotion(reduceMotion?0:clock,reduceMotion,parallax,ringSpeed,ringWobble,pillarBob,cloudSpeedNear,cloudSpeedFar,glowPulse,particleSpeed,seamSpeed);
            if(ready)for(int i=0;i<buttons.Length;i++){
                var b=buttons[i];bool selected=EventSystem.current&&EventSystem.current.currentSelectedGameObject==b.button.gameObject;
                bool active=b.hover||selected;float previous=b.emphasis;
                b.emphasis=Mathf.MoveTowards(b.emphasis,active?1:0,dt/Mathf.Max(.01f,feedbackDuration));
                if(active&&previous==0)b.sheen=0;
                b.sheen=b.sheen>=0?b.sheen+dt:-1;
                float scale=reduceMotion?1:(b.pressed||clock<b.pressUntil)?pressScale:Mathf.Lerp(1,hoverScale,b.emphasis);
                b.rect.localScale=Vector3.Lerp(b.rect.localScale,b.scale*scale,1-Mathf.Exp(-dt*3/Mathf.Max(.01f,feedbackDuration)));
                if(b.border){b.border.color=Color.Lerp(b.color,Color.white,b.emphasis*.3f);b.border.TitleEmphasis=reduceMotion?0:b.emphasis*(.8f+.2f*Mathf.Sin(clock*2.4f));b.border.TitleSheen=reduceMotion||b.sheen<0||b.sheen>.6f?-1:b.sheen/.6f;b.border.SetVerticesDirty();}
            }
            sweep=reduceMotion?-1:Mathf.Repeat(clock/Mathf.Max(1,titleSweepPeriod),1)*2-.5f;
            graphic.SetVerticesDirty();
        }
        void ApplyIntro(float p)
        {
            float elapsed=Mathf.Clamp01(p)*introDuration;
            cover.color=new Color(0,0,0,1-Ease(elapsed/.8f));
            // Move each half from the seam to its original anchored position, under its existing mask.
            float shift=reduceMotion?0:(1-Ease(elapsed/1.2f))*introSlide;
            if(whiteHalf)whiteHalf.anchoredPosition=whiteRest+Vector2.right*shift;
            if(blackHalf)blackHalf.anchoredPosition=blackRest-Vector2.right*shift;
            float title=Ease((elapsed-.3f)/1.2f);titleGroup.alpha=title;titleGlow.effectColor=new Color(.65f,.55f,1,.06f+Mathf.Sin(title*Mathf.PI)*.12f);spacing=reduceMotion?0:(1-title)*introLetterSpacing;
            if(taglineGroup)taglineGroup.alpha=Ease((elapsed-.3f-taglineDelay)/.9f);
            for(int i=0;i<buttons.Length;i++){var b=buttons[i];float v=Ease((elapsed-1.1f-b.order*buttonStagger)/.65f);b.group.alpha=v;b.rect.anchoredPosition=b.position-Vector2.up*(reduceMotion?0:(1-v)*introSlide);}
        }
        public override void ModifyMesh(VertexHelper vh)
        {
            if(!IsActive())return;
            float min=float.MaxValue,max=float.MinValue;UIVertex v=default;
            for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref v,i);min=Mathf.Min(min,v.position.x);max=Mathf.Max(max,v.position.x);}
            float width=Mathf.Max(1,max-min);
            for(int i=0;i<vh.currentVertCount;i++){
                vh.PopulateUIVertex(ref v,i);float u=(v.position.x-min)/width;
                // Offset glyph centres instead of scaling glyphs; final spacing is exactly the authored text.
                v.position.x+=(Mathf.Clamp(Mathf.Floor(u*7),0,6)-3)*spacing;
                if(v.color.r>100){Color tint=Color.Lerp(new Color(1,.94f,.78f),new Color(.79f,.72f,1),Mathf.SmoothStep(0,1,u));float shine=sweep<0?0:Mathf.Max(0,1-Mathf.Abs(u-sweep)*8)*.2f;tint=Color.Lerp(tint,Color.white,shine);tint.a=v.color.a/255f;v.color=tint;}
                vh.SetUIVertex(v,i);
            }
        }
        protected override void OnDestroy(){if(play&&originalPlayEvent!=null)play.onClick=originalPlayEvent;base.OnDestroy();}
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Paradox/Install Title Motion")]
        public static void Install()
        {
            var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            foreach(var text in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))if(text.text=="PARADOX"&&!text.GetComponent<TitleScreenAnimator>())text.gameObject.AddComponent<TitleScreenAnimator>();
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }
#endif
    }
}
