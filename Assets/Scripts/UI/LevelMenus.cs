using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Paradox
{
    [DefaultExecutionOrder(-1000)]
    public sealed class LevelMenus : MonoBehaviour
    {
        public static LevelMenus Active { get; private set; }
        public static bool InputBlocked => Active && (Active.Open || Time.frameCount == Active.closedFrame);
        public Font font;
        public bool Open { get; private set; }
        public bool Completed { get; private set; }
        GameObject panel; Text heading; Button resume, next; float priorScale=1; int closedFrame=-1; bool leaving;
        void Awake() { Active=this; Build(); }
        void Update() { if (Input.GetKeyDown(KeyCode.Escape) && !Completed) TogglePause(); }
        public void TogglePause()
        {
            var transition=GetComponent<RealmTransition>();
            if (Completed || leaving || (transition && transition.Busy)) return;
            if(Open) Resume(); else Show(false);
        }
        public void ShowCompletion() { if(!Completed) Show(true); }
        void Show(bool complete)
        {
            if(!Open) priorScale=Time.timeScale;
            Completed=complete; Open=true; Time.timeScale=0;
            FirstPersonController.LockCursor(false); panel.SetActive(true);
            heading.text=complete?"PUZZLE COMPLETED":"BETWEEN WORLDS";
            resume.gameObject.SetActive(!complete);next.gameObject.SetActive(complete);
        }
        public void Resume() { if(Completed)return; panel.SetActive(false);Open=false;closedFrame=Time.frameCount;Time.timeScale=priorScale;FirstPersonController.LockCursor(true); }
        public void Home() { if(leaving)return;leaving=true;Time.timeScale=1;SceneManager.LoadScene("MainMenu"); }
        public void Retry() { if(leaving)return;leaving=true;Time.timeScale=1;GetComponent<GameFlow>().Restart(); }
        public void Continue() { if(!Completed||leaving)return;if(SceneManager.GetActiveScene().name=="Level05_Heart"){Home();return;}leaving=true;Time.timeScale=1;GetComponent<GameFlow>().NextLevel(); }
        void OnDestroy(){if(Active==this){Active=null;if(Open)Time.timeScale=priorScale;}}
        RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
        { var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=position;return r; }
        Text Label(string value,Transform parent,Vector2 size,Vector2 position,int point)
        {var t=Rect(value,parent,size,position).gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=point;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.91f,.88f,.78f);t.raycastTarget=false;return t;}
        Button Button(string text, Transform parent,float y,UnityEngine.Events.UnityAction action)
        {var r=Rect(text,parent,new Vector2(360,62),new Vector2(0,y));var g=r.gameObject.AddComponent<MenuButtonGraphic>();var b=r.gameObject.AddComponent<Button>();b.targetGraphic=g;b.onClick.AddListener(action);Label(text,r,new Vector2(340,60),Vector2.zero,24);return b;}
        void Build()
        {
            var hud=FindFirstObjectByType<HUD>();var canvas=hud.GetComponent<Canvas>();
            var r=Rect("Shared pause and completion",canvas.transform,Vector2.zero,Vector2.zero);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            panel=r.gameObject;var shade=panel.AddComponent<Image>();shade.color=new Color(.015f,.01f,.035f,.82f);
            var frame=Rect("Silver violet tablet",r,new Vector2(560,470),Vector2.zero);frame.gameObject.AddComponent<MenuButtonGraphic>();
            heading=Label("BETWEEN WORLDS",frame,new Vector2(520,70),new Vector2(0,150),32);
            Label("P A R A D O X",frame,new Vector2(400,30),new Vector2(0,103),16);
            Button("HOME",frame,35,Home);resume=Button("RESUME",frame,-45,Resume);Button("RETRY",frame,-125,Retry);
            next=Button(SceneManager.GetActiveScene().name=="Level05_Heart"?"MAIN MENU":"CONTINUE",frame,-45,Continue);
            panel.SetActive(false);
        }
    }
}
