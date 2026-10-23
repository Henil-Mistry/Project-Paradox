using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Paradox.Tests
{
    public sealed class GardenVisualTests
    {
        [UnityTest]
        public IEnumerator GardenArtVisibilityMaterialsAndPreviews()
        {
            PuzzleConfigurationSelector.DebugSeed=0;
            SceneManager.LoadScene("Level01");yield return null;yield return new WaitForFixedUpdate();
            var player=Object.FindFirstObjectByType<FirstPersonController>();player.enabled=false;
            var manager=Object.FindFirstObjectByType<DimensionManager>();
            var art=GameObject.Find("Garden of Echoes Art");Assert.IsNotNull(art);
            Assert.AreEqual(0,art.GetComponentsInChildren<Collider>(true).Length,"Decoration must not add collisions");
            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                foreach(var material in renderer.sharedMaterials)
                {Assert.IsNotNull(material,renderer.name);Assert.IsNotNull(material.shader);Assert.IsTrue(material.shader.isSupported,material.name);}
            var camera=player.GetComponentInChildren<Camera>();
            var canvas=Object.FindFirstObjectByType<HUD>().GetComponent<Canvas>();canvas.enabled=false;
            var bridge=GameObject.Find("Constellation bridge");
            for(int dimension=0;dimension<2;dimension++)
            {
                foreach(var renderer in bridge.GetComponentsInChildren<Renderer>())Assert.AreEqual(dimension==1,renderer.enabled);
                Assert.AreEqual(dimension==1,GameObject.Find("Black Bridge").GetComponent<Collider>().enabled);
                Assert.AreEqual(dimension==1,Physics.Raycast(new Vector3(0,2,8),Vector3.down,3));
                yield return new WaitForSeconds(.5f);
                camera.transform.position=new Vector3(5,3,-3);camera.transform.LookAt(new Vector3(0,2,17));Capture(camera,"Garden-"+manager.Current);
                camera.transform.position=new Vector3(-5,2,-7);camera.transform.LookAt(new Vector3(-12,2.5f,-13));Capture(camera,"Grove-"+manager.Current);
                if(dimension==0){Assert.IsTrue(manager.TrySwitch());yield return new WaitForFixedUpdate();}
            }
            PuzzleConfigurationSelector.DebugSeed=null;
        }
        private static void Capture(Camera camera,string name)
        {
            var target=new RenderTexture(1280,720,24);var previous=RenderTexture.active;
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
            System.IO.Directory.CreateDirectory("/tmp/paradox-garden-previews");System.IO.File.WriteAllBytes("/tmp/paradox-garden-previews/"+name+".png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=previous;Object.Destroy(image);Object.Destroy(target);
        }
    }
}
