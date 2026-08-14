using System.Collections.Generic;
using UnityEngine;

namespace Utils {

    public class Blinker : MonoBehaviour {

        public List<MonoBehaviour> targets;
        public float frequency;
        private float _lastBlinkTime;

        // Update is called once per frame
        void Update() {
            if (Time.time > _lastBlinkTime + frequency) {
                _lastBlinkTime = Time.time;
                foreach (var target in targets) 
                    target.enabled = !target.enabled;
            }
        }

    }

}
