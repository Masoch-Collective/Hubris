using System.Collections;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public sealed class AttractModeController : MonoBehaviour
    {
        private const string DefaultMainMenuScenePath = "Assets/Core/Scenes/MainMenu.unity";

        [SerializeField] private MapVerticalFlipper mapVerticalFlipper;
        [SerializeField, Min(0f)] private float returnDelay = 2f;
        [SerializeField] private string mainMenuScenePath = DefaultMainMenuScenePath;

        private Coroutine _returnRoutine;

        private void OnEnable()
        {
            if (mapVerticalFlipper == null)
                mapVerticalFlipper = FindFirstObjectByType<MapVerticalFlipper>();

            if (mapVerticalFlipper != null)
                mapVerticalFlipper.onFlipStart.AddListener(OnFlipDetected);
        }

        private void OnDisable()
        {
            if (mapVerticalFlipper != null)
                mapVerticalFlipper.onFlipStart.RemoveListener(OnFlipDetected);

            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }
        }

        private void Update()
        {
            if (HasKeyboardInput() || HasMouseInput())
                ReturnToMainMenu();
        }

        private void OnFlipDetected()
        {
            if (_returnRoutine != null)
                return;

            _returnRoutine = StartCoroutine(ReturnToMainMenuAfterDelay());
        }

        private IEnumerator ReturnToMainMenuAfterDelay()
        {
            yield return new WaitForSecondsRealtime(returnDelay);
            _returnRoutine = null;

            ReturnToMainMenu();
        }

        private void ReturnToMainMenu()
        {
            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }

            Time.timeScale = 1f;
            LoadScene(mainMenuScenePath);
        }

        private static bool HasKeyboardInput()
        {
            return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        }

        private static bool HasMouseInput()
        {
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

        private static void LoadScene(string scenePath)
        {
            if (!Application.CanStreamedLevelBeLoaded(scenePath))
            {
                Debug.LogError($"Scene '{scenePath}' is not in Build Settings.");
                return;
            }

            SceneManager.LoadScene(scenePath);
        }
    }
}
