using Character;
using System;
using System.Collections.Generic;
using System.Linq;
using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Systems
{
    public class RoomManager : MonoBehaviour
    {

        public TextMeshProUGUI leaderText;
        public Color leaderTextColorIfNull = Color.clear;

        public int currentRoom = 0;
        public int roomHeight = 12;
        public int roomOffset = 0;
        public float killPlaneBuffer = -1;
        public float cameraLerpSpeed;
        
        private Camera _camera;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _camera = Camera.main;
        }

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
                if (CombatLoopManager.Instance.Leader.transform.position.y > _camera.transform.position.y + roomHeight / 2f)
                    CombatLoopManager.Instance.Leader.Rigidbody.velocity = Vector2.up * CombatLoopManager.Instance.Leader.Controller.JumpForce;
            } else
                currentRoom = 0;

            // Smoothly move the camera to the position indicated by the room index multiplied by the room height plus the room offset
            _camera.transform.localPosition = new Vector3(0, Mathf.Lerp(currentRoom * roomHeight + roomOffset, _camera.transform.localPosition.y, Time.deltaTime * cameraLerpSpeed), _camera.transform.localPosition.z);
            _camera.transform.rotation = Quaternion.identity;
            
            // Kill players below the bottom of the screen (plus some buffer)
            if (CombatLoopManager.Instance.TopGoal &&
                CombatLoopManager.Instance.TopGoal.transform.position.y < _camera.transform.position.y - roomHeight / 2f + killPlaneBuffer && 
                CombatLoopManager.Instance.TopGoal.gameObject.activeInHierarchy)
                CombatLoopManager.Instance.TopGoal.Die();
            if (CombatLoopManager.Instance.BottomGoal &&
                CombatLoopManager.Instance.BottomGoal.transform.position.y < _camera.transform.position.y - roomHeight / 2f + killPlaneBuffer && 
                CombatLoopManager.Instance.BottomGoal.gameObject.activeInHierarchy)
                CombatLoopManager.Instance.BottomGoal.Die();
            
        }
    }
}
