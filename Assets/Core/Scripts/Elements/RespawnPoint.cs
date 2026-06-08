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

        public RespawnPoint selectLeft;
        public RespawnPoint selectRight;

        [field:SerializeField] public PlayerRoles AllowRespawning { get; private set; }
        public Sprite spriteDefault;
        public Sprite spriteActive;
        public Sprite spriteSelected;

        private void Start() {
            RespawnSystem.Instance.RespawnPoints.Add(this);
            // Auto-set which character this RespawnPoint can spawn according to this RespawnPoint's orientation
            AllowRespawning = transform.up.y > 0 ? PlayerRoles.BottomGoal : PlayerRoles.TopGoal;
        }

        private void Update() {
            // Only enable this point if there's a character waiting to respawn,
            bool viableCandidate = RespawnSystem.Instance.RespawnTarget && 
            // this point allows respawning that character,
                                   CombatLoopManager.EvaluateRole(RespawnSystem.Instance.RespawnTarget, AllowRespawning) && 
            // and this point is in the current room
                                   RoomManager.LocalPositionToIndex(transform.localPosition) == RoomManager.Instance.currentRoom;
            SpriteRenderer.sprite = viableCandidate ? Selected ? spriteSelected : spriteActive : spriteDefault;
        }

        private void OnDestroy() {
            // TODO: This is causing a RespawnSystem to be instantiated after the existing RespawnSystem instance is destroyed during scene close cleanup
            RespawnSystem.Instance.RespawnPoints.Remove(this);
        }

        private void OnValidate() {
            SpriteRenderer.sprite = spriteDefault;
            if (selectLeft)
                selectLeft.selectRight = this;
            if (selectRight)
                selectRight.selectLeft = this;
        
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.deepPink;
            if (selectLeft)
                Gizmos.DrawLine(transform.position, selectLeft.transform.position);
            else {
                Gizmos.DrawRay(transform.position, -transform.right);
                Gizmos.DrawLine(transform.position - transform.right + transform.up, 
                    transform.position - transform.right - transform.up);
            }
            Gizmos.color = Color.deepSkyBlue;
            if (selectRight)
                Gizmos.DrawLine(transform.position, selectRight.transform.position);
            else {
                Gizmos.DrawRay(transform.position, transform.right);

                Gizmos.DrawLine(transform.position + transform.right + transform.up,
                    transform.position + transform.right - transform.up);
            }
        }

    }

}