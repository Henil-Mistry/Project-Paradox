using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace Paradox.Editor
{
    public static class FinishGardenVisuals
    {
        const string Folder="Assets/Art/Garden/";
        public static void Apply()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Level01.unity");
            var manager=Object.FindFirstObjectByType<DimensionManager>();
            Material pink=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Blossom pink.mat"), violet=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Starlit blossom.mat");
            foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/Art/Garden"}))
            {
                var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if(m.HasProperty("_EmissionColor") && m.GetColor("_EmissionColor").maxColorComponent>0)
                {m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;m.EnableKeyword("_EMISSION");}
                m.enableInstancing=true;EditorUtility.SetDirty(m);
            }
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"MarbleVeins.asset");
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)texture.SetPixel(x,y,Color.Lerp(texture.GetPixel(x,y),Color.white,.8f));
            texture.Apply();EditorUtility.SetDirty(texture);
            // A soft alpha sprite avoids square petals/motes with the Standard surface shader.
            var sprite=new Texture2D(32,32,TextureFormat.RGBA32,false);
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {float radius=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;float a=Mathf.Pow(Mathf.Clamp01(1-radius),1.5f);sprite.SetPixel(x,y,new Color(1,1,1,a));}
            sprite.Apply();AssetDatabase.CreateAsset(sprite,Folder+"SoftParticle.asset");
            foreach(var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
            {
                if(ps.name!="Drifting petals" && ps.name!="Astral motes")continue;
                var material=new Material(Shader.Find("Legacy Shaders/Particles/Additive"));material.mainTexture=sprite;
                material.SetColor("_TintColor",ps.name=="Drifting petals"?new Color(1,.4f,.6f,.6f):new Color(.4f,.55f,1,.8f));
                AssetDatabase.CreateAsset(material,Folder+ps.name+".mat");ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=material;
            }
            var marble=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Ivory marble.mat");var night=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Midnight stone.mat");
            foreach(var tint in Object.FindObjectsByType<DimensionTint>(FindObjectsSortMode.None))
            {
                if(tint.name=="Walking path") {tint.Configure(manager,marble,night);tint.GetComponent<Renderer>().sharedMaterial=marble;}
                if(tint.name=="Geometric flower") {tint.Configure(manager,pink,violet);tint.GetComponent<Renderer>().sharedMaterial=pink;}
            }
            // Shared low-poly blossom mesh: small five-petal clusters distributed around a canopy.
            var vertices=new List<Vector3>();var triangles=new List<int>();var random=new System.Random(731);
            for(int i=0;i<900;i++)
            {
                float angle=(float)random.NextDouble()*Mathf.PI*2, radius=Mathf.Sqrt((float)random.NextDouble())*2.6f;
                Vector3 centre=new Vector3(Mathf.Cos(angle)*radius,3.8f+(float)random.NextDouble()*1.6f,Mathf.Sin(angle)*radius);
                Quaternion rotation=Quaternion.Euler((float)random.NextDouble()*180,(float)random.NextDouble()*180,(float)random.NextDouble()*180);
                for(int petal=0;petal<5;petal++)
                {
                    float a=petal*Mathf.PI*2/5;Vector3 tip=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*.13f;
                    Vector3 edge=new Vector3(-Mathf.Sin(a),Mathf.Cos(a),0)*.045f;int n=vertices.Count;
                    vertices.Add(centre);vertices.Add(centre+rotation*(tip*.6f+edge));vertices.Add(centre+rotation*tip);vertices.Add(centre+rotation*(tip*.6f-edge));
                    triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3,n+2,n+1,n,n+3,n+2,n});
                }
            }
            var mesh=new Mesh();mesh.name="Blossom clusters";mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"BlossomClusters.asset");
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(t.name!="Blossom tree" && t.name!="Key grove landmark")continue;
                var detail=new GameObject("Blossom clusters");detail.transform.SetParent(t,false);detail.AddComponent<MeshFilter>().sharedMesh=mesh;
                detail.AddComponent<MeshRenderer>().sharedMaterial=pink;detail.AddComponent<DimensionTint>().Configure(manager,pink,violet);
            }
            // Bake static decoration into shared-material batches, retaining separate dimension roots.
            Batch(GameObject.Find("Garden of Echoes Art").transform,manager,"Garden");
            Batch(GameObject.Find("Constellation bridge").transform,manager,"Bridge");
            Batch(GameObject.Find("Gate filigree").transform,manager,"Gate");
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
        }
        static void Batch(Transform root,DimensionManager manager,string label)
        {
            var groups=new Dictionary<string,List<MeshFilter>>();var materials=new Dictionary<string,Material[]>();
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                // The constellation has its own visibility controller; never combine it with shared stone.
                if(label=="Garden" && filter.GetComponentInParent<DimensionObject>())continue;
                var tint=filter.GetComponent<DimensionTint>();if(!tint || !filter.GetComponent<Renderer>().enabled)continue;
                var so=new SerializedObject(tint);var white=(Material)so.FindProperty("white").objectReferenceValue;var black=(Material)so.FindProperty("black").objectReferenceValue;
                string key=white.name+"_"+black.name;if(!groups.ContainsKey(key)){groups[key]=new List<MeshFilter>();materials[key]=new[]{white,black};}groups[key].Add(filter);
            }
            foreach(var pair in groups)
            {
                var combine=new List<CombineInstance>();foreach(var f in pair.Value)combine.Add(new CombineInstance{mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix});
                var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.CombineMeshes(combine.ToArray());AssetDatabase.CreateAsset(mesh,Folder+label+"-"+pair.Key+".asset");
                var go=new GameObject(label+" detail batch");go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=materials[pair.Key][0];go.AddComponent<DimensionTint>().Configure(manager,materials[pair.Key][0],materials[pair.Key][1]);
                foreach(var f in pair.Value){Object.DestroyImmediate(f.GetComponent<DimensionTint>());Object.DestroyImmediate(f.GetComponent<Renderer>());Object.DestroyImmediate(f);}
            }
        }
    }
}
