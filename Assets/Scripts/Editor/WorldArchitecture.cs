using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Paradox.Editor {
public static class WorldArchitecture {
static DimensionManager manager; static Transform root; static Material marble,dark,gold,glow,ice;
static GameObject Shape(string n,Vector3 p,Vector3 s,PrimitiveType type,Material w,Material b){var g=GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=w;g.AddComponent<DimensionTint>().Configure(manager,w,b);return g;}
static void Beam(Vector3 a,Vector3 b,float width,Material w,Material d){var g=Shape("Architectural molding",(a+b)/2,new Vector3(width,(b-a).magnitude,width),PrimitiveType.Cube,w,d);g.transform.up=b-a;}
static void Arc(Vector3 c,float r,float start,float end,Material w,Material d){for(int i=0;i<16;i++){float a=Mathf.Lerp(start,end,i/16f)*Mathf.Deg2Rad,b=Mathf.Lerp(start,end,(i+1)/16f)*Mathf.Deg2Rad;Beam(c+new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*r,c+new Vector3(Mathf.Cos(b),Mathf.Sin(b),0)*r,.18f,w,d);}}
public static void Apply(){marble=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Garden/Ivory marble.mat");dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Garden/Midnight stone.mat");gold=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Garden/Brushed gold.mat");glow=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Celestial/Violet crystal.mat");ice=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Celestial/Glacial glass.mat");
foreach(string name in new[]{"Level02_Library","Level03_MirrorLake","Level04_Observatory","Level05_Heart"}){
var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");if(GameObject.Find("Celestial architecture"))continue;manager=Object.FindFirstObjectByType<DimensionManager>();root=new GameObject("Celestial architecture").transform;
// Frames sit beyond the rear boundary; they cannot conceal the traversable gap.
for(int i=-1;i<=1;i++){float x=i*9;Beam(new Vector3(x-3,0,28),new Vector3(x-3,10,28),.65f,marble,dark);Beam(new Vector3(x+3,0,28),new Vector3(x+3,10,28),.65f,marble,dark);Arc(new Vector3(x,10,28),3,0,180,marble,dark);Arc(new Vector3(x,10,27.6f),2.7f,0,180,gold,glow);}
if(name.Contains("Library")){
// Frame the already placed volumes as archive cabinets.
for(int side=-1;side<=1;side+=2){for(int k=0;k<4;k++)Beam(new Vector3(side*(14.5f+k*2),0,-13),new Vector3(side*(14.5f+k*2),7,-13),.22f,marble,glow);for(int k=0;k<5;k++)Beam(new Vector3(side*14.5f,k*1.5f,-13),new Vector3(side*20.5f,k*1.5f,-13),.18f,gold,glow);}
Arc(new Vector3(0,7,-9.8f),2.2f,55,305,gold,glow);
}
if(name.Contains("Lake")){for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++){float x=side*(7+i*2);Beam(new Vector3(x,1,24.8f),new Vector3(x,14,24.8f),.25f,gold,glow);Shape("Palace crystal crown",new Vector3(x,15,25),new Vector3(.8f,3,.8f),PrimitiveType.Cube,ice,glow).transform.rotation=Quaternion.Euler(0,45,25);}}
if(name.Contains("Observatory")){Arc(new Vector3(0,8,19.3f),4.1f,0,360,gold,glow);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Shape("Moon phase inset",new Vector3(Mathf.Cos(a)*3,8+Mathf.Sin(a)*3,19.2f),new Vector3(.5f,.5f,.08f),PrimitiveType.Sphere,gold,glow);}for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Beam(new Vector3(0,8,19),new Vector3(Mathf.Cos(a)*1.6f,8+Mathf.Sin(a)*1.6f,19),.12f,gold,glow);}Arc(new Vector3(-9,16,27.5f),5,50,310,gold,glow);}
if(name.Contains("Heart")){// Extra faceted lobes are visual only; existing calm/glitch behavior remains authoritative.
for(int i=0;i<7;i++){float x=(i-3)*.65f,y=7.2f+Mathf.Sin(i*Mathf.PI/6)*1.5f;var g=Shape("Fractured heart crystal",new Vector3(x,y,14.7f),new Vector3(.85f,1.4f,.65f),PrimitiveType.Cube,ice,glow);g.transform.rotation=Quaternion.Euler(20,45,i*19);g.AddComponent<VisualGlitch>().Configure(Object.FindFirstObjectByType<PuzzleManager>(),1.1f,.3f,.06f,0,false);}Arc(new Vector3(0,3.6f,25),2,0,360,gold,glow);Arc(new Vector3(0,3.6f,25),1.5f,0,360,gold,glow);}
// Reuse the garden's blossom mesh, without modifying its assets or scene.
if(name.Contains("Library")||name.Contains("Lake")){var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Garden/BlossomClusters.asset");for(int side=-1;side<=1;side+=2){var tree=new GameObject("Archive blossom canopy");tree.transform.SetParent(root);tree.transform.position=new Vector3(side*22,0,-7);tree.AddComponent<MeshFilter>().sharedMesh=mesh;tree.AddComponent<MeshRenderer>().sharedMaterial=marble;tree.AddComponent<DimensionTint>().Configure(manager,marble,glow);Beam(new Vector3(side*22,0,-7),new Vector3(side*22,4,-7),.4f,marble,dark);}}
var settings=new SerializedObject(manager);settings.FindProperty("whiteAmbient").colorValue=new Color(.62f,.68f,.76f);settings.FindProperty("whiteIntensity").floatValue=1.1f;settings.FindProperty("blackAmbient").colorValue=new Color(.24f,.21f,.38f);settings.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.SaveScene(scene);
}AssetDatabase.SaveAssets();}
}}
