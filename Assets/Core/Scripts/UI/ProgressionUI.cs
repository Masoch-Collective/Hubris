using System;
using UnityEngine;
using Utils;

namespace UI {

    public class ProgressionUI : Singleton<ProgressionUI> {

        public SpriteRenderer fillSprite;
        public float min = 5.21875f;
        public float max = 6.78125f;
        public int levels = 5;
        public int current;
        public int middleIndex = 3;
        public float lerpSpeed = 10;

        public void Update() {
            float setToHeight = current == 0 ? 0 : (max - min) * (((float)current - middleIndex) / levels) + min;
            Vector2 size = fillSprite.size;
            size.y = Mathf.Lerp(size.y, setToHeight, Time.deltaTime * lerpSpeed);
            fillSprite.size = size;
        }

    }

}
