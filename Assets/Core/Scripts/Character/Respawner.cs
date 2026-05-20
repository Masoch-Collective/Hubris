using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character {

    public class Respawner : MonoBehaviour {

        public enum RespawnModes {
            /// <summary>
            /// Character will spawn when the InputAction's performed event is invoked.
            /// </summary>
            OnInputActionPerformed,
            /// <summary>
            /// Character will spawn after the specified time has elapsed.
            /// </summary>
            Timed,
            /// <summary>
            /// Combination of the other two: spawns after time, or before that if InputAction is performed.
            /// </summary>
            TimedWithInterruption
        }

        private static Respawner Prefab {
            get {
                if (_prefab == null)
                    _prefab = Resources.Load<Respawner>("Prefabs/Respawner");
                return _prefab;
            }
        }
        [NonSerialized] private static Respawner _prefab;
        // [field:NonSerialized] 
        public InputAction RespawnAction { get; private set; }
        [field:NonSerialized] public CharacterCore RespawnTarget { get; private set; }
        [NonSerialized] private float _startTime;
        public RespawnModes mode;
        public float minRespawnTime;
        public float respawnTimeout;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            if (minRespawnTime > respawnTimeout) {
                Debug.LogWarning("Min respawn time was greater than the respawn timeout, will use respawn timeout value as min respawn time. Please rectify this in the Respawner prefab.", Prefab);
                minRespawnTime = respawnTimeout;
            }
        }

        // Update is called once per frame
        void Update() {
            if (mode == RespawnModes.Timed || mode == RespawnModes.TimedWithInterruption && Time.time - _startTime > respawnTimeout)
                Spawn();
        }

        public void Spawn(InputAction.CallbackContext context = default) {
            // Player must wait the minimum respawn time before spawn command can be executed
            if (Time.time - _startTime < minRespawnTime) {
                Debug.LogWarning("Impatient! Ignored Spawn method call because it was called before the minimum wait time had elapsed.");
                return;
            }
            // Respawning logic occurs here!
            // For the current rudimentary death implementation, respawning is a simple as re-enabling the GameObject. But this will likely change in the future.
            RespawnTarget.gameObject.SetActive(true);
            RespawnTarget.transform.position = transform.position;
            RespawnTarget.Spawned();
            // TODO: Pooling will likely be implemented into the game at some point (steal the one from mini-capstone project). Implement pooling here once that's available.
            Destroy(gameObject);
        }

        public void OnDestroy() {
            // Cleanup! Make sure to remove Spawn from InputAction.performed event before destroying this object.
            RespawnAction.performed -= Spawn;
        }

        /// <summary>
        /// Instantiates Respawner resource prefab to respawn the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The character to respawn.</param>
        /// <param name="action">The action to listen for input-based respawning (not needed if mode == RespawnModes.Timed).</param>
        public static void Enqueue(CharacterCore target, InputAction action) {
            Respawner respawner = Instantiate(Prefab);
            respawner.name = $"Respawner for {target.name}";
            respawner.RespawnAction = action;
            respawner.RespawnTarget = target;
            respawner._startTime = Time.time;
            respawner.RespawnAction.performed += respawner.Spawn;
        }

    }

}