using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Paradox.Editor
{
 public static class WorldVisualPass
 {
  static DimensionManager manager; static Transform root; static Material white,black,gold,glow;
  static GameObject Shape(string name,Vector3 p,Vector3 scale,PrimitiveType type, bool trim=false)
  {var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=scale;Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=trim?gold:white;g.AddComponent<DimensionTint>().Configure(manager,trim?gold:white,trim?glow:black);return g;}
  static void Ring(Vector3 p,float radius,Quaternion rotation)
  {for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Vector3 x=p+rotation*new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*radius,y=p+rotation*new Vector3(Mathf.Cos(b),Mathf.Sin(b),0)*radius;var g=Shape("Celestial ring",(x+y)/2,new Vector3(.06f,(y-x).magnitude/2,.06f),PrimitiveType.Cylinder,true);g.transform.up=y-x;}}
  static void Pillar(Vector3 p,float h){Shape("Fluted pillar",p+Vector3.up*h/2,new Vector3(.8f,h/2,.8f),PrimitiveType.Cylinder);foreach(float y in new[]{.2f,h-.2f})Shape("Gilt capital",p+Vector3.up*y,new Vector3(1.2f,.15f,1.2f),PrimitiveType.Cylinder,true);}
  public static void Apply()
  {
   white=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Garden/Ivory marble.mat");black=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Garden/Midnight stone.mat");gold=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Garden/Brushed gold.mat");glow=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Garden/Astral blue.mat");
   string[] names={"Level02_Library","Level03_MirrorLake","Level04_Observatory","Level05_Heart"};
   for(int level=0;level<4;level++)
   {
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+names[level]+".unity");if(GameObject.Find("Celestial visual dressing"))continue;manager=Object.FindFirstObjectByType<DimensionManager>();root=new GameObject("Celestial visual dressing").transform;
    foreach(var tint in Object.FindObjectsByType<DimensionTint>(FindObjectsSortMode.None)){if(tint.name.Contains("Cube")||tint.name.Contains("Plate"))continue;tint.Configure(manager,white,black);tint.GetComponent<Renderer>().sharedMaterial=white;}
    for(int side=-1;side<=1;side+=2)for(int z=-16;z<=20;z+=12)Pillar(new Vector3(side*17,0,z),level==0?9:12);
    Vector3 centre=new Vector3(0,level==0?7:9,level==0?-10:19);
    for(int i=0;i<3;i++)Ring(centre,3.2f+i*.45f,Quaternion.Euler(i*55,i*30,0));
    if(level==0)
    {
     Shape("Archive orrery sun",centre,Vector3.one*1.4f,PrimitiveType.Sphere,true);
     for(int side=-1;side<=1;side+=2)for(int row=0;row<3;row++)for(int book=0;book<12;book++)Shape("Archive volume",new Vector3(side*(15+row*2.2f),1.4f+(book%4)*1.4f,-12+(book/4)*2),new Vector3(.6f,1,.65f),PrimitiveType.Cube,book%3==0);
    }
    if(level==1)
    {for(int side=-1;side<=1;side+=2){Pillar(new Vector3(side*5,0,24),16);for(int i=0;i<5;i++){var c=Shape("Suspended palace crystal",new Vector3(side*(8+i*2),6+i,18),new Vector3(.6f,2.5f,.6f),PrimitiveType.Cube,true);c.transform.rotation=Quaternion.Euler(0,45,20);}}Shape("Palace orb",centre,Vector3.one*1.7f,PrimitiveType.Sphere,true);}
    if(level==2)
    {for(int i=0;i<24;i++){float a=i*Mathf.PI/12;Shape("Clock index",new Vector3(Mathf.Cos(a)*3.7f,8+Mathf.Sin(a)*3.7f,19.7f),new Vector3(.12f,.35f,.12f),PrimitiveType.Cube,true);}Ring(new Vector3(-9,16,28),5,Quaternion.identity);Shape("Eclipse silhouette",new Vector3(-8.2f,16,27.8f),new Vector3(9,9,.2f),PrimitiveType.Sphere);}
    if(level==3)
    {foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))if(t.name=="Heart core segment"){var tint=t.GetComponent<DimensionTint>();if(!tint)tint=t.gameObject.AddComponent<DimensionTint>();tint.Configure(manager,gold,glow);t.GetComponent<Renderer>().sharedMaterial=gold;}for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var shard=Shape("Heart satellite shard",new Vector3(Mathf.Cos(a)*4,7+Mathf.Sin(a)*3,16.8f),new Vector3(.4f,.9f,.4f),PrimitiveType.Cube,true);shard.transform.rotation=Quaternion.Euler(20,i*30,30);}}
    var settings=new SerializedObject(manager);settings.FindProperty("whiteFog").colorValue=new Color(.72f,.81f,.91f);settings.FindProperty("blackFog").colorValue=new Color(.035f,.025f,.09f);settings.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.SaveScene(scene);
   }
   AssetDatabase.SaveAssets();
  }
 }
}
