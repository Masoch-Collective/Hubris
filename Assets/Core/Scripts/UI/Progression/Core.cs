using System;
using UnityEngine;
using Utils;

namespace UI.Progression {

    [ExecuteInEditMode]
    public class Core : Singleton<Core> {

        public Transform currentRoomIndicator;
        public PixelPerfectFloat min = new(167);
        public PixelPerfectFloat max = new(217);
        public PixelPerfectFloat indicatorOffset;
        public int levels = 5;
        public int current;
        public int middleIndex = 3;
        public float lerpSpeed = 10;

        private void Update() {
            // I'm getting lost in the sauce I can no longer tell if this kinda logic is a masterpiece or cursed;
            int effectiveIndex = current + middleIndex;
            float setToHeight = effectiveIndex == 0 ? 0 : (max - min) * (effectiveIndex / (float)levels) + min + indicatorOffset;
            Vector3 localPos = currentRoomIndicator.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, setToHeight, Application.isPlaying ? Time.deltaTime * lerpSpeed : 1);
            currentRoomIndicator.localPosition = localPos;
        }

    }

}
