using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Paradox.Editor
{
    // In-place spatial pass on the saved scenes. Does not call either original scene generator.
    // Floor cells are merged into strips; exposed outer edges receive physical boundaries.
    public static class ExpandWorlds
    {
        private static Transform additions;
        private static DimensionManager manager;
        private static Material white, black, accent;
        private static Scene scene;
        [MenuItem("Paradox/Apply Spacious Layouts Once")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (var entry in EditorBuildSettings.scenes)
            {
                if (!entry.enabled) continue;
                scene = EditorSceneManager.OpenScene(entry.path);
                if (Root("Expanded Environment", false)) throw new Exception("Expansion already applied: " + entry.path);
                manager = Root("Systems").GetComponent<DimensionManager>();
                white = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PaleStone.mat");
                black = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NightStone.mat");
                accent = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WarmTrim.mat");
                additions = new GameObject("Expanded Environment").transform;
                int level = scene.name == "Level01" ? 0 : scene.name.StartsWith("Level02") ? 1 : scene.name.StartsWith("Level03") ? 2 : scene.name.StartsWith("Level04") ? 3 : 4;
                foreach (string name in new[] { "West Wall", "East Wall", "Start Wall", "End Wall" })
                {
                    // Keep the old object IDs, but retire their enclosing surfaces.
                    var wall = Root(name); wall.GetComponent<Renderer>().enabled = false;
                    wall.GetComponent<Collider>().enabled = false; wall.GetComponent<DimensionTint>().enabled = false;
                }
                var occupied = new HashSet<Vector2Int>();
                // Cells: x = integer centre; z = integer + 0.5. Original floors remain untouched.
                for (int z = -28; z < 25; z++)
                    for (int x = -22; x <= 22; x++)
                        if (Inside(level, x, z + 0.5f)) occupied.Add(new Vector2Int(x, z));
                for (int z = -28; z < 25; z++)
                {
                    int start = -99;
                    for (int x = -22; x <= 23; x++)
                    {
                        bool add = occupied.Contains(new Vector2Int(x, z)) && !Original(x, z);
                        if (add && start == -99) start = x;
                        if (!add && start != -99)
                        {
                            Box("Exploration ground", new Vector3((start + x - 1) * 0.5f, -0.5f, z + 0.5f), new Vector3(x - start, 1, 1)); start = -99;
                        }
                    }
                }
                var directions = new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
                foreach (Vector2Int cell in occupied)
                    foreach (Vector2Int direction in directions)
                    {
                        if (occupied.Contains(cell + direction)) continue;
                        // Preserve the original deliberate chasm edge and crossing in the core route.
                        if ((cell.y == 4 && direction == Vector2Int.up || cell.y == 11 && direction == Vector2Int.down) && Mathf.Abs(cell.x) <= 7) continue;
                        Vector3 pos = new Vector3(cell.x + direction.x * 0.5f, 1, cell.y + 0.5f + direction.y * 0.5f);
                        Box(level == 1 ? "Library perimeter" : "Landscape edge", pos, direction.x != 0 ? new Vector3(0.24f, level == 1 ? 10 : 2, 1.08f) : new Vector3(1.08f, level == 1 ? 10 : 2, 0.24f));
                    }
                // The expanded far-side space must never bypass the existing locked door.
                Root("Exit Partition Left").transform.SetPositionAndRotation(new Vector3(-11.875f, 2, 21), Quaternion.identity);
                Root("Exit Partition Left").transform.localScale = new Vector3(20.75f, 4, 0.6f);
                Root("Exit Partition Right").transform.SetPositionAndRotation(new Vector3(11.875f, 2, 21), Quaternion.identity);
                Root("Exit Partition Right").transform.localScale = new Vector3(20.75f, 4, 0.6f);
                switch (level) { case 0: Garden(); break; case 1: Library(); break; case 2: Lake(); break; case 3: Observatory(); break; case 4: Heart(); break; }
                EditorSceneManager.SaveScene(scene);
                Debug.Log("PARADOX SPATIAL: " + scene.name + " ground area " + occupied.Count + " m2; original puzzle transforms retained.");
            }
            AssetDatabase.SaveAssets();
        }
        private static bool Original(int x, int z) => Mathf.Abs(x) <= 7 && (z >= -6 && z < 5 || z >= 11 && z < 25);
        private static bool Inside(int level, float x, float z)
        {
            if (Original((int)x, Mathf.FloorToInt(z))) return true;
            if (z > 5 && z < 11 || z >= 21) return false;
            if (z < 5)
            {
                switch (level)
                {
                    case 0: return x*x / 361 + (z+8)*(z+8)/225 < 1 || Mathf.Abs(x) < 9 && z > -23;
                    case 1: return Mathf.Abs(x) < 13 && z > -21 || Mathf.Abs(x) < 22 && z > -15 && z < -3;
                    case 2: return x*x / 441 + (z+4)*(z+4)/256 < 1 || x < -7 && x > -21 && z > -22 && z < -8;
                    case 3: return Mathf.Abs(x) < 9 && z > -27 || Mathf.Abs(x) < 19 && z > -14 && z < -2;
                    default: return Mathf.Abs(x) < 7 + (z+27)*0.38f && z > -27 && Mathf.Abs(x) < 21;
                }
            }
            switch (level)
            {
                case 0: return Mathf.Abs(x) < 17 && z < 21;
                case 1: return Mathf.Abs(x) < 21 && z < 21;
                case 2: return x*x/441 + (z-14)*(z-14)/100 < 1;
                case 3: return Mathf.Abs(x) < 20 && z < 21;
                default: return x*x/441 + (z-16)*(z-16)/64 < 1;
            }
        }
        private static GameObject Root(string name, bool required = true)
        { foreach (var root in scene.GetRootGameObjects()) if (root.name == name) return root; if (required) throw new Exception("Missing " + name); return null; }
        private static GameObject Shape(string name, PrimitiveType type, Vector3 pos, Vector3 scale, bool solid = true)
        {
            var item = GameObject.CreatePrimitive(type); item.name = name; item.transform.SetParent(additions, false); item.transform.position = pos; item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = white; item.AddComponent<DimensionTint>().Configure(manager, white, black);
            if (!solid) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
            return item;
        }
        private static GameObject Box(string name, Vector3 pos, Vector3 size, bool solid = true) => Shape(name, PrimitiveType.Cube, pos, size, solid);
        private static void PathSegment(Vector3 a, Vector3 b, float width = 2.5f)
        {
            var path = Box("Walking path", (a+b)*0.5f + Vector3.up*0.015f, new Vector3(width, 0.02f, Vector3.Distance(a,b)), false);
            path.transform.rotation = Quaternion.LookRotation(b-a); path.GetComponent<DimensionTint>().Configure(manager, accent, black);
        }
        private static void Terrace(Vector3 centre, Vector2 size, float height)
        {
            Box("Raised terrace", new Vector3(centre.x, height*0.5f, centre.z), new Vector3(size.x, height, size.y));
            // Broad shallow steps stay within the existing controller's 0.25-unit step offset.
            for (int i=0;i<10;i++) Box("Terrace step", new Vector3(centre.x, (i+1)*height/20, centre.z-size.y*0.5f-5+(i+0.5f)*0.5f), new Vector3(3, (i+1)*height/10, 0.52f));
            Box("Terrace back rail", new Vector3(centre.x,height+1,centre.z+size.y*0.5f),new Vector3(size.x,2,0.2f));
            foreach (int side in new[]{-1,1}) Box("Terrace side rail",new Vector3(centre.x+side*size.x*0.5f,height+1,centre.z),new Vector3(0.2f,2,size.y));
        }
        private static void Garden()
        {
            Vector3[] loop = {new Vector3(0,0,-3),new Vector3(-8,0,-7),new Vector3(-11,0,-13),new Vector3(0,0,-19),new Vector3(11,0,-13),new Vector3(8,0,-7),new Vector3(0,0,-3)};
            for(int i=1;i<loop.Length;i++) PathSegment(loop[i-1],loop[i]);
            foreach(var point in new[]{new Vector3(-13,0,-7),new Vector3(13,0,-7),new Vector3(-7,0,-15),new Vector3(7,0,-15),new Vector3(-12,0,16),new Vector3(12,0,16)})
            {
                Shape("Garden tree trunk",PrimitiveType.Cylinder,point+Vector3.up*1.8f,new Vector3(0.5f,1.8f,0.5f));
                Shape("Garden tree crown",PrimitiveType.Sphere,point+Vector3.up*4.5f,new Vector3(4,4,4),false);
                Box("Separated flower bed",point+Vector3.right*2+Vector3.up*0.2f,new Vector3(2,0.4f,3));
            }
            Shape("Distant garden pavilion",PrimitiveType.Cylinder,new Vector3(0,3.5f,-21),new Vector3(6,0.2f,6),false);
            foreach(int side in new[]{-1,1}) Box("Pavilion pillar",new Vector3(side*2,1.7f,-21),new Vector3(0.4f,3.4f,0.4f));
        }
        private static void Library()
        {
            foreach(int side in new[]{-1,1})
            {
                for(int row=0;row<3;row++)
                    for(int shelf=0;shelf<5;shelf++) Box("Tall collection shelves",new Vector3(side*(15+row*2.2f),0.8f+shelf*1.4f,-9),new Vector3(0.4f,0.18f,8));
                for(int z=-18;z<=-2;z+=8) Box("Reading hall pier",new Vector3(side*10,4.5f,z),new Vector3(0.7f,9,0.7f));
                Box("Reading gallery beam",new Vector3(side*10,9,-10),new Vector3(1,0.6f,20));
                Terrace(new Vector3(side*11,0,17),new Vector2(5,5),2);
                Box("Side archive table",new Vector3(side*16,0.7f,15),new Vector3(4,1.4f,2));
            }
            Box("Reading hall roof",new Vector3(0,10,-10),new Vector3(25,0.3f,20));
            for(int x=-5;x<=5;x+=5) Box("Central reading table",new Vector3(x,0.6f,-12),new Vector3(2.5f,1.2f,5));
            Box("Balcony connector",new Vector3(0,2.1f,19.8f),new Vector3(22,0.2f,1.5f));
            Box("Balcony inner rail",new Vector3(0,3.2f,19.1f),new Vector3(20,2,0.15f));
            // Connector is overhead; it does not reach over the locked partition at z=21.
        }
        private static void Lake()
        {
            var lake=Root("Greybox Scenery");
            foreach(var item in lake.GetComponentsInChildren<Transform>(true))
                if(item.name=="Lake surface") item.localScale=new Vector3(43,0.05f,5.8f);
            Vector3[] shore={new Vector3(-17,0,1),new Vector3(-15,0,-9),new Vector3(-8,0,-15),new Vector3(6,0,-15),new Vector3(16,0,-7),new Vector3(18,0,1)};
            for(int i=1;i<shore.Length;i++) PathSegment(shore[i-1],shore[i],3);
            foreach(int side in new[]{-1,1})
            {
                for(int i=0;i<3;i++) Shape("Distant ice formation",PrimitiveType.Cube,new Vector3(side*(11+i*3),2+i,-9+i*2),new Vector3(1.5f,4+i*2,2));
                Terrace(new Vector3(side*13,0,17),new Vector2(5,4),1);
                // Isolated islands are scenery, not a White route across the gap.
                Shape("Offshore ice",PrimitiveType.Cylinder,new Vector3(side*13,-0.8f,8),new Vector3(3,0.2f,2),false);
            }
        }
        private static void Observatory()
        {
            foreach(int side in new[]{-1,1})
            {
                Terrace(new Vector3(side*13,0,17),new Vector2(6,5),2);
                Shape("Observation tower",PrimitiveType.Cylinder,new Vector3(side*15,4,-9),new Vector3(3,4,3));
                Shape("Observation dome",PrimitiveType.Sphere,new Vector3(side*15,8,-9),new Vector3(5,3,5),false);
                for(int z=-23;z<-4;z+=6) Box("Approach obelisk",new Vector3(side*6,2.8f,z),new Vector3(0.8f,5.6f,0.8f));
            }
            foreach(var item in Root("Greybox Scenery").GetComponentsInChildren<Transform>(true))
            {
                if(item.name=="Clock face") {item.position=new Vector3(0,8,20);item.localScale=new Vector3(9,0.15f,9);}
                if(item.name=="Skipping clock hand") {item.position=new Vector3(0,8,19.7f);item.localScale=new Vector3(0.22f,7,0.12f);}
                if(item.name=="Eclipse disc") {item.position=new Vector3(1,12,17);item.localScale=new Vector3(3,3,0.25f);}
                if(item.name=="Eclipse halo") {item.position=new Vector3(0,12,17.3f);item.localScale=new Vector3(4,4,0.25f);}
            }
            Box("Astronomical portico",new Vector3(0,13,20),new Vector3(18,0.5f,1));
            foreach(int side in new[]{-1,1}) Box("Portico support",new Vector3(side*8,6.5f,20),new Vector3(1,13,1));
        }
        private static void Heart()
        {
            foreach(var item in Root("Greybox Scenery").GetComponentsInChildren<Transform>(true))
            {
                if(item.name=="Heart core segment") {item.position+=Vector3.up*5;item.localScale*=3;}
                if(item.name=="Core echo") {item.position=new Vector3(item.position.x*3,7,item.position.z);item.localScale*=2;}
                if(item.name=="Entrance fin") item.position=new Vector3(item.position.x*2.4f,3,-9);
            }
            for(int i=0;i<5;i++)
                foreach(int side in new[]{-1,1})
                {
                    float x=side*(5+i*2.8f),z=-23+i*4;
                    Box("Processional monolith",new Vector3(x,2+i*0.6f,z),new Vector3(0.8f,4+i*1.2f,1.2f));
                }
            foreach(int side in new[]{-1,1})
            {
                Terrace(new Vector3(side*13,0,17),new Vector2(6,4),1.6f);
                Box("Arena spire",new Vector3(side*17,5,17),new Vector3(1,10,1));
            }
            PathSegment(new Vector3(0,0,-25),new Vector3(0,0,-5),4);
        }
    }
}
