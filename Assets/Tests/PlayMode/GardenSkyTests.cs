using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Paradox.Tests {
public sealed class GardenSkyTests {
[UnityTest] public IEnumerator RepeatedDimensionSkySwitches() {
foreach(string scene in new[]{"Level01","Level02_Library","Level03_MirrorLake","Level04_Observatory","Level05_Heart"}) {
PuzzleConfigurationSelector.DebugSeed=0;SceneManager.LoadScene(scene);yield return null;yield return new WaitForFixedUpdate();
var manager=Object.FindFirstObjectByType<DimensionManager>();var sky=Object.FindFirstObjectByType<GardenSkyBackground>();Assert.IsNotNull(sky);
var player=Object.FindFirstObjectByType<FirstPersonController>();player.enabled=false;var camera=player.GetComponentInChildren<Camera>();Object.FindFirstObjectByType<HUD>().GetComponent<Canvas>().enabled=false;
for(int i=0;i<6;i++) {
Assert.AreEqual(manager.Current==Dimension.White?"Day":"Night",sky.CurrentTexture.name);
camera.transform.rotation=Quaternion.Euler(-15,i*61,0);Assert.AreEqual(1,camera.GetCommandBuffers(UnityEngine.Rendering.CameraEvent.BeforeForwardOpaque).Length);Capture(camera,scene+"-"+i+"-"+manager.Current);
yield return new WaitForSeconds(.4f);Assert.IsTrue(manager.TrySwitch());yield return new WaitForFixedUpdate();
}
sky.enabled=false;sky.enabled=true;Assert.AreEqual("Day",sky.CurrentTexture.name);
}
PuzzleConfigurationSelector.DebugSeed=null;
}
        private static void Capture(Camera camera,string name)
        {
            int width=name.Contains("-2-")||name.Contains("-3-")?1680:1280; int height=name.Contains("-4-")||name.Contains("-5-")?960:720;
            var target=new RenderTexture(width,height,24);var previous=RenderTexture.active;
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            System.IO.Directory.CreateDirectory("/tmp/paradox-sky-previews");System.IO.File.WriteAllBytes("/tmp/paradox-sky-previews/"+name+".png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=previous;Object.Destroy(image);Object.Destroy(target);
        }
    }
}
