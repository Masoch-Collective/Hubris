using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.MainMenu {

    public class Controller : MonoBehaviour {

        public TextMeshProUGUI selectionText;
        public Transform flipLogo;
        public float flipSpeed;

        private GameObject _lastSelection;
        private float _logoRotation;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() { }

        // Update is called once per frame
        void Update() {

            selectionText.text = EventSystem.current.currentSelectedGameObject.name.Split('.')[1];
            
            if (_lastSelection != EventSystem.current.currentSelectedGameObject)
                _logoRotation += 181;

            flipLogo.rotation = Quaternion.Lerp(flipLogo.rotation, Quaternion.Euler(0, 0, _logoRotation), Time.deltaTime * flipSpeed);
            _lastSelection = EventSystem.current.currentSelectedGameObject;
        }

    }

}