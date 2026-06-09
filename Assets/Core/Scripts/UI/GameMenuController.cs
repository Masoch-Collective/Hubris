using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace UI {

    public sealed class GameMenuController : Singleton<GameMenuController> {
        private enum MenuState {
            Main,
            Playing,
            Paused,
            Options,
            Credits
        }

        private const string PrefabPath = "Prefabs/GameMenu";

        [SerializeField] private GameObject _overlay;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _optionsPanel;
        [SerializeField] private GameObject _creditsPanel;

        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Toggle _muteToggle;
        [SerializeField] private TextMeshProUGUI _volumeLabel;

        private MenuState _state = MenuState.Main;
        private MenuState _returnState = MenuState.Main;
        private bool _muted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap() {
            if (FindAnyObjectByType<GameMenuController>() != null)
                return;

            GameMenuController prefab = Resources.Load<GameMenuController>(PrefabPath);
            if (prefab == null) {
                Debug.LogError($"Could not load menu prefab at Resources/{PrefabPath}.prefab.");
                return;
            }

            GameMenuController menu = Instantiate(prefab, GlobalCanvas.Instance.Root);
            menu.name = nameof(GameMenuController);
        }

        private void Awake() {
            InitializeControls();
            ShowMainMenu();
        }

        private void Update() {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
                return;

            if (_state == MenuState.Playing)
                PauseGame();
            else if (_state == MenuState.Paused)
                ResumeGame();
            else if (_state == MenuState.Options || _state == MenuState.Credits)
                ShowState(_returnState);
        }
 
        private void InitializeControls() {
            if (_muteToggle != null)
                _muteToggle.SetIsOnWithoutNotify(_muted);

            if (_volumeSlider != null)
                _volumeSlider.SetValueWithoutNotify(AudioListener.volume);
            SetVolume(AudioListener.volume);
        }

        public void StartGame() {
            _state = MenuState.Playing;
            Time.timeScale = 1f;
            ShowState(MenuState.Playing);
        }

        private void PauseGame() {
            _state = MenuState.Paused;
            Time.timeScale = 0f;
            ShowState(MenuState.Paused);
        }

        public void ResumeGame() {
            _state = MenuState.Playing;
            Time.timeScale = 1f;
            ShowState(MenuState.Playing);
        }

        private void ShowMainMenu() {
            _returnState = MenuState.Main;
            Time.timeScale = 0f;
            ShowState(MenuState.Main);
        }

        public void EndGame() {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(activeScene.path))
                SceneManager.LoadScene(activeScene.path);
            else
                SceneManager.LoadScene(activeScene.buildIndex);

            ShowMainMenu();
        }

        public void OpenOptions() => OpenSubmenu(MenuState.Options);

        public void OpenCredits() => OpenSubmenu(MenuState.Credits);

        public void ReturnToPreviousMenu() => ShowState(_returnState);

        private void OpenSubmenu(MenuState submenu) {
            _returnState = _state;
            ShowState(submenu);
        }

        private void ShowState(MenuState state) {
            _state = state;
            bool menuVisible = state != MenuState.Playing;

            if (_overlay != null)
                _overlay.SetActive(menuVisible);
            if (_mainPanel != null)
                _mainPanel.SetActive(state == MenuState.Main);
            if (_pausePanel != null)
                _pausePanel.SetActive(state == MenuState.Paused);
            if (_optionsPanel != null)
                _optionsPanel.SetActive(state == MenuState.Options);
            if (_creditsPanel != null)
                _creditsPanel.SetActive(state == MenuState.Credits);
        }

        public void SetMuted(bool muted) {
            _muted = muted;
            AudioListener.pause = muted;
        }

        public void SetVolume(float volume) {
            AudioListener.volume = volume;
            if (_volumeLabel != null)
                _volumeLabel.text = $"Volume: {Mathf.RoundToInt(volume * 100f)}%";
        }

        public void ExitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
