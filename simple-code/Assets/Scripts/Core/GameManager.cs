using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TestGame.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scenes")]
        [SerializeField] private string menuSceneName = SceneNames.Menu;
        [SerializeField] private string castleSceneName = SceneNames.Castle;
        [SerializeField] private string youLostSceneName = SceneNames.YouLost;
        [SerializeField] private string youWinSceneName = SceneNames.YouWin;

        [Header("Difficulty / Win Condition")]
        [Tooltip("Value needed to allow exiting the maze.")]
        [SerializeField] private int requiredValueToWin = 100;

        [Header("Loading Tip")]
        [TextArea]
        [SerializeField] private string[] loadingTips =
        {
            "Collect coins and treasure to open the exit.",
            "Use Tab to toggle the camera view.",
            "Enemies spawn after you enter the maze."
        };

        public int TotalCoinsCollected { get; private set; }
        public int TotalTreasuresCollected { get; private set; }
        public int TotalValueCollected { get; private set; }
        public int TotalVictories { get; private set; }
        public int TotalLosses { get; private set; }

        public int LevelCoinsCollected { get; private set; }
        public int LevelTreasuresCollected { get; private set; }
        public int LevelValueCollected { get; private set; }

        public bool IsPaused { get; private set; }

        public event Action<bool> PauseChanged;
        public event Action<int> LevelValueChanged;

        private LoadingOverlay _loadingOverlay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureLoadingOverlay();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.Escape))
#endif
            {
                if (IsPaused) Resume();
                else Pause();
            }
        }

        public void StartGame()
        {
            ResetLevelProgress();
            StartCoroutine(LoadSceneAsync(castleSceneName));
        }

        public void ReturnToMenu()
        {
            StartCoroutine(LoadSceneAsync(menuSceneName));
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void LoadYouLost()
        {
            TotalLosses++;
            StartCoroutine(LoadSceneAsync(youLostSceneName));
        }

        public void LoadYouWin()
        {
            TotalVictories++;
            StartCoroutine(LoadSceneAsync(youWinSceneName));
        }

        public void AddCollectible(int value, bool isTreasure)
        {
            LevelValueCollected += value;
            TotalValueCollected += value;

            if (isTreasure)
            {
                LevelTreasuresCollected++;
                TotalTreasuresCollected++;
            }
            else
            {
                LevelCoinsCollected++;
                TotalCoinsCollected++;
            }

            LevelValueChanged?.Invoke(LevelValueCollected);
        }

        public bool CanExitMaze()
        {
            return LevelValueCollected >= requiredValueToWin;
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            Time.timeScale = 0f;
            PauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            PauseChanged?.Invoke(false);
        }

        private void ResetLevelProgress()
        {
            LevelCoinsCollected = 0;
            LevelTreasuresCollected = 0;
            LevelValueCollected = 0;
            LevelValueChanged?.Invoke(LevelValueCollected);
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            Resume();
            EnsureLoadingOverlay();
            _loadingOverlay.Show(GetRandomTip());

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            op.allowSceneActivation = true;

            while (!op.isDone)
            {
                _loadingOverlay.SetProgress(op.progress);
                yield return null;
            }

            _loadingOverlay.Hide();
        }

        private string GetRandomTip()
        {
            if (loadingTips == null || loadingTips.Length == 0) return string.Empty;
            return loadingTips[UnityEngine.Random.Range(0, loadingTips.Length)];
        }

        private void EnsureLoadingOverlay()
        {
            if (_loadingOverlay != null) return;
            GameObject overlayGo = new GameObject("Loading Overlay");
            overlayGo.transform.SetParent(transform, false);
            _loadingOverlay = overlayGo.AddComponent<LoadingOverlay>();
        }

        private sealed class LoadingOverlay : MonoBehaviour
        {
            private Canvas _canvas;
            private UnityEngine.UI.Image _bar;
            private UnityEngine.UI.Text _tip;

            private void Awake()
            {
                _canvas = gameObject.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                GameObject panel = new GameObject("Panel");
                panel.transform.SetParent(transform, false);
                var panelImg = panel.AddComponent<UnityEngine.UI.Image>();
                panelImg.color = new Color(0f, 0f, 0f, 0.65f);
                var rtPanel = panel.GetComponent<RectTransform>();
                rtPanel.anchorMin = Vector2.zero;
                rtPanel.anchorMax = Vector2.one;
                rtPanel.offsetMin = Vector2.zero;
                rtPanel.offsetMax = Vector2.zero;

                GameObject tipGo = new GameObject("Tip");
                tipGo.transform.SetParent(panel.transform, false);
                _tip = tipGo.AddComponent<UnityEngine.UI.Text>();
                _tip.alignment = TextAnchor.MiddleCenter;
                _tip.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                _tip.color = Color.white;
                var rtTip = tipGo.GetComponent<RectTransform>();
                rtTip.anchorMin = new Vector2(0.1f, 0.55f);
                rtTip.anchorMax = new Vector2(0.9f, 0.85f);
                rtTip.offsetMin = Vector2.zero;
                rtTip.offsetMax = Vector2.zero;

                GameObject barBgGo = new GameObject("Bar BG");
                barBgGo.transform.SetParent(panel.transform, false);
                var barBg = barBgGo.AddComponent<UnityEngine.UI.Image>();
                barBg.color = new Color(1f, 1f, 1f, 0.25f);
                var rtBg = barBgGo.GetComponent<RectTransform>();
                rtBg.anchorMin = new Vector2(0.2f, 0.35f);
                rtBg.anchorMax = new Vector2(0.8f, 0.45f);
                rtBg.offsetMin = Vector2.zero;
                rtBg.offsetMax = Vector2.zero;

                GameObject barGo = new GameObject("Bar");
                barGo.transform.SetParent(barBgGo.transform, false);
                _bar = barGo.AddComponent<UnityEngine.UI.Image>();
                _bar.color = Color.white;
                var rtBar = barGo.GetComponent<RectTransform>();
                rtBar.anchorMin = new Vector2(0f, 0f);
                rtBar.anchorMax = new Vector2(0f, 1f);
                rtBar.pivot = new Vector2(0f, 0.5f);
                rtBar.offsetMin = Vector2.zero;
                rtBar.offsetMax = Vector2.zero;

                Hide();
            }

            public void Show(string tip)
            {
                if (_canvas != null) _canvas.enabled = true;
                if (_tip != null) _tip.text = tip;
                SetProgress(0f);
            }

            public void Hide()
            {
                if (_canvas != null) _canvas.enabled = false;
            }

            public void SetProgress(float progress)
            {
                if (_bar == null) return;
                float clamped = Mathf.Clamp01(progress);
                RectTransform rt = _bar.GetComponent<RectTransform>();
                rt.anchorMax = new Vector2(clamped, 1f);
            }
        }
    }
}
