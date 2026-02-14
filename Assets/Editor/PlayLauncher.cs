using UnityEditor;

public static class PlayLauncher
{
    public static void StartPlayMode()
    {
        EditorApplication.isPlaying = true;
    }
}
