using System;
using System.Collections.Generic;
using Character;
using Elements;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Systems {

    public class RespawnSystem : Singleton<RespawnSystem> {

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
        public InputAction RespawnAction { get; private set; }

        [field: NonSerialized] public CharacterCore RespawnTarget { get; private set; }

        [NonSerialized] private float _startTime;
        public RespawnModes mode;
        public float minRespawnTime;
        public float respawnTimeout;

        public RespawnPoint SelectedPoint {
            get {
                // Deselect the current point if it's not in the current room
                if (_selectedPoint && RoomManager.LocalPositionToIndex(_selectedPoint.transform.localPosition) !=
                    RoomManager.Instance.currentRoom)
                    _selectedPoint = null;
                // If no point is selected...
                if (!_selectedPoint)
                    // Loop over every RespawnPoint
                    foreach (var respawnPoint in RespawnPoints)
                        // As soon as we find a RespawnPoint in this room that can respawn the target...
                        if (RoomManager.LocalPositionToIndex(respawnPoint.transform.localPosition) ==
                            RoomManager.Instance.currentRoom && CombatLoopManager.EvaluateRole(RespawnTarget, respawnPoint.allowRespawning)) {
                            // Select that respawn point and stop looking
                            _selectedPoint = respawnPoint;
                            break;
                        }
                return _selectedPoint;
            }
        }
        [NonSerialized] private RespawnPoint _selectedPoint;
        [field:SerializeField] public List<RespawnPoint> RespawnPoints { get; private set; }

        // Update is called once per frame
        void Update() {
            if (RespawnTarget && (mode == RespawnModes.Timed || mode == RespawnModes.TimedWithInterruption && Time.time - _startTime > respawnTimeout))
                Spawn();
        } 

        public void Spawn(InputAction.CallbackContext context = default) {
            // Player must wait the minimum respawn time before spawn command can be executed
            if (Time.time - _startTime < minRespawnTime) {
                Debug.LogWarning("Impatient! Ignored Spawn method call because it was called before the minimum wait time had elapsed.");
                return;
            }
            if (!SelectedPoint) {
                Debug.Log($"Cannot respawn; no RespawnPoints available in room {RoomManager.Instance.currentRoom}.");
                return;
            }
            // Respawning logic occurs here!
            // For the current rudimentary death implementation, respawning is a simple as re-enabling the GameObject. But this will likely change in the future.
            RespawnTarget.gameObject.SetActive(true);
            RespawnTarget.transform.position = SelectedPoint.transform.position;
            RespawnTarget.ActionHorizontal.performed -= SwitchSpawnPoint;
            RespawnTarget.Spawned();
            RespawnTarget = null;
            _selectedPoint = null;
            RespawnAction.performed -= Spawn;
        }

        public void OnDestroy() {
            // Cleanup! Make sure to remove Spawn from InputAction.performed event before destroying this object.
            if (RespawnAction != null)
                RespawnAction.performed -= Spawn;
            if (RespawnTarget)
                RespawnTarget.ActionHorizontal.performed -= SwitchSpawnPoint;
        }
        
        public void Enqueue(CharacterCore target, InputAction action) {
            // Ignore enqueue if queuing the same target that is already queued
            if (RespawnTarget && RespawnTarget == target)
                return;
            if (RespawnTarget)
                throw new Exception($"Attempted to enqueue new CharacterCore for respawn, but {RespawnTarget.name} was already queued up. Only one character should be dead at a time!");
            _startTime = Time.time;
            RespawnTarget = target;
            RespawnAction = action;
            RespawnAction.performed += Spawn;
            RespawnTarget.ActionHorizontal.performed += SwitchSpawnPoint;
        }

        private void SwitchSpawnPoint(InputAction.CallbackContext context) {
            if (RespawnTarget.DigitalAxisHorizontal == 1)
                _selectedPoint = SelectedPoint.transform.right.x switch {
                    > 0 when SelectedPoint.selectRight => SelectedPoint.selectRight,
                    < 0 when SelectedPoint.selectLeft => SelectedPoint.selectLeft,
                    _ => _selectedPoint
                };

            if (RespawnTarget.DigitalAxisHorizontal == -1)
                _selectedPoint = SelectedPoint.transform.right.x switch {
                    > 0 when SelectedPoint.selectLeft => SelectedPoint.selectLeft,
                    < 0 when SelectedPoint.selectRight => SelectedPoint.selectRight,
                    _ => _selectedPoint
                };
        }

        private void OnValidate() {
            if (!(minRespawnTime > respawnTimeout)) return;
            Debug.LogWarning("Min respawn time was greater than the respawn timeout, will use respawn timeout value as min respawn time. Please rectify this in the Respawner singleton prefab.", Prefab);
            minRespawnTime = respawnTimeout;
        }

    }

}