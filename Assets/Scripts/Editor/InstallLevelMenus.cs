using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Paradox.Editor
{
    public static class InstallLevelMenus
    {
        static Material silver,navy,cyan; static Mesh ring;
        public static void Apply()
        {
            silver=Mat("TransferSilver",new Color(.83f,.88f,.92f),.65f);navy=Mat("TransferNavy",new Color(.025f,.04f,.065f),.5f);cyan=Mat("TransferCyan",new Color(.05f,.7f,.85f),.4f);cyan.EnableKeyword("_EMISSION");cyan.SetColor("_EmissionColor",new Color(.02f,.65f,.8f));
            ring=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/MainMenu/TransferRing.asset");if(!ring){ring=Ring();AssetDatabase.CreateAsset(ring,"Assets/Art/MainMenu/TransferRing.asset");}
            foreach(var name in MainMenu.LevelScenes)
            {
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
                var systems=Object.FindFirstObjectByType<GameFlow>();var menus=systems.GetComponent<LevelMenus>();if(!menus)menus=systems.gameObject.AddComponent<LevelMenus>();menus.font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/MainMenu/TitleSerif.ttf");
                var gun=Object.FindFirstObjectByType<TransferGun>();var so=new SerializedObject(gun);var model=(GameObject)so.FindProperty("model").objectReferenceValue;
                if(!model.transform.Find("Precision transfer assembly"))
                {
                    for(int i=model.transform.childCount-1;i>=0;i--)Object.DestroyImmediate(model.transform.GetChild(i).gameObject);
                    var root=new GameObject("Precision transfer assembly").transform;root.SetParent(model.transform,false);
                    Part(root,"Dark receiver",new Vector3(0,.05f,.12f),new Vector3(.14f,.16f,.42f),navy);
                    Part(root,"Long internal rail",new Vector3(0,.08f,.44f),new Vector3(.085f,.08f,.43f),navy);
                    foreach(float x in new[]{-.075f,.075f}){
                        Part(root,"Silver armour",new Vector3(x,.08f,.13f),new Vector3(.035f,.145f,.32f),silver);
                        Part(root,"Elongated casing",new Vector3(x*.65f,.08f,.44f),new Vector3(.025f,.1f,.4f),silver);
                        Part(root,"Cyan energy channel",new Vector3(x*.68f,.135f,.43f),new Vector3(.014f,.014f,.43f),cyan);
                    }
                    Part(root,"Upper silver spine",new Vector3(0,.15f,.1f),new Vector3(.13f,.025f,.32f),silver);
                    var grip=Part(root,"Angled grip",new Vector3(0,-.11f,.025f),new Vector3(.115f,.26f,.1f),navy);grip.localRotation=Quaternion.Euler(-18,0,0);
                    var cover=Part(root,"Grip silver back",new Vector3(0,-.115f,-.025f),new Vector3(.12f,.22f,.025f),silver);cover.localRotation=grip.localRotation;
                    Part(root,"Grip indicator",new Vector3(.061f,-.08f,.02f),new Vector3(.008f,.08f,.018f),cyan);
                    foreach(float z in new[]{.63f,.67f})RingPart(root,"Cyan muzzle ring",new Vector3(0,.08f,z),.065f,Quaternion.identity);
                    RingPart(root,"Rear side energy dial",new Vector3(.096f,.07f,-.005f),.055f,Quaternion.Euler(0,90,0));
                }
                if(name=="Level01") { var board=GameObject.Find("Garden controls tablet"); if(board) Object.DestroyImmediate(board); Board(); }
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
        static Material Mat(string name,Color color,float metallic){string path="Assets/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",.65f);return m;}
        static Transform Part(Transform root,string name,Vector3 position,Vector3 size,Material material){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root,false);g.transform.localPosition=position;g.transform.localScale=size;Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=material;g.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return g.transform;}
        static void RingPart(Transform root,string name,Vector3 p,float scale,Quaternion q){var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=Vector3.one*scale;g.transform.localRotation=q;g.GetComponent<MeshFilter>().sharedMesh=ring;g.GetComponent<Renderer>().sharedMaterial=cyan;}
        static Mesh Ring(){var m=new Mesh{name="Emitter torus"};var v=new Vector3[32*8];var t=new int[32*8*6];for(int i=0;i<32;i++)for(int j=0;j<8;j++){float a=i*Mathf.PI/16,b=j*Mathf.PI/4;v[i*8+j]=new Vector3(Mathf.Cos(a)*(1+.12f*Mathf.Cos(b)),Mathf.Sin(a)*(1+.12f*Mathf.Cos(b)),.12f*Mathf.Sin(b));int k=(i*8+j)*6,n=(i+1)%32*8, jj=(j+1)%8;t[k]=i*8+j;t[k+1]=n+j;t[k+2]=n+jj;t[k+3]=i*8+j;t[k+4]=n+jj;t[k+5]=i*8+jj;}m.vertices=v;m.triangles=t;m.RecalculateNormals();return m;}
        static void Board()
        {
            var spawn=GameObject.Find("Player Spawn").transform;var root=new GameObject("Garden controls tablet").transform;
            root.position=spawn.position+spawn.right*2.4f+spawn.forward*2.8f+Vector3.up*1.25f;root.rotation=spawn.rotation;
            var stone=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PaleStone.mat");
            Part(root,"Marble tablet",Vector3.zero,new Vector3(2.25f,1.65f,.12f),stone);
            Part(root,"Gold header",new Vector3(0,.79f,-.075f),new Vector3(2.15f,.035f,.02f),AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WarmTrim.mat"));
            var text=new GameObject("Actual control bindings",typeof(TextMesh)).GetComponent<TextMesh>();text.transform.SetParent(root,false);text.transform.localPosition=new Vector3(0,0,-.075f);text.transform.localRotation=Quaternion.identity;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.fontSize=64;text.characterSize=.021f;text.color=new Color(.06f,.07f,.13f);
            text.text="GARDEN OF ECHOES\n\nWASD / Arrows — Move\nMouse — Look     Space — Jump\nE — Pick up / Place / Equip gun\nLeft click — Transfer aimed cube\nQ — Switch dimension\nR — Restart     Esc — Pause";
        }
    }
}
