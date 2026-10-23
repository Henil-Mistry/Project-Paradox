using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Paradox.Editor {
public static class InstallGardenSky {
public static void Apply(){
AssetDatabase.Refresh();
foreach(string name in new[]{"Day","Night"}){var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Art/GardenSky/"+name+".png");importer.textureType=TextureImporterType.Default;importer.wrapMode=TextureWrapMode.Clamp;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
foreach(string name in new[]{"Level01","Level02_Library","Level03_MirrorLake","Level04_Observatory","Level05_Heart"}) {
var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
var path="Assets/Art/GardenSky/Background.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(Shader.Find("Paradox/Illustrated Background"));AssetDatabase.CreateAsset(material,path);}
var camera=Object.FindFirstObjectByType<FirstPersonController>().GetComponentInChildren<Camera>();
var sky=camera.GetComponent<GardenSkyBackground>();if(!sky)sky=camera.gameObject.AddComponent<GardenSkyBackground>();
sky.Configure(Object.FindFirstObjectByType<DimensionManager>(),material,AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/GardenSky/Day.png"),AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/GardenSky/Night.png"));
EditorUtility.SetDirty(sky);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
}
}
}}
