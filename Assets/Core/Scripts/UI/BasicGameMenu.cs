using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;
using UnityImage = UnityEngine.UI.Image;

namespace UI {

    public sealed class BasicGameMenu : MonoBehaviour {
        private const float PanelWidth = 520f;
        private const float ButtonHeight = 64f;
        private const float PanelPadding = 36f;
        private const float BorderSize = 2f;

        private enum MenuState {
            Main,
            Playing,
            Paused,
            Options,
            Credits
        }

        private static BasicGameMenu _instance;

        private readonly Color _overlayColor = new Color(0f, 0f, 0f, 0.55f);
        private readonly Color _panelColor = new Color32(24, 28, 36, 235);
        private readonly Color _borderColor = new Color32(88, 96, 112, 255);
        private readonly Color _primaryTextColor = new Color32(245, 247, 250, 255);
        private readonly Color _secondaryTextColor = new Color32(210, 216, 224, 255);
        private readonly Color _buttonColor = new Color32(58, 66, 82, 255);
        private readonly Color _buttonHoverColor = new Color32(82, 94, 116, 255);
        private readonly Color _buttonPressedColor = new Color32(112, 128, 154, 255);

        private MenuState _state = MenuState.Main;
        private MenuState _returnState = MenuState.Main;
        private bool _muted;

        private GameObject _overlay;
        private GameObject _mainPanel;
        private GameObject _pausePanel;
        private GameObject _optionsPanel;
        private GameObject _creditsPanel;
        private Slider _volumeSlider;
        private Toggle _muteToggle;
        private TextMeshProUGUI _volumeLabel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap() {
            if (_instance != null)
                return;

            GameObject menuObject = new GameObject(nameof(BasicGameMenu));
            DontDestroyOnLoad(menuObject);
            _instance = menuObject.AddComponent<BasicGameMenu>();
        }

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            BuildMenu();
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

        private void BuildMenu() {
            EnsureEventSystem();

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            _overlay = CreateOverlay(transform);
            _mainPanel = CreatePanel("MainMenu", "HUBRIS", 460f, _overlay.transform);
            _pausePanel = CreatePanel("PauseMenu", "PAUSED", 390f, _overlay.transform);
            _optionsPanel = CreatePanel("OptionsMenu", "OPTIONS", 380f, _overlay.transform);
            _creditsPanel = CreatePanel("CreditsMenu", "CREDITS", 340f, _overlay.transform);

            BuildMainMenu();
            BuildPauseMenu();
            BuildOptionsMenu();
            BuildCreditsMenu();
        }

        private void BuildMainMenu() {
            Transform content = _mainPanel.transform.Find("Content");
            CreateButton("Start", content, StartGame);
            CreateButton("Single Player", content, SinglePlayer);
            CreateButton("Options", content, () => OpenSubmenu(MenuState.Options));
            CreateButton("Credits", content, () => OpenSubmenu(MenuState.Credits));
            CreateButton("Exit", content, ExitGame);
        }

        private void BuildPauseMenu() {
            Transform content = _pausePanel.transform.Find("Content");
            CreateButton("Resume", content, ResumeGame);
            CreateButton("Options", content, () => OpenSubmenu(MenuState.Options));
            CreateButton("End Game", content, EndGame);
        }

        private void BuildOptionsMenu() {
            Transform content = _optionsPanel.transform.Find("Content");
            _muteToggle = CreateToggle("Mute Audio", content);
            _muteToggle.onValueChanged.AddListener(SetMuted);

            _volumeLabel = CreateLabel("Volume: 100%", content, 26);
            _volumeSlider = CreateSlider(content);
            _volumeSlider.value = AudioListener.volume;
            _volumeSlider.onValueChanged.AddListener(SetVolume);

            CreateSpacer(content, 1f);
            CreateButton("Back", content, () => ShowState(_returnState));
        }

        private void BuildCreditsMenu() {
            Transform content = _creditsPanel.transform.Find("Content");
            CreateLabel("Hubris", content, 30);
            CreateLabel("A student game project", content, 24);
            CreateSpacer(content, 1f);
            CreateButton("Back", content, () => ShowState(_returnState));
        }

        private GameObject CreateOverlay(Transform parent) {
            GameObject overlay = new GameObject("MenuOverlay", typeof(RectTransform), typeof(UnityImage));
            overlay.transform.SetParent(parent, false);

            RectTransform rectTransform = overlay.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            overlay.GetComponent<UnityImage>().color = _overlayColor;
            return overlay;
        }

        private GameObject CreatePanel(string name, string title, float height, Transform parent) {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(UnityImage));
            panel.transform.SetParent(parent, false);

            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(PanelWidth, height);
            rectTransform.anchoredPosition = Vector2.zero;

            panel.GetComponent<UnityImage>().color = _panelColor;
            CreateBorder(panel.transform);

            RectTransform titleRect = CreateRect("Title", panel.transform);
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 52f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            TextMeshProUGUI titleText = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = _primaryTextColor;
            titleText.fontSize = 32f;
            titleText.fontStyle = FontStyles.Bold;

            RectTransform content = CreateRect("Content", panel.transform);
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = new Vector2(PanelPadding, PanelPadding);
            content.offsetMax = new Vector2(-PanelPadding, -76f);

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return panel;
        }

        private void CreateBorder(Transform parent) {
            CreateBorderSegment("TopBorder", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, BorderSize));
            CreateBorderSegment("BottomBorder", parent, Vector2.zero, new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, BorderSize));
            CreateBorderSegment("LeftBorder", parent, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(BorderSize, 0f));
            CreateBorderSegment("RightBorder", parent, new Vector2(1f, 0f), Vector2.one, new Vector2(1f, 0.5f), new Vector2(BorderSize, 0f));
        }

        private void CreateBorderSegment(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta) {
            RectTransform border = CreateRect(name, parent);
            border.anchorMin = anchorMin;
            border.anchorMax = anchorMax;
            border.pivot = pivot;
            border.sizeDelta = sizeDelta;
            border.anchoredPosition = Vector2.zero;
            border.gameObject.AddComponent<UnityImage>().color = _borderColor;
        }

        private UnityButton CreateButton(string text, Transform parent, UnityEngine.Events.UnityAction onClick) {
            GameObject buttonObject = new GameObject($"{text}Button", typeof(RectTransform), typeof(UnityImage), typeof(UnityButton));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0f, ButtonHeight);

            UnityImage image = buttonObject.GetComponent<UnityImage>();
            image.color = _buttonColor;

            UnityButton button = buttonObject.GetComponent<UnityButton>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock {
                normalColor = _buttonColor,
                highlightedColor = _buttonHoverColor,
                pressedColor = _buttonPressedColor,
                selectedColor = _buttonHoverColor,
                disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.8f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };
            button.onClick.AddListener(onClick);

            TextMeshProUGUI label = CreateLabel(text, buttonObject.transform, 28);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.color = _primaryTextColor;

            return button;
        }

        private Toggle CreateToggle(string text, Transform parent) {
            GameObject toggleObject = new GameObject("MuteToggle", typeof(RectTransform), typeof(Toggle));
            toggleObject.transform.SetParent(parent, false);
            toggleObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 44f);

            RectTransform checkBox = CreateRect("CheckBox", toggleObject.transform);
            checkBox.anchorMin = new Vector2(0f, 0.5f);
            checkBox.anchorMax = new Vector2(0f, 0.5f);
            checkBox.pivot = new Vector2(0f, 0.5f);
            checkBox.sizeDelta = new Vector2(28f, 28f);
            checkBox.anchoredPosition = Vector2.zero;
            UnityImage checkBackground = checkBox.gameObject.AddComponent<UnityImage>();
            checkBackground.color = _buttonColor;

            RectTransform checkMark = CreateRect("Checkmark", checkBox);
            checkMark.anchorMin = new Vector2(0.2f, 0.2f);
            checkMark.anchorMax = new Vector2(0.8f, 0.8f);
            checkMark.offsetMin = Vector2.zero;
            checkMark.offsetMax = Vector2.zero;
            UnityImage checkMarkImage = checkMark.gameObject.AddComponent<UnityImage>();
            checkMarkImage.color = _primaryTextColor;

            TextMeshProUGUI label = CreateLabel(text, toggleObject.transform, 24);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(44f, 0f);
            labelRect.offsetMax = Vector2.zero;
            label.alignment = TextAlignmentOptions.Left;
            label.color = _secondaryTextColor;

            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = checkBackground;
            toggle.graphic = checkMarkImage;
            toggle.isOn = _muted;
            return toggle;
        }

        private Slider CreateSlider(Transform parent) {
            GameObject sliderObject = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            sliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 32f);

            RectTransform background = CreateRect("Background", sliderObject.transform);
            background.anchorMin = new Vector2(0f, 0.5f);
            background.anchorMax = new Vector2(1f, 0.5f);
            background.sizeDelta = new Vector2(0f, 8f);
            background.gameObject.AddComponent<UnityImage>().color = _buttonColor;

            RectTransform fillArea = CreateRect("Fill Area", sliderObject.transform);
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.sizeDelta = new Vector2(-20f, 8f);
            fillArea.anchoredPosition = Vector2.zero;

            RectTransform fill = CreateRect("Fill", fillArea);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.sizeDelta = Vector2.zero;
            fill.gameObject.AddComponent<UnityImage>().color = _buttonPressedColor;

            RectTransform handleArea = CreateRect("Handle Slide Area", sliderObject.transform);
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(10f, 0f);
            handleArea.offsetMax = new Vector2(-10f, 0f);

            RectTransform handle = CreateRect("Handle", handleArea);
            handle.sizeDelta = new Vector2(22f, 28f);
            handle.gameObject.AddComponent<UnityImage>().color = _primaryTextColor;

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handle.GetComponent<UnityImage>();
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private TextMeshProUGUI CreateLabel(string text, Transform parent, int fontSize) {
            RectTransform rectTransform = CreateRect($"{text}Label", parent);
            rectTransform.sizeDelta = new Vector2(0f, 38f);

            TextMeshProUGUI label = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.color = fontSize >= 28 ? _primaryTextColor : _secondaryTextColor;
            label.fontSize = fontSize;
            label.fontStyle = fontSize >= 28 ? FontStyles.Bold : FontStyles.Normal;
            return label;
        }

        private void CreateSpacer(Transform parent, float flexibleHeight) {
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(parent, false);
            spacer.GetComponent<LayoutElement>().flexibleHeight = flexibleHeight;
        }

        private static RectTransform CreateRect(string name, Transform parent) {
            GameObject rectObject = new GameObject(name, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            return rectObject.GetComponent<RectTransform>();
        }

        private void StartGame() {
            _state = MenuState.Playing;
            Time.timeScale = 1f;
            ShowState(MenuState.Playing);
        }

        private void SinglePlayer() {
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
            _overlay.SetActive(menuVisible);
            _mainPanel.SetActive(state == MenuState.Main);
            _pausePanel.SetActive(state == MenuState.Paused);
            _optionsPanel.SetActive(state == MenuState.Options);
            _creditsPanel.SetActive(state == MenuState.Credits);
        }

        private void SetMuted(bool muted) {
            _muted = muted;
            AudioListener.pause = muted;
        }

        private void SetVolume(float volume) {
            AudioListener.volume = volume;
            _volumeLabel.text = $"Volume: {Mathf.RoundToInt(volume * 100f)}%";
        }

        private static void ExitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void EnsureEventSystem() {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null) {
                GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                DontDestroyOnLoad(eventSystemObject);
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }

            InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

            BaseInputModule[] inputModules = eventSystem.GetComponents<BaseInputModule>();
            foreach (BaseInputModule module in inputModules)
                module.enabled = module == inputModule;

            ConfigureInputModule(inputModule);
        }

        private static void ConfigureInputModule(InputSystemUIInputModule inputModule) {
            InputActionAsset actions = InputSystem.actions;
            InputActionMap uiActions = actions?.FindActionMap("UI", false);
            if (uiActions == null)
                return;

            inputModule.actionsAsset = actions;
            inputModule.move = InputActionReference.Create(uiActions.FindAction("Navigate", false));
            inputModule.submit = InputActionReference.Create(uiActions.FindAction("Submit", false));
            inputModule.cancel = InputActionReference.Create(uiActions.FindAction("Cancel", false));
            inputModule.point = InputActionReference.Create(uiActions.FindAction("Point", false));
            inputModule.leftClick = InputActionReference.Create(uiActions.FindAction("Click", false));
            inputModule.rightClick = InputActionReference.Create(uiActions.FindAction("RightClick", false));
            inputModule.middleClick = InputActionReference.Create(uiActions.FindAction("MiddleClick", false));
            inputModule.scrollWheel = InputActionReference.Create(uiActions.FindAction("ScrollWheel", false));
        }
    }
}
