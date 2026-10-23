using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace Paradox.Editor
{
    // Authored visual dressing only. Existing transforms, collision and puzzle references are retained.
    public static class GardenVisualPass
    {
        const string Folder = "Assets/Art/Garden";
        static DimensionManager manager;
        static Transform root;
        static Material marble, night, gold, blue, bark, darkBark, pink, violet, leaves, luminous, cloud;
        static System.Random random;
        static float R(float a,float b) => a+(float)random.NextDouble()*(b-a);
        [MenuItem("Paradox/Apply Garden Visuals Once")]
        public static void Apply()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Level01.unity");
            if(GameObject.Find("Garden of Echoes Art")) throw new Exception("Garden art already installed");
            System.IO.Directory.CreateDirectory(Folder); AssetDatabase.Refresh(); random=new System.Random(418);
            manager=UnityEngine.Object.FindFirstObjectByType<DimensionManager>();
            marble=Mat("Ivory marble",new Color(.88f,.85f,.79f),Color.black,.28f);
            night=Mat("Midnight stone",new Color(.055f,.075f,.15f),new Color(.008f,.012f,.028f),.5f);
            gold=Mat("Brushed gold",new Color(.72f,.48f,.15f),new Color(.15f,.075f,.014f),.65f);
            blue=Mat("Astral blue",new Color(.14f,.35f,.7f),new Color(.24f,.65f,1.8f),.55f);
            bark=Mat("Rosewood",new Color(.25f,.14f,.15f),Color.black,.15f);
            darkBark=Mat("Indigo bark",new Color(.055f,.035f,.105f),new Color(.035f,.015f,.07f),.2f);
            pink=Mat("Blossom pink",new Color(.98f,.58f,.7f),new Color(.08f,.025f,.03f),.18f);
            violet=Mat("Starlit blossom",new Color(.4f,.12f,.75f),new Color(.55f,.12f,1.1f),.25f);
            leaves=Mat("Sage foliage",new Color(.17f,.3f,.19f),Color.black,.1f);
            luminous=Mat("Luminous foliage",new Color(.13f,.19f,.42f),new Color(.07f,.15f,.4f),.2f);
            cloud=Mat("Cloud ivory",new Color(.8f,.86f,.95f),new Color(.12f,.14f,.19f),0);
            // World-space-scale stone veining; the same small texture is reused on both materials.
            var texture=new Texture2D(128,128,TextureFormat.RGB24,false);texture.wrapMode=TextureWrapMode.Repeat;
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                float n=Mathf.PerlinNoise(x*.07f,y*.07f);float vein=Mathf.Pow(Mathf.Abs(Mathf.Sin(x*.14f+y*.08f+n*8)),24);
                texture.SetPixel(x,y,Color.white*(.94f-vein*.11f-R(0,.025f)));
            }
            texture.Apply();AssetDatabase.CreateAsset(texture,Folder+"/MarbleVeins.asset");marble.mainTexture=texture;night.mainTexture=texture;
            root=new GameObject("Garden of Echoes Art").transform;
            foreach(var tint in UnityEngine.Object.FindObjectsByType<DimensionTint>(FindObjectsSortMode.None))
            {
                string n=tint.name;
                if(n=="Cube" || n.Contains("Glyph") || n.Contains("flower") || n.Contains("Flower"))continue;
                Material a=marble,b=night;
                if(n=="Walking path" || n.Contains("flower bed") || n.Contains("planter")){a=leaves;b=luminous;}
                if(n=="Garden tree trunk"){a=bark;b=darkBark;}
                if(n=="Garden tree crown"){a=pink;b=violet;}
                tint.Configure(manager,a,b);tint.GetComponent<Renderer>().sharedMaterial=a;
            }
            // Replace only the visible sphere crowns with branching, clustered blossom silhouettes.
            foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(t.name=="Garden tree crown") {t.GetComponent<Renderer>().enabled=false;t.GetComponent<DimensionTint>().enabled=false;}
            var tree=TreePrefab();
            foreach(var point in new[]{new Vector3(-13,0,-7),new Vector3(13,0,-7),new Vector3(-7,0,-15),new Vector3(7,0,-15),new Vector3(-12,0,16),new Vector3(12,0,16)})
            {var instance=(GameObject)PrefabUtility.InstantiatePrefab(tree);instance.transform.SetParent(root);instance.transform.position=point;WireTree(instance);}
            // Two candidate cube sites share this landmark; its canopy stays well above retrieval routes.
            var keyTree=(GameObject)PrefabUtility.InstantiatePrefab(tree);keyTree.name="Key grove landmark";keyTree.transform.SetParent(root);keyTree.transform.position=new Vector3(-12,0,-13);WireTree(keyTree);
            // Flower clusters remain inside existing planting beds, away from interaction rays.
            foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(t.name=="Garden planter" || t.name=="Separated flower bed")
                    for(int i=0;i<7;i++)
                    {
                        Vector3 p=t.position+new Vector3(R(-.5f,.5f),.35f,R(-.6f,.6f));
                        Segment("Flower stem",p,p+Vector3.up*.35f,.025f,leaves,luminous);
                        Shape("Blossom",PrimitiveType.Sphere,p+Vector3.up*.35f,new Vector3(.2f,.1f,.2f),pink,violet);
                    }
            // Marble joints and paired inlays make the existing walkable slabs legible.
            for(int z=-5;z<25;z+=2)
            {
                if(z>=5 && z<11)continue;
                Segment("Marble joint",new Vector3(-7.4f,.012f,z),new Vector3(7.4f,.012f,z),.012f,gold,blue);
            }
            foreach(int side in new[]{-1,1})
            {
                for(int z=-18;z<=20;z+=8)Column(new Vector3(side*17,0,z),R(6,10));
                Column(new Vector3(side*2.2f,0,20.9f),6);
                for(int z=-16;z<21;z+=4)
                {
                    var p=new Vector3(side*18,-3,z);
                    Island(p,new Vector3(R(2,4),R(4,8),R(2,4)));
                }
                // Vines descend beneath the original gap lips, never bridging the gap.
                for(int x=-7;x<=7;x+=2)
                {
                    float z=side<0?4.85f:11.15f;Vector3 p=new Vector3(x,-.15f,z);
                    for(int i=0;i<5;i++)
                    {Vector3 q=p+new Vector3(Mathf.Sin(i)*.2f,-i*.45f,0);Shape("Hanging ivy",PrimitiveType.Sphere,q,new Vector3(.3f,.35f,.14f),leaves,luminous);}
                }
            }
            Arch(new Vector3(0,3.8f,21),4.4f,marble,night);
            Arch(new Vector3(0,3.8f,20.88f),4.05f,gold,blue);
            // Gate tracery is a child of the existing moving, dimension-configured door.
            Transform gate=UnityEngine.Object.FindFirstObjectByType<Door>().transform;
            var gateArt=new GameObject("Gate filigree").transform;gateArt.SetParent(root);
            for(int side=-1;side<=1;side+=2)
            {
                for(int i=0;i<4;i++)Segment("Gilded gate rib",new Vector3(side*(.23f+i*.3f),.15f,20.76f),new Vector3(side*(.23f+i*.3f),3.05f,20.76f),.028f,gold,blue,gateArt);
                for(int i=0;i<3;i++)Ring(new Vector3(side*.75f, .65f+i*.85f,20.72f),.33f,gold,blue,gateArt);
            }
            Ring(new Vector3(0,1.9f,20.65f),.48f,gold,blue,gateArt);
            Shape("Keyhole",PrimitiveType.Sphere,new Vector3(0,2.02f,20.61f),new Vector3(.19f,.19f,.06f),gold,blue,gateArt);
            Shape("Keyhole stem",PrimitiveType.Cube,new Vector3(0,1.82f,20.61f),new Vector3(.12f,.25f,.05f),gold,blue,gateArt);
            gateArt.SetParent(gate,true);
            // Surface treatment only: bridge collision and membership are untouched.
            var bridge=GameObject.Find("Black Bridge");bridge.GetComponent<Renderer>().sharedMaterial=night;
            var bridgeArt=new GameObject("Constellation bridge").transform;bridgeArt.SetParent(root);
            for(int i=0;i<=6;i++)
            {
                float z=5+i;foreach(int side in new[]{-1,1})
                {
                    Vector3 p=new Vector3(side*1.15f,.04f,z);
                    Shape("Star node",PrimitiveType.Sphere,p,Vector3.one*.12f,blue,blue,bridgeArt);
                    if(i<6){Segment("Star thread",p,new Vector3(-side*1.15f,.04f,z+1),.018f,blue,blue,bridgeArt);Segment("Bridge rim",p,p+Vector3.forward,.025f,blue,blue,bridgeArt);}
                }
            }
            bridgeArt.gameObject.AddComponent<DimensionObject>().Configure(manager,DimensionMembership.Black);
            // Golden emblem belongs to the actual randomized transferable object, not a second objective.
            var cube=UnityEngine.Object.FindFirstObjectByType<TransferableObject>();
            var emblem=new GameObject("Golden key emblem").transform;emblem.SetParent(root);
            Vector3 origin=cube.transform.position;
            Ring(origin+new Vector3(0,.14f,-.415f),.11f,gold,gold,emblem);
            Segment("Key shaft",origin+new Vector3(0,.04f,-.415f),origin+new Vector3(0,-.23f,-.415f),.035f,gold,gold,emblem);
            Segment("Key tooth",origin+new Vector3(0,-.18f,-.415f),origin+new Vector3(.1f,-.18f,-.415f),.035f,gold,gold,emblem);
            emblem.SetParent(cube.transform,true);
            for(int i=0;i<15;i++)
            {
                float angle=i*Mathf.PI*2/15;Vector3 p=new Vector3(Mathf.Cos(angle)*R(35,65),R(-10,14),Mathf.Sin(angle)*R(35,65)+5);
                Island(p,new Vector3(R(4,8),R(8,16),R(4,8)));Column(p+Vector3.up*.5f,R(5,13));
                Shape("Cloud bank",PrimitiveType.Sphere,p+Vector3.down*8,new Vector3(17,3,10),cloud,night);
            }
            Particles("Drifting petals",DimensionMembership.White,pink,45,.13f,new Vector3(0,6,-4));
            Particles("Astral motes",DimensionMembership.Black,blue,65,.065f,new Vector3(0,5,6));
            var serialized=new SerializedObject(manager);
            SetColor(serialized,"whiteAmbient",new Color(.62f,.65f,.7f));SetColor(serialized,"whiteFog",new Color(.74f,.82f,.91f));
            SetColor(serialized,"whiteSun",new Color(1,.89f,.79f));serialized.FindProperty("whiteIntensity").floatValue=1.05f;
            SetColor(serialized,"blackAmbient",new Color(.20f,.22f,.38f));SetColor(serialized,"blackFog",new Color(.025f,.025f,.085f));
            serialized.FindProperty("blackFogDensity").floatValue=.012f;serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("GARDEN VISUAL PASS saved; no gameplay transforms or colliders modified.");
        }
        static void SetColor(SerializedObject obj,string name,Color color){obj.FindProperty(name).colorValue=color;}
        static Material Mat(string name,Color color,Color emission,float smooth)
        {
            var m=new Material(Shader.Find("Standard"));m.name=name;m.color=color;m.SetFloat("_Glossiness",smooth);
            if(emission.maxColorComponent>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);}
            AssetDatabase.CreateAsset(m,Folder+"/"+name+".mat");return m;
        }
        static GameObject Shape(string name,PrimitiveType type,Vector3 position,Vector3 scale,Material white,Material black,Transform parent=null)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent?parent:root,true);go.transform.position=position;go.transform.localScale=scale;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=white;
            go.AddComponent<DimensionTint>().Configure(manager,white,black);return go;
        }
        static void Segment(string name,Vector3 a,Vector3 b,float width,Material white,Material black,Transform parent=null)
        {var go=Shape(name,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(width,(a-b).magnitude*.5f,width),white,black,parent);go.transform.up=b-a;}
        static void Ring(Vector3 centre,float radius,Material white,Material black,Transform parent=null)
        {for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;Segment("Ornament",centre+new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*radius,centre+new Vector3(Mathf.Cos(b),Mathf.Sin(b),0)*radius,.025f,white,black,parent);}}
        static void Arch(Vector3 centre,float width,Material white,Material black)
        {for(int i=0;i<20;i++){float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;Segment("Garden arch",centre+new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*width*.5f,centre+new Vector3(Mathf.Cos(b),Mathf.Sin(b),0)*width*.5f,.16f,white,black);}}
        static void Column(Vector3 p,float height)
        {
            Shape("Ruined column",PrimitiveType.Cylinder,p+Vector3.up*height*.5f,new Vector3(.65f,height*.5f,.65f),marble,night);
            foreach(float y in new[]{.15f,height-.15f})Shape("Column capital",PrimitiveType.Cylinder,p+Vector3.up*y,new Vector3(1,.15f,1),marble,night);
            Segment("Column inlay",p+new Vector3(0,1,-.34f),p+new Vector3(0,height-1,-.34f),.025f,gold,blue);
            Ring(p+new Vector3(0,height*.65f,-.35f),.2f,gold,blue);
        }
        static void Island(Vector3 p,Vector3 scale)
        {var go=Shape("Floating stone shard",PrimitiveType.Cube,p-Vector3.up*scale.y*.5f,scale,marble,night);go.transform.rotation=Quaternion.Euler(8,R(0,180),14);}
        static GameObject TreePrefab()
        {
            var tree=new GameObject("Blossom tree");tree.transform.SetParent(root);
            Segment("Branch",Vector3.zero,new Vector3(.2f,3.2f,0),.3f,bark,darkBark,tree.transform);
            for(int i=0;i<7;i++)
            {
                float angle=i*2.4f;Vector3 end=new Vector3(Mathf.Cos(angle)*R(1.2f,2.4f),R(3.7f,5.2f),Mathf.Sin(angle)*R(1.2f,2.4f));
                Segment("Branch",new Vector3(.15f,2.1f,0),end,.13f,bark,darkBark,tree.transform);
                for(int j=0;j<4;j++)Shape("Blossom crown",PrimitiveType.Sphere,end+new Vector3(R(-.65f,.65f),R(-.3f,.4f),R(-.65f,.65f)),new Vector3(R(1.2f,2),R(.6f,1),R(1,1.7f)),pink,violet,tree.transform);
            }
            // Prefab cannot reference a scene manager. Instances are wired to the existing manager below.
            foreach(var tint in tree.GetComponentsInChildren<DimensionTint>())UnityEngine.Object.DestroyImmediate(tint);
            var prefab=PrefabUtility.SaveAsPrefabAsset(tree,Folder+"/BlossomTree.prefab");UnityEngine.Object.DestroyImmediate(tree);return prefab;
        }
        static void WireTree(GameObject tree)
        {foreach(var r in tree.GetComponentsInChildren<Renderer>())r.gameObject.AddComponent<DimensionTint>().Configure(manager,r.sharedMaterial,r.sharedMaterial==bark?darkBark:violet);}
        static void Particles(string name,DimensionMembership membership,Material material,int count,float size,Vector3 position)
        {
            var go=new GameObject(name);go.transform.SetParent(root);go.transform.position=position;var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.maxParticles=count;main.startLifetime=12;main.startSpeed=.12f;main.startSize=size;main.simulationSpace=ParticleSystemSimulationSpace.World;main.prewarm=true;
            var emission=ps.emission;emission.rateOverTime=count/12f;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(28,7,32);
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=.15f;velocity.y=-.08f;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial=material;
            go.AddComponent<DimensionObject>().Configure(manager,membership);
        }
    }
}
