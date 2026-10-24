using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Paradox.Tests
{
    public sealed class HUDInstructionTests
    {
        void Check()
        {
            var hud=Object.FindFirstObjectByType<HUD>(); Assert.NotNull(hud);
            var play=hud.transform.Find("Play"); Assert.NotNull(play);
            foreach(string name in new[]{"Prompt","Prompt Backing","Controls","Footer Backing","Header Backing","Tagline"})
                Assert.IsFalse(play.Find(name).gameObject.activeInHierarchy,name);
            foreach(string name in new[]{"Dimension","Crosshair","Message"})
                Assert.IsTrue(play.Find(name).gameObject.activeInHierarchy,name);
            Assert.NotNull(Object.FindFirstObjectByType<LevelMenus>());
            if(SceneManager.GetActiveScene().name=="Level01") Assert.NotNull(GameObject.Find("Garden controls tablet"));
        }
        [UnityTest] public IEnumerator InstructionBarsStayHiddenAcrossAllLevelsAndSwitches()
        {
            foreach(string scene in MainMenu.LevelScenes)
            {
                SceneManager.LoadScene(scene);yield return null;yield return null;Check();
                var gun=Object.FindFirstObjectByType<TransferGun>();gun.ToggleEquipped();yield return null;Assert.IsTrue(gun.Equipped);
                var hud=Object.FindFirstObjectByType<HUD>();hud.Prompt("Left-click — Transfer cube • E — Holster");Check();
                var dimensions=Object.FindFirstObjectByType<DimensionManager>();
                for(int i=0;i<2;i++){
                    var before=dimensions.Current;dimensions.RequestSwitch();
                    float deadline=Time.realtimeSinceStartup+4;
                    while(Object.FindFirstObjectByType<RealmTransition>().Busy && Time.realtimeSinceStartup<deadline)yield return null;
                    Assert.AreNotEqual(before,dimensions.Current);Check();
                }
                Object.FindFirstObjectByType<GameFlow>().Restart();yield return null;yield return null;Check();
            }
        }
    }
}
