using System;
using Systems;
using UnityEngine;

namespace Elements {

    public class RespawnPoint : MonoBehaviour {
        
        public SpriteRenderer SpriteRenderer {
            get {
                if (_spriteRenderer == null)
                    _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                return _spriteRenderer;
            }
        }
        [NonSerialized] private SpriteRenderer _spriteRenderer;

        public bool Selected => RespawnSystem.Instance.SelectedPoint == this;

        public PlayerRoles allowRespawning;
        public Sprite spriteDefault;
        public Sprite spriteActive;
        public Sprite spriteSelected;

        private void Start() {
            RespawnSystem.Instance.RespawnPoints.Add(this);
        }

        private void Update() {
            // Only enable this point if there's a character waiting to respawn,
            bool viableCandidate = RespawnSystem.Instance.RespawnTarget && 
            // this point allows respawning that character,
                                   CombatLoopManager.EvaluateRole(RespawnSystem.Instance.RespawnTarget, allowRespawning) && 
            // and this point is in the current room
                                   RoomManager.LocalPositionToIndex(transform.localPosition) == RoomManager.Instance.currentRoom;
            SpriteRenderer.sprite = viableCandidate ? Selected ? spriteSelected : spriteActive : spriteDefault;
        }

        private void OnDestroy() {
            RespawnSystem.Instance.RespawnPoints.Remove(this);
        }

        private void OnValidate() {
            SpriteRenderer.sprite = spriteDefault;
        }

    }

}