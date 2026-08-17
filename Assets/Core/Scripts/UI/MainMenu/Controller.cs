using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.MainMenu {

    [ExecuteInEditMode]
    public class Controller : MonoBehaviour {

        public TextMeshProUGUI selectionText;
        public TextMeshProUGUI versionText;
        public Transform flipLogo;
        public float flipSpeed;

        private GameObject _lastSelection;
        private float _logoRotation;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            versionText.text = Application.version;
        }

        // Update is called once per frame
        void Update() {

            if (!Application.isPlaying) {
                versionText.text = Application.version;
                return;
            }

            if (EventSystem.current.currentSelectedGameObject &&
                EventSystem.current.currentSelectedGameObject.name.Split('.').Length >= 2)
                selectionText.text = EventSystem.current.currentSelectedGameObject.name.Split('.')[1];
            
            if (_lastSelection != EventSystem.current.currentSelectedGameObject)
                _logoRotation += 181;

            flipLogo.rotation = Quaternion.Lerp(flipLogo.rotation, Quaternion.Euler(0, 0, _logoRotation), Time.deltaTime * flipSpeed);
            _lastSelection = EventSystem.current.currentSelectedGameObject;
        }

    }

}