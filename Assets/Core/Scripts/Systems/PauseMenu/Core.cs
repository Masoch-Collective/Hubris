using Character;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utils;

namespace Systems.PauseMenu {

    public class Core : Singleton<Core> {
        
        [field:SerializeField] public bool Paused { get; private set; }
        public TextMeshProUGUI selectedField;

        private void Start() {
            InputSystem.actions["Pause"].performed += Toggle;
            SetState(false);
        }

        private void OnDestroy() {
            InputSystem.actions["Pause"].performed -= Toggle;
        }

        private void Update() {
            selectedField.text = Paused ? EventSystem.current.currentSelectedGameObject.name.Split('.')[1] : "";
        }

        private void Toggle(InputAction.CallbackContext obj) {
            // Ignore pause inputs from gamepads controlling players that aren't ready to avoid pausing when readying with start button
            if (obj.control.device is Gamepad gamepad && !CharacterCore.Gamepads[gamepad].Ready)
                return;
            Toggle();
        }

        public void Toggle() {
            SetState(!Paused);
        }

        public void SetState(bool setTo) {
            Paused = setTo;
            Time.timeScale = Paused ? 0 : 1;
            transform.GetChild(0).gameObject.SetActive(Paused);
        }

        public void RestartGame() {
            SceneTransitioner.LoadScene(SceneTransitioner.GameScene);
        }

        public void MainMenu() {
            SetState(false);
            SceneTransitioner.LoadScene(SceneTransitioner.MainMenuScene);
        }

    }

}