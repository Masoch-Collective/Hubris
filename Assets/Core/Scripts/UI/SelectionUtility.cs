using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {

    public class SelectionUtility : MonoBehaviour {

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

        public void Select() => EventSystem.current.SetSelectedGameObject(target);
        // ReSharper disable once ParameterHidesMember
        public void Select(GameObject target) => EventSystem.current.SetSelectedGameObject(target);

    }

}