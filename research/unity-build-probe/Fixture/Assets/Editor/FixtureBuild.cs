using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using SampleGame;
public static class FixtureBuild
{
    public static void Run()
    {
        string output = Environment.GetEnvironmentVariable("KS_PROBE_OUTPUT");
        if (String.IsNullOrEmpty(output)) throw new Exception("KS_PROBE_OUTPUT required");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone, 1);
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Disabled);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Game").AddComponent<GameRunner>();
        Directory.CreateDirectory("Assets/Fixture");
        File.WriteAllText("Assets/Fixture/message.txt", "kingsSafe fixture: asset bytes unchanged\n");
        AssetDatabase.Refresh();
        EditorSceneManager.SaveScene(scene, "Assets/Fixture/Main.unity");
        Directory.CreateDirectory("Assets/StreamingAssets");
        var bundle = new AssetBundleBuild { assetBundleName = "fixture.bundle", assetNames = new[]{ "Assets/Fixture/message.txt" } };
        if (BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", new[]{bundle}, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneOSX) == null)
            throw new Exception("bundle build failed");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[]{"Assets/Fixture/Main.unity"}, locationPathName=output,
            target=BuildTarget.StandaloneOSX, options=BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Player build failed: " + report.summary.result);
        Debug.Log("PROBE_BUILD_PASS " + output);
    }
}
