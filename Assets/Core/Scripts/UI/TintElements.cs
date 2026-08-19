using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {

    [ExecuteInEditMode]
    public class TintElements : MonoBehaviour {

        public Color color;
        public List<TextMeshProUGUI> tintText;
        public List<Image> tintImage;

        private void LateUpdate() {
            foreach (var text in tintText)
                text.color = color;
            foreach (var image in tintImage)
                image.color = color;
        }

    }

}