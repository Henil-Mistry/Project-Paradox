using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Paradox.Tests {
public sealed class GardenPropTests {
[UnityTest] public IEnumerator PropsAreDecorativeAndCubeVisualsFollowTransfer() {
PuzzleConfigurationSelector.DebugSeed=0;SceneManager.LoadScene("Level01");yield return null;yield return new WaitForFixedUpdate();
var root=GameObject.Find("Garden discoveries");Assert.IsNotNull(root);
Assert.AreEqual(0,root.GetComponentsInChildren<Collider>(true).Length);
foreach(Transform group in root.transform) Assert.IsTrue(Physics.Raycast(group.position+Vector3.up*.1f,Vector3.down,1f),group.name+" must have ground beneath it");
Assert.AreEqual(0,root.GetComponentsInChildren<Rigidbody>(true).Length);
foreach(var behavior in root.GetComponentsInChildren<MonoBehaviour>(true)) Assert.IsInstanceOf<DimensionTint>(behavior);
Assert.AreEqual(1,Object.FindObjectsByType<TransferableObject>(FindObjectsSortMode.None).Length);
var cube=Object.FindFirstObjectByType<TransferableObject>();Assert.IsNotNull(cube.transform.Find("Puzzle crystal inlay"));
Assert.AreEqual(1,cube.GetComponentsInChildren<Collider>(true).Length);
Assert.AreEqual(new Vector3(.8f,.8f,.8f),cube.transform.localScale);
var manager=Object.FindFirstObjectByType<DimensionManager>();
var player=Object.FindFirstObjectByType<FirstPersonController>();player.enabled=false;
var camera=player.GetComponentInChildren<Camera>();Object.FindFirstObjectByType<HUD>().GetComponent<Canvas>().enabled=false;
for(int dimension=0;dimension<2;dimension++) {
cube.MoveToDimension(manager.Current,new Vector3(-10,.5f,-12));yield return new WaitForFixedUpdate();
foreach(var r in cube.GetComponentsInChildren<Renderer>()) Assert.IsTrue(r.enabled);
foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) foreach(var m in r.sharedMaterials){Assert.IsNotNull(m);Assert.IsTrue(m.shader.isSupported);}
camera.transform.position=cube.transform.position+new Vector3(1.15f,.7f,1.5f);camera.transform.LookAt(cube.transform.position);Capture(camera,"Cube-"+manager.Current);
camera.transform.position=new Vector3(12,2,-11);camera.transform.LookAt(new Vector3(15,1,-6));Capture(camera,"Props-"+manager.Current);
cube.MoveToDimension(manager.Current==Dimension.White?Dimension.Black:Dimension.White,cube.transform.position);
foreach(var r in cube.GetComponentsInChildren<Renderer>()) Assert.IsFalse(r.enabled,"All inlays must hide with the transferred cube");
if(dimension==0){yield return new WaitForSeconds(.4f);Assert.IsTrue(manager.TrySwitch());yield return new WaitForFixedUpdate();}
}
PuzzleConfigurationSelector.DebugSeed=null;
}
        private static void Capture(Camera camera,string name)
        {
            var target=new RenderTexture(1280,720,24);var previous=RenderTexture.active;
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
            System.IO.Directory.CreateDirectory("/tmp/paradox-garden-props");System.IO.File.WriteAllBytes("/tmp/paradox-garden-props/"+name+".png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=previous;Object.Destroy(image);Object.Destroy(target);
        }
    }
}
