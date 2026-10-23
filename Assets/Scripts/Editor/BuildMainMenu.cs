using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Paradox.Editor
{
    public static class BuildMainMenu
    {
        private static Font font;
        public static void Apply()
        {
            AssetDatabase.Refresh();font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/MainMenu/TitleSerif.ttf");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Menu camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.03f,.02f,.08f);camera.orthographic=true;
            new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var canvas=new GameObject("Menu canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var bg=Rect("Split worlds",canvas.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            foreach(bool dark in new[]{false,true}){
                var half=Rect(dark?"Black cosmos":"White sky",bg,new Vector2(dark?.5f:0,0),new Vector2(dark?1:.5f,1),Vector2.zero,Vector2.zero);
                var mask=half.gameObject.AddComponent<RectMask2D>();
                var image=Rect("Sky illustration",half,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject.AddComponent<RawImage>();image.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(dark?"Assets/Art/GardenSky/Night.png":"Assets/Art/GardenSky/Day.png");image.raycastTarget=false;
                var fit=image.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=(float)image.texture.width/image.texture.height;
            }
            var art=Rect("Floating architecture and celestial rift",canvas.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject.AddComponent<MenuAtmosphere>();art.raycastTarget=false;
            // A centred design frame preserves spacing on narrow and ultrawide screens.
            var frame=Rect("Responsive content",canvas.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(1000,900));
            var title=Label("PARADOX",frame,new Vector2(0,200),new Vector2(940,140),112,new Color(.96f,.95f,1));title.gameObject.AddComponent<Shadow>().effectColor=new Color(.08f,.025f,.2f,.85f);title.GetComponent<Shadow>().effectDistance=new Vector2(2,-3);
            Label("T W O   W O R L D S   ·   O N E   T R U T H",frame,new Vector2(0,118),new Vector2(800,35),17,new Color(.83f,.85f,.98f));
            foreach(int sign in new[]{-1,1}){var line=Rect("Silver rule",frame,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(sign*160,90),new Vector2(280,1)).gameObject.AddComponent<Image>();line.color=new Color(.78f,.8f,1,.65f);line.raycastTarget=false;}
            Label("◇",frame,new Vector2(0,90),new Vector2(32,32),26,new Color(.9f,.91f,1));
            var home=Rect("Main buttons",frame,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            Color gold=new Color(.9f,.75f,.48f), silver=new Color(.64f,.72f,.94f),violet=new Color(.68f,.51f,.91f);
            var play=Button("PLAY",home,-45,gold);var choose=Button("LEVEL SELECT",home,-143,silver);var quit=Button("QUIT",home,-241,violet);
            var picker=Rect("Level selection",frame,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var buttons=new Button[5];for(int i=0;i<5;i++)buttons[i]=Button((i+1).ToString("00")+"    "+MainMenu.LevelNames[i],picker,22-i*72,silver,58,23);
            var back=Button("BACK",picker,-345,gold,54,21);
            var status=Label("",frame,new Vector2(0,-412),new Vector2(900,28),18,new Color(.93f,.88f,.76f));
            Label("A JOURNEY BETWEEN LIGHT & SHADOW",canvas.transform,new Vector2(0,24),new Vector2(850,28),14,new Color(.8f,.8f,.92f)).rectTransform.anchorMin=new Vector2(.5f,0);
            var footer=canvas.transform.GetChild(canvas.transform.childCount-1).GetComponent<RectTransform>();footer.anchorMax=new Vector2(.5f,0);
            var menu=new GameObject("Main menu").AddComponent<MainMenu>();menu.Configure(home.gameObject,picker.gameObject,play,choose,quit,back,buttons,status);picker.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/MainMenu.unity");
            var existing=EditorBuildSettings.scenes.Where(s=>s.path!="Assets/Scenes/MainMenu.unity").ToList();
            existing.Insert(0,new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",true));
            foreach(string name in MainMenu.LevelScenes){string path="Assets/Scenes/"+name+".unity";if(!existing.Any(s=>s.path==path))existing.Add(new EditorBuildSettingsScene(path,true));else existing.First(s=>s.path==path).enabled=true;}
            EditorBuildSettings.scenes=existing.ToArray();AssetDatabase.SaveAssets();
        }
        public static void Standalone(){var result=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),"/tmp/paradox-menu-build/Paradox.x86_64",BuildTarget.StandaloneLinux64,BuildOptions.Development);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Build failed");}
        private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 position,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.anchoredPosition=position;r.sizeDelta=size;return r;}
        private static Text Label(string text,Transform parent,Vector2 pos,Vector2 size,int fontSize,Color color){var r=Rect(text.Length>0?text:"Status",parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),pos,size);var t=r.gameObject.AddComponent<Text>();t.text=text;t.font=font;t.fontSize=fontSize;t.color=color;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;}
        private static Button Button(string label,Transform parent,float y,Color accent,float height=76,int size=30){var r=Rect(label,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,y),new Vector2(440,height));var g=r.gameObject.AddComponent<MenuButtonGraphic>();g.color=accent;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=g;var c=b.colors;c.normalColor=Color.white;c.highlightedColor=new Color(1.2f,1.2f,1.2f);c.selectedColor=c.highlightedColor;c.pressedColor=new Color(.7f,.7f,.85f);c.fadeDuration=.15f;b.colors=c;Label(label,r,Vector2.zero,new Vector2(360,height),size,new Color(.97f,.94f,.88f));return b;}
    }
}
