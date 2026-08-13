using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace UI {

    public sealed class GlobalCanvas : Singleton<GlobalCanvas> {
        private const int SortingOrder = 1000;
        private static readonly Vector2 ReferenceResolution = new Vector2(1280f, 720f);

        private static EventSystem _eventSystem;

        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private GraphicRaycaster _raycaster;

        public new static GlobalCanvas Instance {
            get {
                GlobalCanvas instance = FindFirstObjectByType<GlobalCanvas>();
                if (instance == null)
                    instance = Create();

                instance.Initialize();
                return instance;
            }
        }

        public Transform Root => transform;

        private void Awake() {
            GlobalCanvas[] instances = FindObjectsByType<GlobalCanvas>(FindObjectsSortMode.InstanceID);
            if (instances.Length > 0 && instances[0] != this) {
                Destroy(gameObject);
                return;
            }

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
                _canvas = GetComponent<Canvas>();
            if (_canvas == null)
                _canvas = gameObject.AddComponent<Canvas>();

            if (_scaler == null)
                _scaler = GetComponent<CanvasScaler>();
            if (_scaler == null)
                _scaler = gameObject.AddComponent<CanvasScaler>();

            if (_raycaster == null)
                _raycaster = GetComponent<GraphicRaycaster>();
            if (_raycaster == null)
                _raycaster = gameObject.AddComponent<GraphicRaycaster>();

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
                Debug.LogError("No EventSystem found. Add an EventSystem with an InputSystemUIInputModule configured in the scene.");
                return;
            }

            foreach (EventSystem eventSystem in eventSystems)
                eventSystem.enabled = eventSystem == _eventSystem;

            InputSystemUIInputModule inputModule = _eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null) {
                Debug.LogError("The active EventSystem is missing an InputSystemUIInputModule configured with UI actions.");
                return;
            }

            BaseInputModule[] inputModules = _eventSystem.GetComponents<BaseInputModule>();
            foreach (BaseInputModule module in inputModules)
                module.enabled = module == inputModule;
        }
    }
}
