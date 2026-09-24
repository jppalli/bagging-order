using System;
using System.IO;
using System.Linq;
using BaggingOrder;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GroceryImporter : AssetPostprocessor
{
    public override uint GetVersion(){return 2;}
    void OnPreprocessModel()
    {
        if(!assetPath.Contains("Resources/Groceries/"))return;
        var importer=(ModelImporter)assetImporter;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
        importer.isReadable=true;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;
    }
}
public class MarketArtImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if(!assetPath.Contains("Resources/UI/"))return;
        var importer=(TextureImporter)assetImporter;importer.alphaIsTransparency=true;
        importer.mipmapEnabled=false;importer.maxTextureSize=assetPath.Contains("journey-map")?4096:assetPath.Contains("shop-backdrop")?2048:1024;importer.wrapMode=TextureWrapMode.Clamp;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
    }
}
public class CozyAudioImporter : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if(!assetPath.Contains("Resources/Audio/"))return;
        var importer=(AudioImporter)assetImporter;
        var settings=importer.defaultSampleSettings;
        settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.75f;
        settings.loadType=assetPath.Contains("loop")?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
        importer.defaultSampleSettings=settings;
    }
}
public static class BuildGame
{
    [MenuItem("Bagging Order/Build Windows preview")]
    public static void Windows()
    {
        Configure();var args=Environment.GetCommandLineArgs();if(!args.Contains("-baggingSkipChecks"))RulesCheck();int index=Array.IndexOf(args,"-baggingBuildFolder");string folder=index>=0&&index+1<args.Length?args[index+1]:"Windows";
        if(folder.IndexOfAny(Path.GetInvalidFileNameChars())>=0||folder.Contains(".."))throw new Exception("Invalid build folder");
        string output=Path.Combine("Builds",folder);Directory.CreateDirectory(output);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/Scenes/Checkout.unity"},locationPathName=Path.Combine(output,"BaggingOrder.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
        Debug.Log("BAGGING_BUILD_PASS "+report.summary.totalSize+" bytes");
    }
    [MenuItem("Bagging Order/Build WebGL")]
    public static void Web()
    {
        Configure();if(!Environment.GetCommandLineArgs().Contains("-baggingSkipChecks"))RulesCheck();Directory.CreateDirectory("Builds/WebGL");
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL))throw new Exception("Install Unity WebGL Build Support through Unity Hub.");
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;PlayerSettings.WebGL.decompressionFallback=true;
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Checkout.unity"},locationPathName="Builds/WebGL",target=BuildTarget.WebGL,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Web build failed");
        WebLoaderFix.Apply();
        Debug.Log("BAGGING_WEB_BUILD_PASS "+report.summary.totalSize+" bytes");
    }
    public static void Configure()
    {
        PlayerSettings.companyName="Bagging Order";PlayerSettings.productName="Bagging Order";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
        PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SplashScreen.show=false;
        Directory.CreateDirectory("Assets/Scenes");var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Bagging Order").AddComponent<BaggingGame>();EditorSceneManager.SaveScene(scene,"Assets/Scenes/Checkout.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Checkout.unity",true)};AssetDatabase.SaveAssets();
    }
    public static void RulesCheck()
    {
        var items=new System.Collections.Generic.List<PackedItem>{new PackedItem{Key="apple",Id=0},new PackedItem{Key="can",Id=1,Y=1}};
        var p=PackingRules.Preview(items,"can",2,0,0,false,3);
        if(!p.Valid||!p.Crushed.Contains(0)||Math.Abs(p.Loads[0]-6)>.01f)throw new Exception("Weight propagation failed");
        if(PackingRules.Preview(items,"milk",3,0,0,false,3).Valid)throw new Exception("Height limit failed");
        var back=PackingRules.Preview(items,"can",3,0,1,false,3);if(!back.Valid||back.Item.Y!=0)throw new Exception("Two-row independence failed");
        Debug.Log("BAGGING_RULES_PASS");
        MechanicsChecks.Run();
    }
}
