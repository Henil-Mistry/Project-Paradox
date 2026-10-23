using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Paradox.Editor {
public static class WorldPropPass {
static DimensionManager manager;static Material white,black,gold,blue,glassW,glassB,coreW,coreB;static Mesh crystal;
static Material Load(string folder,string name)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/"+folder+"/"+name+".mat");
static GameObject Part(Transform root,string name,Vector3 p,Vector3 s,PrimitiveType shape,Material w,Material b){var g=GameObject.CreatePrimitive(shape);Object.DestroyImmediate(g.GetComponent<Collider>());g.name=name;g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=w;g.AddComponent<DimensionTint>().Configure(manager,w,b);return g;}
static void Shard(Transform root,Vector3 p,float h){var g=Part(root,"Rooted crystal",p,new Vector3(.3f,h,.3f),PrimitiveType.Cube,coreW,coreB);g.GetComponent<MeshFilter>().sharedMesh=crystal;}
static void Ring(Transform root,Vector3 centre,float radius,bool horizontal=false){for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;Vector3 x=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*radius,y=new Vector3(Mathf.Cos(b),Mathf.Sin(b),0)*radius;if(horizontal){x=new Vector3(x.x,0,x.y);y=new Vector3(y.x,0,y.y);}var g=Part(root,"Engraved celestial ring",centre+(x+y)/2,new Vector3(.035f,(y-x).magnitude,.035f),PrimitiveType.Cube,gold,blue);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,y-x);}}
public static void Apply(){white=Load("Garden","Ivory marble");black=Load("Garden","Midnight stone");gold=Load("Garden","Brushed gold");blue=Load("GardenProps","Puzzle core Black");glassW=Load("GardenProps","Puzzle glass White");glassB=Load("GardenProps","Puzzle glass Black");coreW=Load("GardenProps","Puzzle core White");coreB=blue;crystal=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/GardenProps/Crystal.asset");
string[] scenes={"Level02_Library","Level03_MirrorLake","Level04_Observatory","Level05_Heart"};
for(int level=0;level<4;level++){
var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+scenes[level]+".unity");if(GameObject.Find("World discoveries"))continue;manager=Object.FindFirstObjectByType<DimensionManager>();var root=new GameObject("World discoveries").transform;
var cube=Object.FindFirstObjectByType<TransferableObject>();var tint=cube.GetComponent<DimensionTint>();if(!tint)tint=cube.gameObject.AddComponent<DimensionTint>();tint.Configure(manager,glassW,glassB);cube.GetComponent<Renderer>().sharedMaterial=glassW;
var inlay=new GameObject("Puzzle crystal inlay").transform;inlay.SetParent(cube.transform,false);var gem=Part(inlay,"Suspended puzzle core",Vector3.zero,new Vector3(.38f,.58f,.38f),PrimitiveType.Cube,coreW,coreB);gem.GetComponent<MeshFilter>().sharedMesh=crystal;
for(int axis=0;axis<3;axis++)for(int a=-1;a<=1;a+=2)for(int b=-1;b<=1;b+=2){Vector3 p=Vector3.zero,s=Vector3.one*.024f;p[(axis+1)%3]=a*.475f;p[(axis+2)%3]=b*.475f;s[axis]=.97f;Part(inlay,"Puzzle gilt edge",p,s,PrimitiveType.Cube,gold,coreW);}
// Off-route side alcoves and far-side corners, never the centre bridge, plate or retrieval site.
Vector3[] points={new Vector3(-5.8f,0,-18),new Vector3(5.7f,0,-17),new Vector3(-12,0,14),new Vector3(12,0,18)};
for(int j=0;j<points.Length;j++){
var group=new GameObject(level==0?"Archive curiosities":level==1?"Ice reliquary":level==2?"Astronomical instrument":"Fractured shrine").transform;group.SetParent(root,false);group.position=points[j];group.localRotation=Quaternion.Euler(0,j*43+12,0);
Part(group,"Stone plinth",Vector3.up*.15f,new Vector3(1.1f,.15f,.85f),PrimitiveType.Cylinder,white,black);
if(level==0){for(int k=0;k<4;k++){var book=Part(group,"Bound archive volume",new Vector3(.06f*k,.38f+k*.17f,0),new Vector3(.8f,.14f,.52f),PrimitiveType.Cube,white,black);book.transform.localRotation=Quaternion.Euler(0,k*13,0);Part(group,"Book spine inlay",new Vector3(.06f*k,.38f+k*.17f,-.27f),new Vector3(.6f,.025f,.02f),PrimitiveType.Cube,gold,blue);}Part(group,"Reading lamp stem",new Vector3(.65f,.65f,0),new Vector3(.08f,.5f,.08f),PrimitiveType.Cylinder,gold,black);Shard(group,new Vector3(.65f,1.25f,0),.4f);}
else if(level==1){for(int k=0;k<3;k++){Shard(group,new Vector3((k-1)*.32f,.55f+k*.12f,0),.75f+k*.23f);}Ring(group,new Vector3(0,.32f,0),.65f,true);}
else if(level==2){Part(group,"Instrument support",Vector3.up*.7f,new Vector3(.2f,.5f,.2f),PrimitiveType.Cylinder,white,black);Ring(group,Vector3.up*1.5f,.65f);Ring(group,Vector3.up*1.5f,.45f,true);Part(group,"Instrument orb",Vector3.up*1.5f,Vector3.one*.25f,PrimitiveType.Sphere,gold,blue);for(int k=0;k<8;k++){float a=k*Mathf.PI/4;Part(group,"Dial index",new Vector3(Mathf.Cos(a)*.6f,1.5f+Mathf.Sin(a)*.6f,0),Vector3.one*.08f,PrimitiveType.Cube,gold,blue);}}
else{Ring(group,Vector3.up*.32f,.7f,true);for(int k=0;k<3;k++){Shard(group,new Vector3((k-1)*.38f,.8f+k*.3f,0),.8f);Part(group,"Broken marble fragment",new Vector3((k-1)*.5f,.45f, .4f),new Vector3(.25f,.5f,.25f),PrimitiveType.Cube,white,black).transform.localRotation=Quaternion.Euler(15,k*30,20);}}
}
// Stable, non-interactive library seal; existing letter glitches remain untouched.
if(level==0){var seal=new GameObject("Stable archive seal").transform;seal.SetParent(root,false);seal.position=new Vector3(-10,3,20.5f);Ring(seal,Vector3.zero,1);for(int i=0;i<4;i++){var stroke=Part(seal,"Archive diamond",Vector3.zero,new Vector3(.04f,1.3f,.04f),PrimitiveType.Cube,gold,blue);stroke.transform.localRotation=Quaternion.Euler(0,0,45+i*90);stroke.transform.localPosition=new Vector3(i%2==0?.45f:-.45f,i<2?.45f:-.45f,0);}}
EditorSceneManager.SaveScene(scene);
}AssetDatabase.SaveAssets();}
}}
