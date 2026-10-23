using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Paradox.Tests {
public sealed class WorldVisualTests {
[UnityTest] public IEnumerator AllWorldMaterialsAndDimensionPreviews() {
foreach(string name in new[]{"Level02_Library","Level03_MirrorLake","Level04_Observatory","Level05_Heart"}) {
PuzzleConfigurationSelector.DebugSeed=0;SceneManager.LoadScene(name);yield return null;yield return new WaitForFixedUpdate();
var player=Object.FindFirstObjectByType<FirstPersonController>();player.enabled=false;
var manager=Object.FindFirstObjectByType<DimensionManager>();
foreach(string root in new[]{"Celestial visual dressing","Celestial finishing kit","Celestial architecture"}) {var art=GameObject.Find(root);Assert.IsNotNull(art,name+root);Assert.AreEqual(0,art.GetComponentsInChildren<Collider>(true).Length);}
var camera=player.GetComponentInChildren<Camera>();Object.FindFirstObjectByType<HUD>().GetComponent<Canvas>().enabled=false;
for(int dimension=0;dimension<2;dimension++) {
foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) foreach(var m in r.sharedMaterials){Assert.IsNotNull(m,r.name);Assert.IsNotNull(m.shader);Assert.IsTrue(m.shader.isSupported,m.name);}
camera.transform.position=new Vector3(6,5,-5);camera.transform.LookAt(new Vector3(0,6,20));Capture(camera,name+"-"+manager.Current);
if(name.Contains("Library")){camera.transform.position=new Vector3(6,3,-1);camera.transform.LookAt(new Vector3(0,6,-10));Capture(camera,name+"-Archive-"+manager.Current);}
if(dimension==0){yield return new WaitForSeconds(.4f);Assert.IsTrue(manager.TrySwitch());yield return new WaitForFixedUpdate();}
}
}PuzzleConfigurationSelector.DebugSeed=null;
}
        private static void Capture(Camera camera,string name)
        {
            var target=new RenderTexture(1280,720,24);var previous=RenderTexture.active;
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
            System.IO.Directory.CreateDirectory("/tmp/paradox-world-previews");System.IO.File.WriteAllBytes("/tmp/paradox-world-previews/"+name+".png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=previous;Object.Destroy(image);Object.Destroy(target);
        }
    }
}
