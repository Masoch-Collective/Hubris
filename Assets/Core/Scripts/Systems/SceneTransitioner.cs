using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Systems {

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Canvas))]
    public class SceneTransitioner : Singleton<SceneTransitioner> {
            
        private static readonly int Cover = Animator.StringToHash("Cover");

        private static HubrisScene FindScene(string sceneName) {
            return Instance.scenes.Find(s => s.SceneName == sceneName);
        }
        public static HubrisScene MainMenuScene {
            get {
                if (mainMenuScene == null)
                    mainMenuScene = FindScene("MainMenu");
                return mainMenuScene;
            }
        }
        private static HubrisScene mainMenuScene;
        public static HubrisScene GameScene {
            get {
                if (gameScene == null)
                    gameScene = FindScene("BuildScene");
                return gameScene;
            }
        }
        private static HubrisScene gameScene;

        [SerializeField]
        public List<HubrisScene> scenes;
	public float canvasOffset = 0.5f;

	public Camera Camera {
	    get {
		if (_camera == null)
		    _camera = Camera.main;
		return _camera;
	    }
	}
	private Camera _camera;

        public Canvas Canvas {
            get {
                if (_canvas == null)
                    _canvas = GetComponent<Canvas>();
                return _canvas;
            }
        }
        private Canvas _canvas;

        public Animator Animator {
            get {
                if (_animator == null)
                    _animator = GetComponent<Animator>();
                return _animator;
            }
        }
        private Animator _animator;

        private HubrisScene _queuedScene;
        private bool _quitting;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            if (Instance != this) {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += SceneLoaded;
        }
        
        public static void LoadScene(HubrisScene scene) {
            Instance._queuedScene = scene;
            Instance.Animator.SetBool(Cover, true);
        }

        public static void Quit() {
            Instance._quitting = true;
            Instance.Animator.SetBool(Cover, true);
        }

        private void CoverComplete() {
            if (_quitting) {
                Application.Quit();
                Debug.Log("Goodbye!");
                return;
            }
            if (_queuedScene != null) {
                SceneManager.LoadScene(_queuedScene.SceneName);
                Debug.Log($"Loading {_queuedScene.name}.");
                return;
            }
            Debug.LogError("Cover animation complete, but no scene nor quit was set!");
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode) {
            Canvas.transform.localScale = Vector3.one / FindScene(scene.name).pixelsPerUnit;
            Canvas.worldCamera = Camera.main;
            Reveal();
        }

        private void Reveal() {
            Animator.SetBool(Cover, false);
        }

	public void LateUpdate() {
	    transform.position = Canvas.worldCamera.transform.position + (Vector3.forward * canvasOffset);
	}

    }

}
