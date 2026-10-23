using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
namespace Paradox.Editor
{
    // Optional local Linux build, using the editor's installed platform support.
    public static class BuildPlayer
    {
        [MenuItem("Paradox/Build Linux Player")]
        public static void BuildLinux()
        {
            PlayerSettings.productName = "Project Paradox";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = UnityEngine.FullScreenMode.Windowed;
            Directory.CreateDirectory("Builds/Linux");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Level01.unity" },
                locationPathName = "Builds/Linux/ProjectParadox.x86_64",
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Linux build failed: " + report.summary.result);
            UnityEngine.Debug.Log("PARADOX: Linux player build succeeded.");
        }
    }
}
