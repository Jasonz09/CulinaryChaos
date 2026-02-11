using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IOChef.Core;

namespace IOChef.UI
{
    /// <summary>
    /// Full-screen blocking overlay that prevents game access without server connection.
    /// During gameplay: semi-transparent with countdown progress bar (gameplay continues).
    /// In menus: fully blocking overlay.
    /// After 30s timeout: force-reloads to Bootstrap for fresh login.
    /// </summary>
    public class ConnectionGateUI : MonoBehaviour
    {
        public static ConnectionGateUI Instance { get; private set; }

        private Canvas _canvas;
        private GameObject _panel;
        private Image _panelImage;
        private TextMeshProUGUI _statusText;
        private Button _retryButton;
        private TextMeshProUGUI _retryButtonText;

        // Progress bar elements
        private GameObject _progressBarContainer;
        private Image _progressBarFill;
        private TextMeshProUGUI _countdownText;

        private bool _isGameplayMode;
        private float _spinnerAngle;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildUI();
            ShowConnecting();
        }

        private void Start()
        {
            if (PlayFabManager.Instance != null)
            {
                PlayFabManager.Instance.OnLoginSuccess += OnLoginSuccess;
                PlayFabManager.Instance.OnLoginFailed += OnLoginFailed;
                PlayFabManager.Instance.OnConnectionLost += OnConnectionLost;
                PlayFabManager.Instance.OnConnectionRestored += OnConnectionRestored;
                PlayFabManager.Instance.OnConnectionTimeout += OnConnectionTimeout;

                if (PlayFabManager.Instance.IsLoggedIn)
                    Hide();
                else if (PlayFabManager.Instance.IsConnecting)
                    ShowConnecting();
            }
        }

        private void Update()
        {
            // Update progress bar during gameplay reconnection
            if (_isGameplayMode && _panel.activeSelf && PlayFabManager.Instance != null)
            {
                float elapsed = PlayFabManager.Instance.DisconnectedDuration;
                float remaining = Mathf.Max(0f, 30f - elapsed);
                float pct = remaining / 30f;

                if (_progressBarFill != null)
                    _progressBarFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(pct), 1f);

                if (_countdownText != null)
                    _countdownText.text = $"Reconnecting... {Mathf.CeilToInt(remaining)}s";

                // Color shifts from green to yellow to red
                Color barColor;
                if (pct > 0.5f)
                    barColor = Color.Lerp(new Color(1f, 0.8f, 0f), new Color(0.2f, 0.8f, 0.2f), (pct - 0.5f) * 2f);
                else
                    barColor = Color.Lerp(new Color(0.9f, 0.2f, 0.15f), new Color(1f, 0.8f, 0f), pct * 2f);

                if (_progressBarFill != null)
                    _progressBarFill.color = barColor;
            }
        }

        private void BuildUI()
        {
            // Canvas
            var canvasGO = new GameObject("ConnectionGateCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Full-screen panel
            _panel = new GameObject("Panel");
            _panel.transform.SetParent(canvasGO.transform, false);
            var panelRT = _panel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

            _panelImage = _panel.AddComponent<Image>();
            _panelImage.color = new Color(0.08f, 0.08f, 0.12f, 1f);

            // Status text
            var textGO = new GameObject("StatusText");
            textGO.transform.SetParent(_panel.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.1f, 0.45f);
            textRT.anchorMax = new Vector2(0.9f, 0.6f);
            textRT.offsetMin = textRT.offsetMax = Vector2.zero;

            _statusText = textGO.AddComponent<TextMeshProUGUI>();
            _statusText.text = "Connecting...";
            _statusText.fontSize = 42;
            _statusText.alignment = TextAlignmentOptions.Center;
            _statusText.color = Color.white;

            // Retry button (hidden initially)
            var btnGO = new GameObject("RetryButton");
            btnGO.transform.SetParent(_panel.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.3f, 0.3f);
            btnRT.anchorMax = new Vector2(0.7f, 0.4f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.3f, 1f);

            _retryButton = btnGO.AddComponent<Button>();
            _retryButton.targetGraphic = btnImg;
            _retryButton.onClick.AddListener(OnRetryClicked);

            var btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            var btnTextRT = btnTextGO.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = btnTextRT.offsetMax = Vector2.zero;

            _retryButtonText = btnTextGO.AddComponent<TextMeshProUGUI>();
            _retryButtonText.text = "Retry";
            _retryButtonText.fontSize = 36;
            _retryButtonText.alignment = TextAlignmentOptions.Center;
            _retryButtonText.color = Color.white;

            _retryButton.gameObject.SetActive(false);

            // Progress bar container (for gameplay reconnection mode)
            _progressBarContainer = new GameObject("ProgressBar");
            _progressBarContainer.transform.SetParent(_panel.transform, false);
            var pbRT = _progressBarContainer.AddComponent<RectTransform>();
            pbRT.anchorMin = new Vector2(0.15f, 0.38f);
            pbRT.anchorMax = new Vector2(0.85f, 0.42f);
            pbRT.offsetMin = pbRT.offsetMax = Vector2.zero;

            // Bar background
            var barBgImg = _progressBarContainer.AddComponent<Image>();
            barBgImg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            // Bar fill
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(_progressBarContainer.transform, false);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;

            _progressBarFill = fillGO.AddComponent<Image>();
            _progressBarFill.color = new Color(0.2f, 0.8f, 0.2f);

            // Countdown text (below progress bar)
            var cdGO = new GameObject("CountdownText");
            cdGO.transform.SetParent(_panel.transform, false);
            var cdRT = cdGO.AddComponent<RectTransform>();
            cdRT.anchorMin = new Vector2(0.1f, 0.32f);
            cdRT.anchorMax = new Vector2(0.9f, 0.38f);
            cdRT.offsetMin = cdRT.offsetMax = Vector2.zero;

            _countdownText = cdGO.AddComponent<TextMeshProUGUI>();
            _countdownText.text = "Reconnecting... 30s";
            _countdownText.fontSize = 28;
            _countdownText.alignment = TextAlignmentOptions.Center;
            _countdownText.color = new Color(0.9f, 0.9f, 0.9f);

            _progressBarContainer.SetActive(false);
            _countdownText.gameObject.SetActive(false);
        }

        /// <summary>Shows "Connecting..." state with no button.</summary>
        public void ShowConnecting()
        {
            _isGameplayMode = false;
            _panel.SetActive(true);
            _panelImage.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            _panelImage.raycastTarget = true;
            _statusText.text = "Connecting...";
            _retryButton.gameObject.SetActive(false);
            _progressBarContainer.SetActive(false);
            _countdownText.gameObject.SetActive(false);
        }

        /// <summary>Shows "Connection Failed" state with Retry button.</summary>
        public void ShowFailed(string reason = null)
        {
            _isGameplayMode = false;
            _panel.SetActive(true);
            _panelImage.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            _panelImage.raycastTarget = true;
            _statusText.text = string.IsNullOrEmpty(reason)
                ? "Connection Failed\nPlease check your internet and try again."
                : $"Connection Failed\n{reason}";
            _retryButton.gameObject.SetActive(true);
            _progressBarContainer.SetActive(false);
            _countdownText.gameObject.SetActive(false);
        }

        /// <summary>Shows full-blocking "Reconnecting..." for menus.</summary>
        public void ShowReconnecting()
        {
            _isGameplayMode = false;
            _panel.SetActive(true);
            _panelImage.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            _panelImage.raycastTarget = true;
            _statusText.text = "Reconnecting...";
            _retryButton.gameObject.SetActive(true);
            _progressBarContainer.SetActive(false);
            _countdownText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows semi-transparent overlay with progress bar during gameplay.
        /// Does NOT block gameplay input underneath.
        /// </summary>
        public void ShowGameplayReconnecting()
        {
            _isGameplayMode = true;
            _panel.SetActive(true);
            // Semi-transparent: gameplay visible underneath
            _panelImage.color = new Color(0.08f, 0.08f, 0.12f, 0.6f);
            // Don't block input — let gameplay continue
            _panelImage.raycastTarget = false;
            _statusText.text = "CONNECTION LOST";
            _statusText.fontSize = 42;
            _retryButton.gameObject.SetActive(false);
            _progressBarContainer.SetActive(true);
            _countdownText.gameObject.SetActive(true);
        }

        /// <summary>Hides the overlay.</summary>
        public void Hide()
        {
            _isGameplayMode = false;
            _panel.SetActive(false);
        }

        private void OnRetryClicked()
        {
            ShowConnecting();
            PlayFabManager.Instance?.RetryLogin();
        }

        private void OnLoginSuccess()
        {
            Hide();
        }

        private void OnLoginFailed(string error)
        {
            ShowFailed(error);
        }

        private void OnConnectionLost()
        {
            // Check if we're in gameplay — use non-blocking overlay with progress bar
            bool inGameplay = GameManager.Instance != null &&
                (GameManager.Instance.CurrentGameState == GameState.Playing ||
                 GameManager.Instance.CurrentGameState == GameState.Paused);

            if (inGameplay)
                ShowGameplayReconnecting();
            else
                ShowReconnecting();
        }

        private void OnConnectionRestored()
        {
            Hide();
        }

        private void OnConnectionTimeout()
        {
            // 30 seconds elapsed without reconnection — force reload
            _isGameplayMode = false;
            _panel.SetActive(true);
            _panelImage.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            _panelImage.raycastTarget = true;
            _statusText.text = "Connection Lost\nReloading...";
            _retryButton.gameObject.SetActive(false);
            _progressBarContainer.SetActive(false);
            _countdownText.gameObject.SetActive(false);

            // Force reload after a brief delay so user sees the message
            StartCoroutine(ForceReloadCoroutine());
        }

        private System.Collections.IEnumerator ForceReloadCoroutine()
        {
            yield return new WaitForSecondsRealtime(1.5f);

            PlayFabManager.Instance?.ForceDisconnect();

            if (GameManager.Instance != null)
                GameManager.Instance.ForceReloadGame();
            else
            {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene("Bootstrap");
            }
        }

        private void OnDestroy()
        {
            if (PlayFabManager.Instance != null)
            {
                PlayFabManager.Instance.OnLoginSuccess -= OnLoginSuccess;
                PlayFabManager.Instance.OnLoginFailed -= OnLoginFailed;
                PlayFabManager.Instance.OnConnectionLost -= OnConnectionLost;
                PlayFabManager.Instance.OnConnectionRestored -= OnConnectionRestored;
                PlayFabManager.Instance.OnConnectionTimeout -= OnConnectionTimeout;
            }
        }
    }
}
