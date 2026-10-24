using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Paradox.Tests
{
    public sealed class LevelUIPolishTests
    {
        [UnityTest] public IEnumerator IntroductionFadesOnceAcrossEveryLevel()
        {
            for(int i=0;i<5;i++)
            {
                SceneManager.LoadScene(MainMenu.LevelScenes[i]);yield return null;yield return null;
                var hud=Object.FindFirstObjectByType<HUD>();var play=hud.transform.Find("Play");
                var title=play.Find("Title").GetComponent<Text>();var group=title.GetComponent<CanvasGroup>();
                Assert.AreEqual("Level "+(i+1)+" — "+MainMenu.LevelNames[i],title.text);
                Assert.AreEqual(1,group.alpha);Assert.IsFalse(play.Find("Header Backing").gameObject.activeSelf);
                Assert.IsFalse(play.Find("Tagline").gameObject.activeSelf);
                var menu=Object.FindFirstObjectByType<LevelMenus>();
                menu.TogglePause();Assert.AreEqual(0,Time.timeScale);
                yield return new WaitForSecondsRealtime(2.5f);Assert.AreEqual(1,group.alpha);
                yield return new WaitForSecondsRealtime(.7f);Assert.That(group.alpha,Is.InRange(.01f,.99f));
                yield return new WaitForSecondsRealtime(.5f);Assert.IsFalse(title.gameObject.activeSelf);
                if(i==0)
                {
                    title.gameObject.SetActive(true);group.alpha=1;
                    Assert.NotNull(GameObject.Find("Garden controls tablet"));
                    foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1024,768),new Vector2Int(2560,1080),new Vector2Int(720,1280)})
                        Capture(hud,size,"intro");
                    title.gameObject.SetActive(false);
                }
                menu.ShowDisplay(true);
                Assert.IsTrue(GameObject.Find("Display settings").activeInHierarchy);
                if(i==0)foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1024,768),new Vector2Int(2560,1080),new Vector2Int(720,1280)})
                    Capture(hud,size,"display");
                menu.TogglePause();Assert.IsTrue(menu.Open); // Escape returns from settings first.
                menu.Resume();Assert.AreEqual(1,Time.timeScale);
                var manager=Object.FindFirstObjectByType<DimensionManager>();
                Assert.IsTrue(manager.RequestSwitch());yield return new WaitForSecondsRealtime(1.8f);
                Assert.IsFalse(title.gameObject.activeSelf,"Dimension switching must not replay intro");
                Assert.IsTrue(play.Find("Dimension").gameObject.activeInHierarchy);
                Assert.IsTrue(play.Find("Crosshair").gameObject.activeInHierarchy);
                Object.FindFirstObjectByType<GameFlow>().Restart();yield return null;yield return null;
                Assert.IsTrue(Object.FindFirstObjectByType<HUD>().transform.Find("Play/Title").gameObject.activeSelf);
            }
            LogAssert.NoUnexpectedReceived();
        }
        static void Capture(HUD hud,Vector2Int size,string stage)
        {
            var canvas=hud.GetComponent<Canvas>();var camera=Camera.main;
            if(!camera) camera=Object.FindFirstObjectByType<FirstPersonController>().GetComponentInChildren<Camera>();
            var rt=new RenderTexture(size.x,size.y,24);var old=RenderTexture.active;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.3f;
            camera.targetTexture=rt;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
            foreach(var button in canvas.GetComponentsInChildren<Button>())
            {
                var corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);
                foreach(var corner in corners){var point=camera.WorldToViewportPoint(corner);Assert.That(point.x,Is.InRange(-.01f,1.01f));Assert.That(point.y,Is.InRange(-.01f,1.01f));}
            }
            var texture=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,size.x,size.y),0,0);texture.Apply();
            System.IO.Directory.CreateDirectory("/tmp/paradox-ui-polish");System.IO.File.WriteAllBytes("/tmp/paradox-ui-polish/"+stage+"-"+size.x+"x"+size.y+".png",texture.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=old;canvas.renderMode=RenderMode.ScreenSpaceOverlay;Object.Destroy(texture);Object.Destroy(rt);
        }
        [UnityTest] public IEnumerator DisplayModesAndResolutionInPlayer()
        {
            if(Application.isEditor)Assert.Ignore("Requires the standalone test player; Editor does not change OS display modes.");
            string[] keys={"Paradox.Display.Width","Paradox.Display.Height","Paradox.Display.Fullscreen"};
            var had=new bool[3];var values=new int[3];for(int i=0;i<3;i++){had[i]=PlayerPrefs.HasKey(keys[i]);values[i]=PlayerPrefs.GetInt(keys[i]);}
            var oldSize=new Vector2Int(Screen.width,Screen.height);bool oldFull=Screen.fullScreen;
            try
            {
                SceneManager.LoadScene("Level01");yield return null;yield return null;
                var sizes=DisplayPreferences.Sizes;Assert.Greater(sizes.Count,0);
                var window=sizes[0];DisplayPreferences.Apply(window,false);yield return new WaitForSecondsRealtime(2);
                Assert.AreEqual(FullScreenMode.Windowed,Screen.fullScreenMode);
                Assert.AreEqual(window.x,Screen.width);Assert.AreEqual(window.y,Screen.height);
                var desktop=DisplayPreferences.DesktopSize;DisplayPreferences.Apply(desktop,true);yield return new WaitForSecondsRealtime(2);
                Assert.IsTrue(Screen.fullScreen);Assert.AreEqual(FullScreenMode.FullScreenWindow,Screen.fullScreenMode);
                DisplayPreferences.Apply(window,false);yield return new WaitForSecondsRealtime(2);
                Assert.IsFalse(Screen.fullScreen);Assert.AreEqual(window.x,Screen.width);Assert.AreEqual(window.y,Screen.height);
                Assert.AreEqual(window.x,PlayerPrefs.GetInt(keys[0]));Assert.AreEqual(0,PlayerPrefs.GetInt(keys[2]));
                SceneManager.LoadScene("Level02_Library");yield return null;yield return null;
                Assert.IsFalse(Screen.fullScreen);Assert.AreEqual(window.x,Screen.width);
            }
            finally
            {
                Screen.SetResolution(oldSize.x,oldSize.y,oldFull?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
                for(int i=0;i<3;i++){if(had[i])PlayerPrefs.SetInt(keys[i],values[i]);else PlayerPrefs.DeleteKey(keys[i]);}PlayerPrefs.Save();
            }
        }
        [TearDown] public void ResetTime(){Time.timeScale=1;}
    }
}
