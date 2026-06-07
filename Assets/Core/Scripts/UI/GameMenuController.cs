using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;

namespace UI {

    public sealed class GameMenuController : MonoBehaviour {
        private enum MenuState {
            Main,
            Playing,
            Paused,
            Options,
            Credits
        }

        private const string PrefabPath = "Prefabs/GameMenu";
        private static GameMenuController _instance;

        [SerializeField] private GameObject _overlay;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _optionsPanel;
        [SerializeField] private GameObject _creditsPanel;

        [SerializeField] private UnityButton _startButton;
        [SerializeField] private UnityButton _attractModeButton;
        [SerializeField] private UnityButton _mainOptionsButton;
        [SerializeField] private UnityButton _creditsButton;
        [SerializeField] private UnityButton _exitButton;
        [SerializeField] private UnityButton _resumeButton;
        [SerializeField] private UnityButton _pauseOptionsButton;
        [SerializeField] private UnityButton _endGameButton;
        [SerializeField] private UnityButton _optionsBackButton;
        [SerializeField] private UnityButton _creditsBackButton;

        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Toggle _muteToggle;
        [SerializeField] private TextMeshProUGUI _volumeLabel;

        private MenuState _state = MenuState.Main;
        private MenuState _returnState = MenuState.Main;
        private bool _muted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap() {
            if (_instance != null)
                return;

            GameMenuController prefab = Resources.Load<GameMenuController>(PrefabPath);
            if (prefab == null) {
                Debug.LogError($"Could not load menu prefab at Resources/{PrefabPath}.prefab.");
                return;
            }

            GameMenuController menu = Instantiate(prefab, GlobalCanvas.Instance.Root);
            menu.name = nameof(GameMenuController);
            _instance = menu;
        }

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            BindControls();
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
 
        private void BindControls() {
            Bind(_startButton, StartGame);
            Bind(_attractModeButton, StartGame);
            Bind(_mainOptionsButton, () => OpenSubmenu(MenuState.Options));
            Bind(_creditsButton, () => OpenSubmenu(MenuState.Credits));
            Bind(_exitButton, ExitGame);
            Bind(_resumeButton, ResumeGame);
            Bind(_pauseOptionsButton, () => OpenSubmenu(MenuState.Options));
            Bind(_endGameButton, EndGame);
            Bind(_optionsBackButton, () => ShowState(_returnState));
            Bind(_creditsBackButton, () => ShowState(_returnState));

            if (_muteToggle != null) {
                _muteToggle.isOn = _muted;
                _muteToggle.onValueChanged.RemoveListener(SetMuted);
                _muteToggle.onValueChanged.AddListener(SetMuted);
            }

            if (_volumeSlider != null) {
                _volumeSlider.value = AudioListener.volume;
                _volumeSlider.onValueChanged.RemoveListener(SetVolume);
                _volumeSlider.onValueChanged.AddListener(SetVolume);
                SetVolume(_volumeSlider.value);
            }
        }

        private static void Bind(UnityButton button, UnityEngine.Events.UnityAction action) {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void StartGame() {
            _state = MenuState.Playing;
            Time.timeScale = 1f;
            ShowState(MenuState.Playing);
        }

        private void PauseGame() {
            _state = MenuState.Paused;
            Time.timeScale = 0f;
            ShowState(MenuState.Paused);
        }

        private void ResumeGame() {
            _state = MenuState.Playing;
            Time.timeScale = 1f;
            ShowState(MenuState.Playing);
        }

        private void ShowMainMenu() {
            _returnState = MenuState.Main;
            Time.timeScale = 0f;
            ShowState(MenuState.Main);
        }

        private void EndGame() {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(activeScene.path))
                SceneManager.LoadScene(activeScene.path);
            else
                SceneManager.LoadScene(activeScene.buildIndex);

            ShowMainMenu();
        }

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

        private void SetMuted(bool muted) {
            _muted = muted;
            AudioListener.pause = muted;
        }

        private void SetVolume(float volume) {
            AudioListener.volume = volume;
            if (_volumeLabel != null)
                _volumeLabel.text = $"Volume: {Mathf.RoundToInt(volume * 100f)}%";
        }

        private static void ExitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
