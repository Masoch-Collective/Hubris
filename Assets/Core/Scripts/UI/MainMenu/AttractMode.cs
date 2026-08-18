using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.MainMenu {

    [RequireComponent(typeof(ButtonFunctions))]
    public class AttractMode : MonoBehaviour {

        public bool startAwake;
        public float timeout;
        public List<GameObject> visibleWhenAwake;
        public List<GameObject> visibleWhenSleeping;
        public List<CanvasGroup> attractModeBlacklist;

        private float _lastInputTime;
        private bool _sleeping;

        private void Start() {
            if (startAwake)
                Wake();
            else
                Sleep();
        }

        // Update is called once per frame
        void Update() {
            
            if (!_sleeping && Time.time > _lastInputTime + timeout && !attractModeBlacklist.Contains(ButtonFunctions.Instance.CurrentPanel))
                Sleep();

            // Ok this abomination of a LINQ statement was recommended by Rider
            // but the gist of it is that it goes through every control in every
            // input device and if it finds one that is pressed, it evaluates to true
            foreach (InputControl input in from gamepad in InputSystem.devices from input in gamepad.allControls where input.IsPressed() select input) {
                // Only register input from keyboard or gamepad, since mouse axis
                // reads as "pressed" every frame no matter what
                if (input.device is Keyboard or Gamepad) {
                    Activity(input);
                    Debug.Log(input);
                }
            }

        }

        void Activity(InputControl who) {
            if (_sleeping)
                if (who != null)
                    Wake(who);
                else
                    Wake();
            _lastInputTime = Time.time;
        }

        void Sleep() {
            _sleeping = true;
            Debug.Log("Main Menu Sleeping!");
            ButtonFunctions.Instance.ShowPanel(null);
            foreach (var go in visibleWhenAwake)
                go.SetActive(false);
            foreach (var go in visibleWhenSleeping)
                go.SetActive(true);
        }

        void Wake(InputControl who) {
            Debug.Log($"Main Menu Woken by {who.device.name} ({who.path})!");
            Wake();
        }

        void Wake() {
            _sleeping = false;
            ButtonFunctions.Instance.ShowPanel(ButtonFunctions.Instance.defaultPanel);
            foreach (var go in visibleWhenAwake)
                go.SetActive(true);
            foreach (var go in visibleWhenSleeping)
                go.SetActive(false);
        }

    }

}