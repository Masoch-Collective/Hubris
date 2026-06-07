using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI {

    public sealed class GlobalCanvas : MonoBehaviour {
        private const int SortingOrder = 1000;
        private static readonly Vector2 ReferenceResolution = new Vector2(1280f, 720f);

        private static GlobalCanvas _instance;
        private static EventSystem _eventSystem;

        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private GraphicRaycaster _raycaster;
 
        public static GlobalCanvas Instance {
            get {
                if (_instance == null)
                    _instance = FindFirstObjectByType<GlobalCanvas>();

                if (_instance == null)
                    _instance = Create();

                _instance.Initialize();
                return _instance;
            }
        }

        public Transform Root => transform;

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            EnsureEventSystem();
        }

        private static GlobalCanvas Create() {
            GameObject canvasObject = new GameObject(
                nameof(GlobalCanvas),
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            DontDestroyOnLoad(canvasObject);
            return canvasObject.AddComponent<GlobalCanvas>();
        }

        private void Initialize() {
            if (_canvas == null)
                _canvas = GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
            if (_scaler == null)
                _scaler = GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
            if (_raycaster == null)
                _raycaster = GetComponent<GraphicRaycaster>() ?? gameObject.AddComponent<GraphicRaycaster>();

            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = SortingOrder;

            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = ReferenceResolution;
            _scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();
        }

        private static void EnsureEventSystem() {
            EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

            if (_eventSystem == null) {
                foreach (EventSystem candidate in eventSystems) {
                    if (candidate.GetComponent<InputSystemUIInputModule>() != null) {
                        _eventSystem = candidate;
                        break;
                    }
                }

                if (_eventSystem == null && eventSystems.Length > 0)
                    _eventSystem = eventSystems[0];
            }

            if (_eventSystem == null) {
                GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                DontDestroyOnLoad(eventSystemObject);
                _eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }

            foreach (EventSystem eventSystem in eventSystems)
                eventSystem.enabled = eventSystem == _eventSystem;

            InputSystemUIInputModule inputModule = _eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
                inputModule = _eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

            BaseInputModule[] inputModules = _eventSystem.GetComponents<BaseInputModule>();
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
