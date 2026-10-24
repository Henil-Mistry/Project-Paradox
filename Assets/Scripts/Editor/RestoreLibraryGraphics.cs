using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Paradox.Editor
{
    // Targeted, idempotent dressing repair. Never runs another level's builders.
    public static class RestoreLibraryGraphics
    {
        static DimensionManager manager; static Transform root;
        static Material marble,night,silver,metal,whiteRune,violet,book,blackBook;
        const string Folder="Assets/Art/LibraryRestoration";
        static Material Mat(string name,Color c,float metallic=0,Color emission=default)
        {
            string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
            m.color=c;m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",.38f);
            m.SetColor("_EmissionColor",emission);if(emission.maxColorComponent>0){m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;m.EnableKeyword("_EMISSION");}EditorUtility.SetDirty(m);return m;
        }
        static void Tint(Renderer r,Material w,Material b)
        {var t=r.GetComponent<DimensionTint>();if(!t)t=r.gameObject.AddComponent<DimensionTint>();t.Configure(manager,w,b);r.sharedMaterial=w;}
        static GameObject Box(string n,Vector3 p,Vector3 s,Material w,Material b)
        {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root,false);g.transform.position=p;g.transform.localScale=s;Object.DestroyImmediate(g.GetComponent<Collider>());Tint(g.GetComponent<Renderer>(),w,b);return g;}
        static void Glyph(Vector3 p,string text,float size,Quaternion rotation)
        {
            var g=new GameObject("Archive inscription",typeof(TextMesh));g.transform.SetParent(root,false);g.transform.position=p;g.transform.rotation=rotation;
            var t=g.GetComponent<TextMesh>();t.text=text;t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;
            // TextMesh's font atlas must be retained: Standard materials would produce opaque rectangles.
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.font=font;
            var w=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/White lettering.mat");var b=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Violet lettering.mat");
            if(!w){w=new Material(font.material);w.color=new Color(.83f,.94f,1);AssetDatabase.CreateAsset(w,Folder+"/White lettering.mat");b=new Material(font.material);b.color=new Color(.7f,.4f,1);AssetDatabase.CreateAsset(b,Folder+"/Violet lettering.mat");}
            Tint(g.GetComponent<Renderer>(),w,b);
        }
        public static void Apply()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Level02_Library.unity");
            System.IO.Directory.CreateDirectory(Folder);AssetDatabase.Refresh();manager=Object.FindFirstObjectByType<DimensionManager>();
            marble=Mat("Pearl limestone",new Color(.91f,.93f,.94f));night=Mat("Indigo stone",new Color(.09f,.085f,.15f),.12f);
            silver=Mat("Champagne silver",new Color(.68f,.64f,.49f),.55f);metal=Mat("Lunar silver",new Color(.24f,.19f,.36f),.55f);
            whiteRune=Mat("White rune light",new Color(.8f,.9f,1),.1f,new Color(.38f,.48f,.58f));violet=Mat("Violet rune light",new Color(.31f,.12f,.53f),.1f,new Color(.36f,.12f,.63f));
            book=Mat("Archive blue bindings",new Color(.26f,.35f,.43f),.1f);blackBook=Mat("Codex bindings",new Color(.12f,.065f,.2f),.15f);
            foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                string n=r.name;
                if(n=="Book"||n=="Archive volume"||n=="Bound archive volume")Tint(r,book,blackBook);
                else if(n=="Celestial ring")Tint(r,silver,violet);
                else if(n=="Gilt capital"||n=="Architectural molding"||n=="Book spine inlay")Tint(r,silver,metal);
                else if(n=="Archive orrery sun"||n=="Runic stroke"||n=="Diamond emblem")Tint(r,whiteRune,violet);
                else if(n=="Shelf"||n=="Shelf end"||n=="Tall collection shelves"||n=="Fluted pillar"||n=="Reading hall pier"||n=="Glass archive backing")Tint(r,marble,night);
            }
            // Keep all existing architecture and gameplay transforms. Added shelves sit beyond the rear reading-hall route.
            var existing=GameObject.Find("Library restored archive details");if(!existing){
                root=new GameObject("Library restored archive details").transform;
                for(int bay=-1;bay<=1;bay++){
                    float x=bay*6;
                    Box("Archive cabinet shadow",new Vector3(x,3.8f,-18.3f),new Vector3(4.8f,6.7f,.18f),book,blackBook);
                    foreach(float side in new[]{-2.5f,2.5f})Box("Archive cabinet pier",new Vector3(x+side,3.8f,-18),new Vector3(.22f,7,.5f),marble,night);
                    for(int shelf=0;shelf<5;shelf++){
                        float y=.6f+shelf*1.35f;Box("Archive shelf lip",new Vector3(x,y,-17.85f),new Vector3(5,.14f,.85f),silver,metal);
                        for(int j=0;j<7;j++){
                            float height=.65f+(j%3)*.15f;Box("Catalogued folio",new Vector3(x-2+j*.64f,y+height*.5f+.07f,-17.9f),new Vector3(.42f,height,.42f),j%3==0?marble:book,blackBook);
                        }
                        Glyph(new Vector3(x,y+.58f,-17.64f),"I  II  III   IV   V  VI  VII",.028f,Quaternion.Euler(0,180,0));
                    }
                }
                // Inscriptions face the approach to the orrery and the existing path-side shelves.
                Glyph(new Vector3(0,7.65f,-17.7f),"L U M I N O S I T Y   /   U M B R A",.035f,Quaternion.Euler(0,180,0));
                for(int side=-1;side<=1;side+=2)foreach(float z in new[]{-1f,3f,13f,18f})
                    Glyph(new Vector3(side*5.04f,2.6f,z),"I  II  III  V  VIII",.022f,Quaternion.Euler(0,side<0?-90:90,0));
                for(int i=0;i<12;i++){
                    float a=i*Mathf.PI/6;var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Orrery satellite";g.transform.SetParent(root,false);g.transform.position=new Vector3(Mathf.Cos(a)*3.5f,7+Mathf.Sin(a)*2.8f,-10);g.transform.localScale=Vector3.one*(i%3==0?.32f:.15f);Object.DestroyImmediate(g.GetComponent<Collider>());Tint(g.GetComponent<Renderer>(),whiteRune,violet);
                }
            }
            var settings=new SerializedObject(manager);settings.FindProperty("whiteAmbient").colorValue=new Color(.69f,.73f,.8f);settings.FindProperty("whiteIntensity").floatValue=.85f;settings.FindProperty("blackAmbient").colorValue=new Color(.32f,.28f,.44f);settings.FindProperty("blackFogDensity").floatValue=.012f;settings.ApplyModifiedPropertiesWithoutUndo();
            foreach(var text in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
                if(text.name=="Archive inscription") text.characterSize=text.text.StartsWith("L U")?.065f:.055f;
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            foreach(var m in new[]{whiteRune,violet}){m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;m.EnableKeyword("_EMISSION");EditorUtility.SetDirty(m);}AssetDatabase.SaveAssets();
        }
    }
}
