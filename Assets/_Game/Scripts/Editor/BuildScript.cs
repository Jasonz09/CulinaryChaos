using UnityEditor;
using UnityEngine;

namespace IOChef.Editor
{
    public static class BuildScript
    {
        [MenuItem("IOChef/Build macOS")]
        public static void BuildOSX()
        {
            string[] scenes = new string[]
            {
                "Assets/_Game/Scenes/Bootstrap.unity",
                "Assets/_Game/Scenes/MainMenu.unity",
                "Assets/_Game/Scenes/LevelSelect.unity",
                "Assets/_Game/Scenes/Kitchen_Level_1.unity",
                "Assets/_Game/Scenes/Results.unity"
            };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/IOChef.app",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] Build succeeded: {report.summary.totalSize} bytes");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[BuildScript] Build failed: {report.summary.result}");
                foreach (var step in report.steps)
                    foreach (var msg in step.messages)
                        if (msg.type == LogType.Error)
                            Debug.LogError(msg.content);
                EditorApplication.Exit(1);
            }
        }
    }
}
