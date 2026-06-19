using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
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
        private const string DefaultMainMenuScenePath = "Assets/Core/Scenes/MainMenu.unity";
        private const string DefaultGameplayScenePath = "Assets/Core/Scenes/BuildScene.unity";
        private const string DefaultAttractScenePath = "Assets/Core/Scenes/AttractScene.unity";
        private const string AudioVolumeKey = "GameMenu.Audio.Volume";
        private const string AudioMutedKey = "GameMenu.Audio.Muted";
        private const string MasterBusPath = "bus:/";

        [SerializeField] private string mainMenuScenePath = DefaultMainMenuScenePath;
        [SerializeField] private string gameplayScenePath = DefaultGameplayScenePath;
        [SerializeField] private string attractScenePath = DefaultAttractScenePath;
        [SerializeField, Min(0f)] private float autoAttractDelay = 6f;
        [SerializeField] private GameObject overlay;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject creditsPanel;

        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private TextMeshProUGUI volumeLabel;

        private MenuState menuState = MenuState.Main;
        private MenuState _returnState = MenuState.Main;
        private Coroutine _autoAttractRoutine;
        private bool _muted;
        private float _volume = 1f;
        private bool _fmodWarningLogged;

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
            menu.ShowStateForScene(SceneManager.GetActiveScene());
        }

        private void Awake() {
            InitializeControls();
            ShowStateForScene(SceneManager.GetActiveScene());
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            CancelAutoAttract();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (mode == LoadSceneMode.Single)
                ShowStateForScene(scene);
            ApplyAudioSettings();
        }

        private void Update() {
            Scene activeScene = SceneManager.GetActiveScene();

            if (menuState == MenuState.Main && IsMainMenuScene(activeScene) && HasUserInput())
                StartAutoAttract();

            if (IsAttractScene(activeScene))
                return;

            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
                return;

            if (menuState == MenuState.Playing)
                PauseGame();
            else if (menuState == MenuState.Paused)
                ResumeGame();
            else if (menuState == MenuState.Options || menuState == MenuState.Credits)
                ShowState(_returnState);
        }
 
        private void InitializeControls() {
            _volume = Mathf.Clamp01(PlayerPrefs.GetFloat(AudioVolumeKey, AudioListener.volume));
            _muted = PlayerPrefs.GetInt(AudioMutedKey, 0) != 0;

            if (muteToggle != null)
                muteToggle.SetIsOnWithoutNotify(_muted);

            if (volumeSlider != null)
                volumeSlider.SetValueWithoutNotify(_volume);
            ApplyAudioSettings();
            UpdateVolumeLabel();
        }

        public void StartGame() {
            CancelAutoAttract();
            Time.timeScale = 1f;
            ShowState(MenuState.Playing);
            LoadScene(gameplayScenePath);
        }

        private void PauseGame() {
            menuState = MenuState.Paused;
            Time.timeScale = 0f;
            ShowState(MenuState.Paused);
        }

        public void ResumeGame() {
            menuState = MenuState.Playing;
            Time.timeScale = 1f;
            ShowState(MenuState.Playing);
        }

        private void ShowMainMenu() {
            _returnState = MenuState.Main;
            Time.timeScale = 0f;
            ShowState(MenuState.Main);
            StartAutoAttract();
        }

        public void EndGame() {
            CancelAutoAttract();
            Time.timeScale = 1f;
            LoadScene(mainMenuScenePath);
        }

        public void OpenOptions() => OpenSubmenu(MenuState.Options);

        public void OpenCredits() => OpenSubmenu(MenuState.Credits);

        public void ReturnToPreviousMenu() {
            ShowState(_returnState);
            if (_returnState == MenuState.Main)
                StartAutoAttract();
        }

        private void OpenSubmenu(MenuState submenu) {
            CancelAutoAttract();
            _returnState = menuState;
            ShowState(submenu);
        }

        private void ShowStateForScene(Scene scene) {
            if (IsMainMenuScene(scene)) {
                ShowMainMenu();
                return;
            }

            Time.timeScale = 1f;
            CancelAutoAttract();
            ShowState(MenuState.Playing);
        }

        private bool IsMainMenuScene(Scene scene) {
            return scene.path == mainMenuScenePath;
        }

        private bool IsAttractScene(Scene scene) {
            return scene.path == attractScenePath;
        }

        private static void LoadScene(string scenePath) {
            if (!Application.CanStreamedLevelBeLoaded(scenePath)) {
                Debug.LogError($"Scene '{scenePath}' is not in Build Settings.");
                return;
            }

            SceneManager.LoadScene(scenePath);
        }

        private void StartAutoAttract() {
            CancelAutoAttract();

            if (autoAttractDelay <= 0f || string.IsNullOrWhiteSpace(attractScenePath))
                return;

            _autoAttractRoutine = StartCoroutine(AutoAttractRoutine());
        }

        private void CancelAutoAttract() {
            if (_autoAttractRoutine == null)
                return;

            StopCoroutine(_autoAttractRoutine);
            _autoAttractRoutine = null;
        }

        private IEnumerator AutoAttractRoutine() {
            yield return new WaitForSecondsRealtime(autoAttractDelay);
            _autoAttractRoutine = null;

            if (menuState != MenuState.Main || SceneManager.GetActiveScene().path != mainMenuScenePath)
                yield break;

            Time.timeScale = 1f;
            ShowState(MenuState.Playing);
            LoadScene(attractScenePath);
        }

        private static bool HasUserInput() {
            return HasKeyboardInput() || HasMouseInput() || HasGamepadInput();
        }

        private static bool HasKeyboardInput() {
            return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        }

        private static bool HasMouseInput() {
            if (Mouse.current == null)
                return false;

            return Mouse.current.leftButton.wasPressedThisFrame
                || Mouse.current.rightButton.wasPressedThisFrame
                || Mouse.current.middleButton.wasPressedThisFrame
                || Mouse.current.backButton.wasPressedThisFrame
                || Mouse.current.forwardButton.wasPressedThisFrame
                || Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f
                || Mouse.current.scroll.ReadValue().sqrMagnitude > 0.01f;
        }

        private static bool HasGamepadInput() {
            foreach (Gamepad gamepad in Gamepad.all) {
                foreach (InputControl control in gamepad.allControls) {
                    if (control is ButtonControl button && button.wasPressedThisFrame)
                        return true;
                }

                if (gamepad.leftStick.ReadValue().sqrMagnitude > 0.01f
                    || gamepad.rightStick.ReadValue().sqrMagnitude > 0.01f)
                    return true;
            }

            return false;
        }

        private void ShowState(MenuState state) {
            menuState = state;
            bool menuVisible = state != MenuState.Playing;

            if (overlay != null)
                overlay.SetActive(menuVisible);
            if (mainPanel != null)
                mainPanel.SetActive(state == MenuState.Main);
            if (pausePanel != null)
                pausePanel.SetActive(state == MenuState.Paused);
            if (optionsPanel != null)
                optionsPanel.SetActive(state == MenuState.Options);
            if (creditsPanel != null)
                creditsPanel.SetActive(state == MenuState.Credits);
        }

        public void SetMuted(bool muted) {
            _muted = muted;
            PlayerPrefs.SetInt(AudioMutedKey, muted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyAudioSettings();
        }

        public void SetVolume(float volume) {
            _volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(AudioVolumeKey, _volume);
            ApplyAudioSettings();
            UpdateVolumeLabel();
        }

        private void ApplyAudioSettings() {
            AudioListener.pause = false;
            AudioListener.volume = _muted ? 0f : _volume;
            ApplyFmodAudioSettings();
        }

        private void ApplyFmodAudioSettings() {
            try {
                FMODUnity.RuntimeManager.MuteAllEvents(_muted);
                FMODUnity.RuntimeManager.GetBus(MasterBusPath).setVolume(_volume);
                _fmodWarningLogged = false;
            }
            catch (System.Exception exception) {
                if (_fmodWarningLogged)
                    return;

                _fmodWarningLogged = true;
                Debug.LogWarning($"Could not apply FMOD audio settings: {exception.Message}", this);
            }
        }

        private void UpdateVolumeLabel() {
            if (volumeLabel != null)
                volumeLabel.text = $"Volume: {Mathf.RoundToInt(_volume * 100f)}%";
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
