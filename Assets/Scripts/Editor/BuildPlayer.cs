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
            // Respect the project's display defaults; runtime preferences override them on launch.
            Directory.CreateDirectory("Builds/Linux");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = System.Array.ConvertAll(System.Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled), scene => scene.path),
                locationPathName = "Builds/Linux/ProjectParadox.x86_64",
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Linux build failed: " + report.summary.result);
            UnityEngine.Debug.Log("PARADOX: Linux player build succeeded.");
        }
    }
}
