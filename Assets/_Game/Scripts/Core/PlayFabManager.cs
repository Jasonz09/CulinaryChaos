using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace IOChef.Core
{
    /// <summary>
    /// Singleton manager that handles PlayFab authentication and provides
    /// server-authoritative async methods. Always-online: login must succeed
    /// before the game proceeds. Uses direct REST API calls via UnityWebRequest.
    /// </summary>
    public class PlayFabManager : MonoBehaviour
    {
        public static PlayFabManager Instance { get; private set; }

        [SerializeField] private PlayFabConfig config;

        /// <summary>True after a successful login.</summary>
        public bool IsLoggedIn { get; private set; }

        /// <summary>True while a login attempt is in progress.</summary>
        public bool IsConnecting { get; private set; }

        /// <summary>True when a valid PlayFab config with title ID is assigned.</summary>
        public bool IsConfigured => config != null && !string.IsNullOrEmpty(config.titleId);

        /// <summary>The PlayFab player ID assigned after login.</summary>
        public string PlayFabId { get; private set; }

        /// <summary>Fires on successful login.</summary>
        public event Action OnLoginSuccess;

        /// <summary>Fires when login fails, with error message.</summary>
        public event Action<string> OnLoginFailed;

        /// <summary>Fires when the connection is lost mid-session.</summary>
        public event Action OnConnectionLost;

        /// <summary>Fires when the connection is restored after being lost.</summary>
        public event Action OnConnectionRestored;

        /// <summary>Fires after CONNECTION_TIMEOUT seconds of continuous disconnection.</summary>
        public event Action OnConnectionTimeout;

        private string _sessionTicket;
        private string _baseUrl;
        private bool _wasConnected;
        private float _heartbeatTimer;
        private float _disconnectedAtRealtime = -1f;
        private bool _isPinging;
        private bool _timeoutFired;
        private const float HEARTBEAT_INTERVAL = 30f;
        private const float GAMEPLAY_HEARTBEAT_INTERVAL = 5f;
        private const float CONNECTION_TIMEOUT = 30f;
        private const string FALLBACK_TITLE_ID = "180D84";

        /// <summary>How long (seconds) we've been disconnected. 0 if connected.</summary>
        public float DisconnectedDuration =>
            _disconnectedAtRealtime < 0 ? 0f : Time.realtimeSinceStartup - _disconnectedAtRealtime;

        private string TitleId => config != null ? config.titleId : FALLBACK_TITLE_ID;

        // ═══════════════════════════════════════════════════════
        //  LIFECYCLE
        // ═══════════════════════════════════════════════════════

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Auto-load config from Resources if not assigned via Inspector
            if (config == null)
                config = Resources.Load<PlayFabConfig>("PlayFabConfig");

            if (IsConfigured)
            {
                _baseUrl = $"https://{TitleId}.playfabapi.com";
                StartCoroutine(LoginCoroutine());
            }
            else
            {
                // Fallback: use hardcoded title ID if no config asset exists
                Debug.LogWarning("[PlayFabManager] No PlayFabConfig asset found. Using hardcoded title ID.");
                _baseUrl = $"https://{TitleId}.playfabapi.com";
                StartCoroutine(LoginCoroutine());
            }
        }

        private void Update()
        {
            if (!IsLoggedIn && _disconnectedAtRealtime < 0) return;

            // Use faster heartbeat during gameplay for quicker disconnect detection
            bool inGameplay = GameManager.Instance != null &&
                (GameManager.Instance.CurrentGameState == GameState.Playing ||
                 GameManager.Instance.CurrentGameState == GameState.Paused);
            float interval = inGameplay ? GAMEPLAY_HEARTBEAT_INTERVAL : HEARTBEAT_INTERVAL;

            _heartbeatTimer += Time.unscaledDeltaTime;
            if (_heartbeatTimer >= interval)
            {
                _heartbeatTimer = 0f;
                CheckConnectivity();
            }

            // Check disconnect timeout continuously (not just on heartbeat)
            if (_disconnectedAtRealtime >= 0 && !_timeoutFired)
            {
                float elapsed = Time.realtimeSinceStartup - _disconnectedAtRealtime;
                if (elapsed >= CONNECTION_TIMEOUT)
                {
                    _timeoutFired = true;
                    Debug.LogWarning($"[PlayFabManager] Connection timeout after {CONNECTION_TIMEOUT}s");
                    OnConnectionTimeout?.Invoke();
                }
            }
        }

        private void CheckConnectivity()
        {
            // Quick check: device has no network at all
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                MarkDisconnected();
                return;
            }

            // If we were disconnected but device now has network, do an active ping
            // to confirm server is actually reachable
            if (!_wasConnected || !IsLoggedIn)
            {
                if (!_isPinging)
                    StartCoroutine(ActivePingCoroutine());
                return;
            }

            // We think we're connected — do periodic active ping to confirm
            if (!_isPinging)
                StartCoroutine(ActivePingCoroutine());
        }

        private IEnumerator ActivePingCoroutine()
        {
            if (_isPinging) yield break;
            _isPinging = true;

            string url = $"{_baseUrl}/Client/ExecuteCloudScript";
            var body = new JObject
            {
                ["FunctionName"] = "Ping",
                ["FunctionParameter"] = new JObject(),
                ["GeneratePlayStreamEvent"] = false
            };

            using var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(body.ToString());
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(_sessionTicket))
                request.SetRequestHeader("X-Authorization", _sessionTicket);
            request.timeout = 8;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MarkConnected();
            }
            else
            {
                MarkDisconnected();
            }

            _isPinging = false;
        }

        private void MarkDisconnected()
        {
            if (_wasConnected)
            {
                _wasConnected = false;
                _disconnectedAtRealtime = Time.realtimeSinceStartup;
                _timeoutFired = false;
                Debug.LogWarning("[PlayFabManager] Connection lost");
                OnConnectionLost?.Invoke();
            }
        }

        private void MarkConnected()
        {
            if (!_wasConnected && IsLoggedIn)
            {
                _wasConnected = true;
                _disconnectedAtRealtime = -1f;
                _timeoutFired = false;
                Debug.Log("[PlayFabManager] Connection restored");
                OnConnectionRestored?.Invoke();
            }
        }

        /// <summary>
        /// Force-clears login state for full game reload. Called before loading Bootstrap.
        /// </summary>
        public void ForceDisconnect()
        {
            IsLoggedIn = false;
            _sessionTicket = null;
            _wasConnected = false;
            _disconnectedAtRealtime = -1f;
            _timeoutFired = false;
            _heartbeatTimer = 0f;
            _isPinging = false;
            PlayFabId = null;
            Debug.Log("[PlayFabManager] Force disconnected for reload");
        }

        // ═══════════════════════════════════════════════════════
        //  AUTH
        // ═══════════════════════════════════════════════════════

        private IEnumerator LoginCoroutine()
        {
            // Pre-check internet
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("[PlayFabManager] No internet. Waiting for connection...");
                OnLoginFailed?.Invoke("No internet connection");
                yield break;
            }

            IsConnecting = true;

            string deviceId = SystemInfo.deviceUniqueIdentifier;
            var body = new JObject
            {
                ["CustomId"] = deviceId,
                ["CreateAccount"] = true,
                ["TitleId"] = TitleId
            };

            string responseText = null;
            string errorText = null;

            yield return PostRequest("/Client/LoginWithCustomID", body.ToString(),
                r => responseText = r, e => errorText = e, useAuth: false);

            IsConnecting = false;

            if (errorText != null)
            {
                Debug.LogWarning($"[PlayFabManager] Login failed: {errorText}");
                OnLoginFailed?.Invoke(errorText);
                yield break;
            }

            try
            {
                var json = JObject.Parse(responseText);
                _sessionTicket = json["data"]?["SessionTicket"]?.ToString();
                PlayFabId = json["data"]?["PlayFabId"]?.ToString();
                bool newlyCreated = json["data"]?["NewlyCreated"]?.Value<bool>() ?? false;
                IsLoggedIn = !string.IsNullOrEmpty(_sessionTicket);

                if (IsLoggedIn)
                {
                    _wasConnected = true;
                    Debug.Log($"[PlayFabManager] Logged in as {PlayFabId} (new={newlyCreated})");

                    // Initialize new players on the server
                    if (newlyCreated)
                    {
                        ExecuteCloudScript("InitNewPlayer", null);
                    }

                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    Debug.LogWarning("[PlayFabManager] Login response missing session ticket.");
                    OnLoginFailed?.Invoke("Missing session ticket");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayFabManager] Login parse error: {ex.Message}");
                OnLoginFailed?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Retries login. Call when the user taps a Retry button.
        /// </summary>
        public void RetryLogin()
        {
            if (IsLoggedIn || IsConnecting) return;
            if (!IsConfigured)
            {
                OnLoginFailed?.Invoke("PlayFab not configured");
                return;
            }

            _baseUrl = $"https://{config.titleId}.playfabapi.com";
            StartCoroutine(LoginCoroutine());
        }

        // ═══════════════════════════════════════════════════════
        //  ACCOUNT LINKING (Apple / Google)
        // ═══════════════════════════════════════════════════════

        // PlayFab error names for account linking conflicts
        public const string ERR_LINKED_ACCOUNT_ALREADY_CLAIMED = "LinkedAccountAlreadyClaimed";
        public const string ERR_ACCOUNT_ALREADY_LINKED = "AccountAlreadyLinked";

        /// <summary>
        /// Extracts the PlayFab error name from an error string (format: "ErrorName|message").
        /// Returns empty string if no error name prefix is present.
        /// </summary>
        public static string GetErrorName(string error)
        {
            if (string.IsNullOrEmpty(error)) return "";
            int pipe = error.IndexOf('|');
            return pipe > 0 ? error.Substring(0, pipe) : "";
        }

        /// <summary>
        /// Links an Apple account to the current player via identity token.
        /// Set forceLink=true to overwrite an existing link on another account.
        /// </summary>
        public void LinkAppleAccount(string identityToken, bool forceLink = false,
            Action onSuccess = null, Action<string> onError = null)
        {
            if (!IsLoggedIn) { onError?.Invoke("Not logged in"); return; }
            StartCoroutine(LinkAppleCoroutine(identityToken, forceLink, onSuccess, onError));
        }

        private IEnumerator LinkAppleCoroutine(string identityToken, bool forceLink,
            Action onSuccess, Action<string> onError)
        {
            var body = new JObject
            {
                ["IdentityToken"] = identityToken,
                ["ForceLink"] = forceLink
            };
            string errorText = null;
            yield return PostRequest("/Client/LinkApple", body.ToString(),
                _ => { }, e => errorText = e);

            if (errorText != null) onError?.Invoke(errorText);
            else { Debug.Log("[PlayFabManager] Apple account linked"); onSuccess?.Invoke(); }
        }

        /// <summary>
        /// Links a Google account to the current player via server auth code.
        /// Set forceLink=true to overwrite an existing link on another account.
        /// </summary>
        public void LinkGoogleAccount(string serverAuthCode, bool forceLink = false,
            Action onSuccess = null, Action<string> onError = null)
        {
            if (!IsLoggedIn) { onError?.Invoke("Not logged in"); return; }
            StartCoroutine(LinkGoogleCoroutine(serverAuthCode, forceLink, onSuccess, onError));
        }

        private IEnumerator LinkGoogleCoroutine(string serverAuthCode, bool forceLink,
            Action onSuccess, Action<string> onError)
        {
            var body = new JObject
            {
                ["ServerAuthCode"] = serverAuthCode,
                ["ForceLink"] = forceLink
            };
            string errorText = null;
            yield return PostRequest("/Client/LinkGoogleAccount", body.ToString(),
                _ => { }, e => errorText = e);

            if (errorText != null) onError?.Invoke(errorText);
            else { Debug.Log("[PlayFabManager] Google account linked"); onSuccess?.Invoke(); }
        }

        /// <summary>
        /// Logs in with an Apple identity token (for device transfer).
        /// </summary>
        public void LoginWithApple(string identityToken, Action onSuccess = null, Action<string> onError = null)
        {
            if (IsLoggedIn || IsConnecting) { onError?.Invoke("Already logged in or connecting"); return; }
            StartCoroutine(LoginWithAppleCoroutine(identityToken, onSuccess, onError));
        }

        private IEnumerator LoginWithAppleCoroutine(string identityToken, Action onSuccess, Action<string> onError)
        {
            IsConnecting = true;
            var body = new JObject
            {
                ["IdentityToken"] = identityToken,
                ["CreateAccount"] = true,
                ["TitleId"] = TitleId
            };

            string responseText = null;
            string errorText = null;
            yield return PostRequest("/Client/LoginWithApple", body.ToString(),
                r => responseText = r, e => errorText = e, useAuth: false);

            IsConnecting = false;

            if (errorText != null) { onError?.Invoke(errorText); yield break; }

            try
            {
                var json = JObject.Parse(responseText);
                _sessionTicket = json["data"]?["SessionTicket"]?.ToString();
                PlayFabId = json["data"]?["PlayFabId"]?.ToString();
                IsLoggedIn = !string.IsNullOrEmpty(_sessionTicket);
                if (IsLoggedIn) { _wasConnected = true; onSuccess?.Invoke(); OnLoginSuccess?.Invoke(); }
                else onError?.Invoke("Missing session ticket");
            }
            catch (Exception ex) { onError?.Invoke(ex.Message); }
        }

        /// <summary>
        /// Logs in with a Google server auth code (for device transfer).
        /// </summary>
        public void LoginWithGoogle(string serverAuthCode, Action onSuccess = null, Action<string> onError = null)
        {
            if (IsLoggedIn || IsConnecting) { onError?.Invoke("Already logged in or connecting"); return; }
            StartCoroutine(LoginWithGoogleCoroutine(serverAuthCode, onSuccess, onError));
        }

        private IEnumerator LoginWithGoogleCoroutine(string serverAuthCode, Action onSuccess, Action<string> onError)
        {
            IsConnecting = true;
            var body = new JObject
            {
                ["ServerAuthCode"] = serverAuthCode,
                ["CreateAccount"] = true,
                ["TitleId"] = TitleId
            };

            string responseText = null;
            string errorText = null;
            yield return PostRequest("/Client/LoginWithGoogleAccount", body.ToString(),
                r => responseText = r, e => errorText = e, useAuth: false);

            IsConnecting = false;

            if (errorText != null) { onError?.Invoke(errorText); yield break; }

            try
            {
                var json = JObject.Parse(responseText);
                _sessionTicket = json["data"]?["SessionTicket"]?.ToString();
                PlayFabId = json["data"]?["PlayFabId"]?.ToString();
                IsLoggedIn = !string.IsNullOrEmpty(_sessionTicket);
                if (IsLoggedIn) { _wasConnected = true; onSuccess?.Invoke(); OnLoginSuccess?.Invoke(); }
                else onError?.Invoke("Missing session ticket");
            }
            catch (Exception ex) { onError?.Invoke(ex.Message); }
        }

        // ═══════════════════════════════════════════════════════
        //  VIRTUAL CURRENCIES (READ-ONLY from client)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Fetches all virtual currency balances from the server.
        /// This is the ONLY way to read currencies — client never writes directly.
        /// </summary>
        public void GetVirtualCurrencies(Action<Dictionary<string, int>> onSuccess, Action<string> onError = null)
        {
            if (!IsLoggedIn) { onError?.Invoke("Not logged in"); return; }
            StartCoroutine(GetVirtualCurrenciesCoroutine(onSuccess, onError));
        }

        private IEnumerator GetVirtualCurrenciesCoroutine(
            Action<Dictionary<string, int>> onSuccess, Action<string> onError)
        {
            string responseText = null;
            string errorText = null;

            yield return PostRequest("/Client/GetUserInventory", "{}",
                r => responseText = r, e => errorText = e);

            if (errorText != null) { onError?.Invoke(errorText); yield break; }

            try
            {
                var json = JObject.Parse(responseText);
                var vc = json["data"]?["VirtualCurrency"] as JObject;
                var result = new Dictionary<string, int>();
                if (vc != null)
                    foreach (var prop in vc.Properties())
                        result[prop.Name] = prop.Value.Value<int>();
                onSuccess?.Invoke(result);
            }
            catch (Exception ex) { onError?.Invoke(ex.Message); }
        }

        /// <summary>
        /// Refreshes CurrencyManager's cache from server balances.
        /// Call after any CloudScript that modifies currencies.
        /// </summary>
        public void RefreshCurrencies()
        {
            if (!IsLoggedIn) return;

            GetVirtualCurrencies(currencies =>
            {
                if (Economy.CurrencyManager.Instance != null)
                    Economy.CurrencyManager.Instance.UpdateFromServer(currencies);
            },
            err => Debug.LogWarning($"[PlayFabManager] RefreshCurrencies failed: {err}"));
        }

        // ═══════════════════════════════════════════════════════
        //  PLAYER DATA (READ-ONLY from client)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Gets player data values for the given keys. Read-only from client.
        /// All writes happen through CloudScript handlers.
        /// </summary>
        public void GetUserData(List<string> keys,
            Action<Dictionary<string, string>> onSuccess, Action<string> onError = null)
        {
            if (!IsLoggedIn) { onError?.Invoke("Not logged in"); return; }
            StartCoroutine(GetUserDataCoroutine(keys, onSuccess, onError));
        }

        private IEnumerator GetUserDataCoroutine(List<string> keys,
            Action<Dictionary<string, string>> onSuccess, Action<string> onError)
        {
            var body = new JObject { ["Keys"] = new JArray(keys) };
            string responseText = null;
            string errorText = null;

            yield return PostRequest("/Client/GetUserData", body.ToString(),
                r => responseText = r, e => errorText = e);

            if (errorText != null) { onError?.Invoke(errorText); yield break; }

            try
            {
                var json = JObject.Parse(responseText);
                var data = json["data"]?["Data"] as JObject;
                var result = new Dictionary<string, string>();
                if (data != null)
                    foreach (var prop in data.Properties())
                        result[prop.Name] = prop.Value?["Value"]?.ToString() ?? "";
                onSuccess?.Invoke(result);
            }
            catch (Exception ex) { onError?.Invoke(ex.Message); }
        }

        // ═══════════════════════════════════════════════════════
        //  TITLE DATA (shared game configs, read-only)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Fetches Title Data values for the given keys. Title Data is shared
        /// across all players (level configs, hero catalog, etc).
        /// </summary>
        public void GetTitleData(List<string> keys,
            Action<Dictionary<string, string>> onSuccess, Action<string> onError = null)
        {
            if (!IsLoggedIn) { onError?.Invoke("Not logged in"); return; }
            StartCoroutine(GetTitleDataCoroutine(keys, onSuccess, onError));
        }

        private IEnumerator GetTitleDataCoroutine(List<string> keys,
            Action<Dictionary<string, string>> onSuccess, Action<string> onError)
        {
            var body = new JObject { ["Keys"] = new JArray(keys) };
            string responseText = null;
            string errorText = null;

            yield return PostRequest("/Client/GetTitleData", body.ToString(),
                r => responseText = r, e => errorText = e);

            if (errorText != null) { onError?.Invoke(errorText); yield break; }

            try
            {
                var json = JObject.Parse(responseText);
                var data = json["data"]?["Data"] as JObject;
                var result = new Dictionary<string, string>();
                if (data != null)
                    foreach (var prop in data.Properties())
                        result[prop.Name] = prop.Value?.ToString() ?? "";
                onSuccess?.Invoke(result);
            }
            catch (Exception ex) { onError?.Invoke(ex.Message); }
        }

        // ═══════════════════════════════════════════════════════
        //  CLOUDSCRIPT
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Executes a CloudScript function on the server. This is the ONLY way
        /// to mutate game state — all writes go through server-side validation.
        /// </summary>
        public void ExecuteCloudScript(string functionName, object args,
            Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (!IsLoggedIn) { onError?.Invoke("Not logged in"); return; }
            StartCoroutine(ExecuteCloudScriptCoroutine(functionName, args, onSuccess, onError));
        }

        private IEnumerator ExecuteCloudScriptCoroutine(string functionName, object args,
            Action<string> onSuccess, Action<string> onError)
        {
            var body = new JObject
            {
                ["FunctionName"] = functionName,
                ["FunctionParameter"] = args != null ? JObject.FromObject(args) : new JObject(),
                ["GeneratePlayStreamEvent"] = true
            };

            string responseText = null;
            string errorText = null;

            yield return PostRequest("/Client/ExecuteCloudScript", body.ToString(),
                r => responseText = r, e => errorText = e);

            if (errorText != null) { onError?.Invoke(errorText); yield break; }

            try
            {
                var json = JObject.Parse(responseText);
                var error = json["data"]?["Error"];
                if (error != null && error.Type != JTokenType.Null)
                {
                    onError?.Invoke(error.ToString());
                }
                else
                {
                    var functionResult = json["data"]?["FunctionResult"]?.ToString() ?? "";
                    onSuccess?.Invoke(functionResult);
                }
            }
            catch (Exception ex) { onError?.Invoke(ex.Message); }
        }

        // ═══════════════════════════════════════════════════════
        //  HTTP LAYER
        // ═══════════════════════════════════════════════════════

        private IEnumerator PostRequest(string endpoint, string jsonBody,
            Action<string> onSuccess, Action<string> onError, bool useAuth = true)
        {
            string url = $"{_baseUrl}{endpoint}";
            using var request = new UnityWebRequest(url, "POST");
            request.timeout = 15; // seconds — fail fast on SSL/network issues
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (useAuth && !string.IsNullOrEmpty(_sessionTicket))
                request.SetRequestHeader("X-Authorization", _sessionTicket);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = $"HTTP {request.responseCode}: {request.error}";
                onError?.Invoke(errorMsg);

                // Check if this indicates a connection problem
                if (request.result == UnityWebRequest.Result.ConnectionError && _wasConnected)
                {
                    _wasConnected = false;
                    OnConnectionLost?.Invoke();
                }
                yield break;
            }

            // Restore connection flag if we were disconnected
            if (!_wasConnected && IsLoggedIn)
            {
                _wasConnected = true;
                OnConnectionRestored?.Invoke();
            }

            string text = request.downloadHandler.text;
            try
            {
                var json = JObject.Parse(text);
                int code = json["code"]?.Value<int>() ?? 200;
                if (code != 200)
                {
                    string errorName = json["error"]?.ToString() ?? "";
                    string msg = json["errorMessage"]?.ToString() ?? "Unknown API error";
                    // Prefix with error name so callers can detect specific errors
                    string fullError = string.IsNullOrEmpty(errorName) ? msg : $"{errorName}|{msg}";
                    onError?.Invoke(fullError);
                }
                else
                {
                    onSuccess?.Invoke(text);
                }
            }
            catch
            {
                onSuccess?.Invoke(text);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  CONFIG ACCESSORS
        // ═══════════════════════════════════════════════════════

        /// <summary>Currency code for coins.</summary>
        public string CoinsCurrencyCode => config?.coinsCurrencyCode ?? "CO";

        /// <summary>Currency code for gems.</summary>
        public string GemsCurrencyCode => config?.gemsCurrencyCode ?? "GM";

        /// <summary>Currency code for hero tokens.</summary>
        public string HeroTokensCurrencyCode => config?.heroTokensCurrencyCode ?? "HT";
    }
}
