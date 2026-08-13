using Character;
using UnityEngine;

namespace Systems {

    public class CameraSway : MonoBehaviour {

        public PlayerRoles follow;
        public float intensity;
        public float speed;

        private CharacterCore[] _characters;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _characters = FindObjectsByType<CharacterCore>(FindObjectsSortMode.None);
        }

        // Update is called once per frame
        void Update() {
            Vector2 avg = Vector3.zero;
            int avgCount = 0;
            foreach (var character in _characters) {
                if (CombatLoopManager.EvaluateRole(character, follow)) {
                    avgCount++;
                    avg += (Vector2)(character.transform.position - transform.parent.position);
                }
            }
            if (avgCount == 0)
                avgCount = 1; // Default to 1 to avoid divide-by-zero errors
            float z = transform.localPosition.z;
            transform.localPosition = Vector2.Lerp(transform.localPosition, avg / avgCount * intensity, Time.deltaTime * speed);
            transform.Translate(Vector3.forward * z);
        }

    }

}