using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BatchBuilder
{
    // Called via: Unity -batchmode -projectPath <path> -executeMethod BatchBuilder.PerformBuild
    public static void PerformBuild()
    {
        try
        {
            // Collect scenes from Assets/_Game/Scenes
            var scenePath = "Assets/_Game/Scenes";
            var sceneFiles = Directory.GetFiles(scenePath, "*.unity", SearchOption.TopDirectoryOnly);

            if (sceneFiles.Length == 0)
            {
                Debug.LogError("[BatchBuilder] No scenes found in Assets/_Game/Scenes. Aborting build.");
                EditorApplication.Exit(1);
                return;
            }

            // Set Editor build settings scenes (use relative paths)
            var builderScenes = sceneFiles.Select(s => new EditorBuildSettingsScene(s, true)).ToArray();
            EditorBuildSettings.scenes = builderScenes;

            // Ensure build output folder exists
            var buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Build");
            if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

            var buildPath = Path.Combine(buildDir, "CulinaryChaos.exe");

            var opts = new BuildPlayerOptions
            {
                scenes = sceneFiles,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[BatchBuilder] Starting build...");
            var report = BuildPipeline.BuildPlayer(opts);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BatchBuilder] Build succeeded: {buildPath}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[BatchBuilder] Build failed: {report.summary.result}");
                EditorApplication.Exit(1);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BatchBuilder] Exception during build: {ex}");
            EditorApplication.Exit(1);
        }
    }
}
