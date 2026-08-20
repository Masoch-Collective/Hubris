using System;
using TMPro;
using UnityEngine;

namespace UI {

    public class Button : MonoBehaviour {

        public TextMeshProUGUI labelForeground;
        public TextMeshProUGUI labelBackground;
        public string label;

        private UnityEngine.UI.Button _b;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() { }

        // Update is called once per frame
        void Update() { }

        private void OnValidate() {
            labelForeground.text = label;
            labelBackground.text = label;
        }
        
        public static implicit operator UnityEngine.UI.Button(Button b) {
            if (b._b == null)
                b._b = b.GetComponent<UnityEngine.UI.Button>();
            return b._b;
        }

    }

}
