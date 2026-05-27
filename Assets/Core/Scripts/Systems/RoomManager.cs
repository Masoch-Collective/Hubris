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
        public BoxCollider2D room12Bounds;
        public BoxCollider2D room23Bounds;
        public BoxCollider2D room34Bounds;
        public BoxCollider2D room45Bounds;

        public GameObject player1;
        public GameObject player2;

        public TextMeshProUGUI leaderText;
        public Color player1Color;
        public Color player2Color;

        public int currentRoom = 3;
        public float cooldownLength = 3;
        public float cooldownTimer;
        public bool hasSwitched = false;

        private Vector3 roomPosition;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            roomPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }

        // Update is called once per frame
        void Update()
        {
            if (CombatLoopManager.Instance.Leader == player1.GetComponent<CharacterCore>())
            {
                leaderText.color = player1Color;
                print("colour swap");
            }
            if (CombatLoopManager.Instance.Leader == player2.GetComponent<CharacterCore>())
            {
                leaderText.color = player2Color;
            }

            if (hasSwitched == true) cooldownTimer = cooldownTimer + 1 * Time.deltaTime;
            if (cooldownTimer > cooldownLength)
            {
                hasSwitched = false;
                cooldownTimer = 0;
            }

            if (room23Bounds.IsTouching(CombatLoopManager.Instance.Leader.Hurtbox))
            {
                CombatLoopManager.Instance.Leader.transform.SetParent(this.transform);
                CombatLoopManager.Instance.Seeker.transform.SetParent(this.transform);

                if (currentRoom == 2 && hasSwitched == false)
                {
                    currentRoom = 3;
                    roomPosition.y = roomPosition.y + 12;
                    this.transform.position = roomPosition;
                    hasSwitched = true;
                }


                if (currentRoom == 3 && hasSwitched == false)
                {
                    currentRoom = 2;
                    roomPosition.y = roomPosition.y - 12;
                    this.transform.position = roomPosition;
                    hasSwitched = true;
                }
            }
            if (room34Bounds.IsTouching(CombatLoopManager.Instance.Leader.Hurtbox))
            {
                CombatLoopManager.Instance.Leader.transform.SetParent(this.transform);
                CombatLoopManager.Instance.Seeker.transform.SetParent(this.transform);

                if (currentRoom == 3 && hasSwitched == false)
                {
                    currentRoom = 4;
                    roomPosition.y = roomPosition.y + 12;
                    this.transform.position = roomPosition;
                    hasSwitched = true;
                }

                if (currentRoom == 4 && hasSwitched == false)
                {
                    currentRoom = 3;
                    roomPosition.y = roomPosition.y - 12;
                    this.transform.position = roomPosition;
                    hasSwitched = true;
                }
            }
        }

        private void LateUpdate()
        {
            CombatLoopManager.Instance.Leader.transform.SetParent(null);
            CombatLoopManager.Instance.Seeker.transform.SetParent(null);
        }
    }
}
