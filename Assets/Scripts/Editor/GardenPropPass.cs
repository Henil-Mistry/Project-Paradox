using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace Paradox.Editor
{
    public static class GardenPropPass
    {
        const string Folder = "Assets/Art/GardenProps/";
        static DimensionManager manager;
        static Material ivory, night, pink, violet, gold, glassWhite, glassBlack, coreWhite, coreBlack;
        static Mesh crystal;
        static Material Existing(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Garden/" + name + ".mat");
        static Material Make(string name, Color color, Color emission, bool glass = false)
        {
            var material = new Material(Shader.Find("Standard")) { name = name, color = color, enableInstancing = true };
            material.SetFloat("_Glossiness", .82f);
            material.SetColor("_EmissionColor", emission);
            if (glass)
            {
                material.SetFloat("_Mode", 3);
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            AssetDatabase.CreateAsset(material, Folder + name + ".mat");
            return material;
        }
        static GameObject Part(Transform parent, string name, Vector3 position, Vector3 scale, PrimitiveType type, Material white, Material black)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = white;
            go.AddComponent<DimensionTint>().Configure(manager, white, black);
            return go;
        }
        static GameObject Shard(Transform parent, string name, Vector3 p, Vector3 s, Material w, Material b)
        {
            var go = Part(parent, name, p, s, PrimitiveType.Cube, w, b);
            go.GetComponent<MeshFilter>().sharedMesh = crystal;
            return go;
        }
        static Mesh CrystalMesh()
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3, b = (i + 1) * Mathf.PI / 3;
                var p = new Vector3(Mathf.Cos(a)*.5f, 0, Mathf.Sin(a)*.5f);
                var q = new Vector3(Mathf.Cos(b)*.5f, 0, Mathf.Sin(b)*.5f);
                int n = vertices.Count;
                vertices.AddRange(new[]{p,Vector3.up*.65f,q,q,Vector3.down*.35f,p});
                triangles.AddRange(new[]{n,n+1,n+2,n+3,n+4,n+5});
            }
            var mesh = new Mesh { name = "Six sided garden crystal" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, Folder + "Crystal.asset"); return mesh;
        }
        [MenuItem("Paradox/Apply Garden Props Once")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Level01.unity");
            if (GameObject.Find("Garden discoveries")) throw new System.InvalidOperationException("Garden props already installed.");
            System.IO.Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            manager = Object.FindFirstObjectByType<DimensionManager>();
            ivory=Existing("Ivory marble"); night=Existing("Midnight stone"); pink=Existing("Blossom pink"); violet=Existing("Starlit blossom"); gold=Existing("Brushed gold");
            glassWhite=Make("Puzzle glass White",new Color(.58f,.8f,.95f,.24f),new Color(.025f,.035f,.06f),true);
            glassBlack=Make("Puzzle glass Black",new Color(.3f,.24f,.65f,.32f),new Color(.025f,.025f,.08f),true);
            coreWhite=Make("Puzzle core White",new Color(.4f,.56f,.9f),new Color(.12f,.18f,.4f));
            coreBlack=Make("Puzzle core Black",new Color(.38f,.22f,.7f),new Color(.25f,.14f,.65f));
            crystal=CrystalMesh();
            // Keep the original renderer, identity, mesh dimensions and every physics component.
            var cube=Object.FindFirstObjectByType<TransferableObject>();
            var tint=cube.GetComponent<DimensionTint>(); if(!tint)tint=cube.gameObject.AddComponent<DimensionTint>();
            tint.Configure(manager,glassWhite,glassBlack); cube.GetComponent<Renderer>().sharedMaterial=glassWhite;
            var art=new GameObject("Puzzle crystal inlay").transform; art.SetParent(cube.transform,false);
            Shard(art,"Suspended puzzle core",Vector3.zero,new Vector3(.38f,.58f,.38f),coreWhite,coreBlack);
            for(int axis=0;axis<3;axis++) for(int a=-1;a<=1;a+=2) for(int b=-1;b<=1;b+=2)
            {
                Vector3 p=Vector3.zero, s=Vector3.one*.024f; p[(axis+1)%3]=a*.475f;p[(axis+2)%3]=b*.475f;s[axis]=.97f;
                Part(art,"Puzzle gilt edge",p,s,PrimitiveType.Cube,gold,coreWhite);
            }
            var root=new GameObject("Garden discoveries").transform;
            // Asymmetric groups sit inside planting beds and at side corners, clear of retrieval and bridge routes.
            var points=new[]{new Vector3(14.7f,.4f,-7.4f),new Vector3(9.4f,.4f,-15.5f),new Vector3(-5.5f,.4f,-15.7f),new Vector3(-10.5f,.4f,16.5f),new Vector3(14.4f,.4f,16.8f),new Vector3(6.1f,.5f,2.4f)};
            for(int i=0;i<points.Length;i++)
            {
                var group=new GameObject("Rooted crystal outcrop").transform;group.SetParent(root,false);group.position=points[i];group.localRotation=Quaternion.Euler(0,i*47,0);
                Part(group,"Weathered stone footing",Vector3.zero,new Vector3(.9f,.12f,.7f),PrimitiveType.Sphere,ivory,night);
                for(int k=0;k<3;k++){var c=Shard(group,"Scenery crystal",new Vector3((k-1)*.25f,.28f, k%2*.18f),new Vector3(.24f,.45f+k*.18f,.24f),pink,violet);c.transform.localRotation=Quaternion.Euler(k*9,0,(k-1)*18);}
            }
            foreach(var p in new[]{new Vector3(12.5f,0,-17.5f),new Vector3(-14,0,20)})
            {
                var group=new GameObject("Silent garden beacon").transform;group.SetParent(root,false);group.position=p;
                Part(group,"Low plinth",Vector3.up*.12f,new Vector3(1.1f,.12f,1.1f),PrimitiveType.Cylinder,ivory,night);
                Part(group,"Beacon stem",Vector3.up*.52f,new Vector3(.45f,.3f,.45f),PrimitiveType.Cylinder,ivory,night);
                Part(group,"Stone cup",Vector3.up*.87f,new Vector3(.85f,.08f,.85f),PrimitiveType.Cylinder,ivory,night);
                Shard(group,"Inset beacon light",Vector3.up*1.1f,new Vector3(.24f,.5f,.24f),pink,violet);
            }
            foreach(var p in new[]{new Vector3(16,0,-3),new Vector3(-15.5f,0,18.5f)})
            {
                var group=new GameObject("Weathered garden sentinel").transform;group.SetParent(root,false);group.position=p;group.localRotation=Quaternion.Euler(0,p.x<0?35:-35,0);
                Part(group,"Statue base",Vector3.up*.15f,new Vector3(1.2f,.15f,1),PrimitiveType.Cylinder,ivory,night);
                Part(group,"Robed silhouette",Vector3.up*1.1f,new Vector3(.65f,.85f,.45f),PrimitiveType.Capsule,ivory,night);
                Part(group,"Faceless head",Vector3.up*2.1f,new Vector3(.42f,.5f,.4f),PrimitiveType.Sphere,ivory,night);
                Shard(group,"Shoulder mantle",Vector3.up*1.5f,new Vector3(.95f,.7f,.55f),ivory,night);
            }
            foreach(var p in new[]{new Vector3(-5.5f,.5f,2.5f),new Vector3(5.9f,.5f,18.4f),new Vector3(-11.4f,.4f,-7.5f)})
            for(int i=0;i<5;i++){
                var pos=p+new Vector3(Mathf.Sin(i*2.4f)*.35f,.1f,Mathf.Cos(i*2.4f)*.35f);
                Part(root,"Flowering tuft",pos,new Vector3(.16f,.35f+i*.04f,.12f),PrimitiveType.Capsule,pink,violet);
            }
            AssetDatabase.SaveAssets();
            foreach(var m in new[]{glassWhite,glassBlack,coreWhite,coreBlack}){m.EnableKeyword("_EMISSION");m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;EditorUtility.SetDirty(m);}
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene);
        }
    }
}
