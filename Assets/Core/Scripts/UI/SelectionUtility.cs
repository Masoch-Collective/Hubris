using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {

    public class SelectionUtility : MonoBehaviour {

        private EventSystem _eventSystem;

        public EventSystem EventSystem {
            get {
                if (!_eventSystem)
                    _eventSystem = FindAnyObjectByType<EventSystem>();

                return _eventSystem;
            }
        }

        public bool selectOnStart;
        public bool selectOnEnable;
        public GameObject target;

        private void Start() {
            if (selectOnStart)
                Select();
        }
        
        private void OnEnable() {
            if (selectOnEnable)
                Select();
        }

        public void Select() => EventSystem.SetSelectedGameObject(target);
        // ReSharper disable once ParameterHidesMember
        public void Select(GameObject target) => EventSystem.SetSelectedGameObject(target);

    }

}