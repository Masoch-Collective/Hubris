using UnityEngine;

namespace Utils {

    public class ReparentOnStart : MonoBehaviour {

        public Transform parentTo;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            transform.parent = parentTo;
        }

    }

}