using TMPro;
using UnityEngine;
using Utils;

namespace Systems {
    
    public class RoomManager : Singleton<RoomManager> {

        public TextMeshProUGUI leaderText;
        public Color leaderTextColorIfNull = Color.clear;
        public GameObject cameraSetup;

        public int currentRoom = 0;
        public int roomHeight = 12;
        public int roomOffset = 0;
        public float killPlaneBuffer = -1;
        public float cameraLerpSpeed;

        // Update is called once per frame
        void Update() {

            // Update the leader HUD colour
            // TODO: This has nothing to do with the room system, and should be moved to a more appropriate script
            leaderText.color = CombatLoopManager.Instance.Leader ? CombatLoopManager.Instance.Leader.Color : leaderTextColorIfNull;

            // If there is no leader, the current room is the start
            if (CombatLoopManager.Instance.Leader) {
                // Calculate the room index according to the player's vertical position relative to the map
                int inRoom = Mathf.RoundToInt(CombatLoopManager.Instance.Leader.transform.localPosition.y / roomHeight);
                // Only update the current room if moving in the leader's direction
                currentRoom = CombatLoopManager.Instance.Orientation switch {
                    > 0 => Mathf.Max(currentRoom, inRoom),
                    < 0 => Mathf.Min(currentRoom, inRoom),
                    _ => currentRoom
                };
                // Give leader uppies if they reach the top of the screen to facilitate screen transition
                if (CombatLoopManager.Instance.Leader.transform.position.y > cameraSetup.transform.position.y + roomHeight / 2f)
                    CombatLoopManager.Instance.Leader.Rigidbody.velocity = Vector2.up * CombatLoopManager.Instance.Leader.Controller.JumpForce;
            } else
                currentRoom = 0;

            // Smoothly move the camera to the position indicated by the room index multiplied by the room height plus the room offset
            cameraSetup.transform.localPosition = new Vector3(0, Mathf.Lerp(cameraSetup.transform.localPosition.y, currentRoom * roomHeight + roomOffset, Time.deltaTime * cameraLerpSpeed), cameraSetup.transform.localPosition.z);
            cameraSetup.transform.rotation = Quaternion.identity;
            
            // Kill seeker if they're below the bottom of the screen (plus some buffer)
            if (CombatLoopManager.Instance.Seeker &&
                CombatLoopManager.Instance.Seeker.transform.position.y < cameraSetup.transform.position.y - roomHeight / 2f + killPlaneBuffer && 
                CombatLoopManager.Instance.Seeker.gameObject.activeInHierarchy)
                CombatLoopManager.Instance.Seeker.Die();
            
        }

        public static int LocalPositionToIndex(Vector2 position) => LocalPositionToIndex(position.y);
        public static int LocalPositionToIndex(float position) {
            return Mathf.RoundToInt(position / Instance.roomHeight);
        }
        
    }
    
}
