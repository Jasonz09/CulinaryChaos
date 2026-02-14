using UnityEditor;
using UnityEngine;

public class AutoBuildPlayer
{
    [MenuItem("Build/Build Windows Player")]
    public static void BuildGame()
    {
        string[] scenes = { "Assets/_Game/Scenes/MainMenu.unity", "Assets/_Game/Scenes/Kitchen.unity" };
        
        // Find all enabled scenes in build settings
        var scenesInBuild = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenesInBuild.Add(scene.path);
            }
        }
        
        string buildPath = "Build/CulinaryChaos.exe";
        
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenesInBuild.Count > 0 ? scenesInBuild.ToArray() : scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        Debug.Log("Starting build to: " + buildPath);
        var report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + report.summary.totalSize + " bytes");
            EditorApplication.Exit(0);
}
        else
        {
            Debug.LogError("Build failed!");
            EditorApplication.Exit(1);
        }
    }
}
