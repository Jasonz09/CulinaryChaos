using UnityEngine;

namespace IOChef.UI
{
    /// <summary>
    /// Forces TextMeshPro settings to load before any scene starts.
    /// Fixes NullReferenceException in TMP_Settings.get_autoSizeTextContainer
    /// when Resources.Load fails to find TMP Settings in builds.
    /// </summary>
    public static class TMPInitializer
    {
        private static Object _tmpSettingsRef;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            // Force-load TMP Settings from Resources to prevent it from being null
            _tmpSettingsRef = Resources.Load("TMP Settings");

            if (_tmpSettingsRef == null)
            {
                // Try alternate paths
                _tmpSettingsRef = Resources.Load("TMP_Settings");
                if (_tmpSettingsRef == null)
                {
                    _tmpSettingsRef = Resources.Load("TextMesh Pro/TMP Settings");
                }
            }

            if (_tmpSettingsRef != null)
            {
                Debug.Log($"[TMPInitializer] TMP Settings loaded successfully: {_tmpSettingsRef.name}");

                // Force TMP_Settings.instance to initialize by accessing it via reflection
                try
                {
                    var settingsType = System.Type.GetType("TMPro.TMP_Settings, Unity.TextMeshPro");
                    if (settingsType == null)
                        settingsType = System.Type.GetType("TMPro.TMP_Settings, Unity.ugui");

                    if (settingsType != null)
                    {
                        var instanceField = settingsType.GetField("s_Instance",
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                        if (instanceField != null)
                        {
                            instanceField.SetValue(null, _tmpSettingsRef);
                            Debug.Log("[TMPInitializer] TMP_Settings.s_Instance set via reflection");
                        }
                        else
                        {
                            // Try property getter to trigger lazy load
                            var instanceProp = settingsType.GetProperty("instance",
                                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                            instanceProp?.GetValue(null);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[TMPInitializer] Reflection setup failed (non-critical): {e.Message}");
                }
            }
            else
            {
                Debug.LogError("[TMPInitializer] FAILED to load TMP Settings from Resources! " +
                    "Text rendering will be broken. Ensure TMP Essential Resources are imported.");
            }
        }
    }
}
