using System.Collections.Generic;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.Editor {

    public static class MainMenuSceneBuilder {
        private const string MainMenuScenePath = "Assets/Core/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/Core/Scenes/BuildScene.unity";
        private const string GameMenuPrefabPath = "Assets/Core/Resources/Prefabs/GameMenu.prefab";
        private const string UiActionsPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("Tools/Hubris/Rebuild Main Menu Scene")]
        public static void RebuildMainMenuScene() {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            CreateGlobalCanvasWithMenu();

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            SetBuildScenes();
            AssetDatabase.SaveAssets();
        }

        private static void CreateCamera() {
            GameObject cameraObject = new GameObject("Main Menu Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.07f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.tag = "MainCamera";
        }

        private static void CreateEventSystem() {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(UiActionsPath);
            if (actions == null) {
                Debug.LogWarning($"Could not load UI actions at {UiActionsPath}. Main menu UI may not receive input.");
                return;
            }

            inputModule.actionsAsset = actions;
            inputModule.move = InputActionReference.Create(actions.FindAction("UI/Navigate"));
            inputModule.submit = InputActionReference.Create(actions.FindAction("UI/Submit"));
            inputModule.cancel = InputActionReference.Create(actions.FindAction("UI/Cancel"));
            inputModule.point = InputActionReference.Create(actions.FindAction("UI/Point"));
            inputModule.leftClick = InputActionReference.Create(actions.FindAction("UI/Click"));
            inputModule.rightClick = InputActionReference.Create(actions.FindAction("UI/RightClick"));
            inputModule.middleClick = InputActionReference.Create(actions.FindAction("UI/MiddleClick"));
            inputModule.scrollWheel = InputActionReference.Create(actions.FindAction("UI/ScrollWheel"));
            inputModule.trackedDevicePosition = InputActionReference.Create(actions.FindAction("UI/TrackedDevicePosition"));
            inputModule.trackedDeviceOrientation = InputActionReference.Create(actions.FindAction("UI/TrackedDeviceOrientation"));
        }

        private static void CreateGlobalCanvasWithMenu() {
            GameObject canvasObject = new GameObject(
                nameof(GlobalCanvas),
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(GlobalCanvas)
            );

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            GameMenuController prefab = AssetDatabase.LoadAssetAtPath<GameMenuController>(GameMenuPrefabPath);
            if (prefab == null) {
                Debug.LogError($"Could not load main menu prefab at {GameMenuPrefabPath}.");
                return;
            }

            GameObject menuObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
            menuObject.name = nameof(GameMenuController);
            menuObject.transform.SetParent(canvasObject.transform, false);
        }

        private static void SetBuildScenes() {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene> {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
                if (scene.path == MainMenuScenePath || scene.path == GameplayScenePath)
                    continue;

                scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
