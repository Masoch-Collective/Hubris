using System;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class RoomManager : MonoBehaviour
{
    public BoxCollider2D room12Bounds;
    public BoxCollider2D room23Bounds;
    public BoxCollider2D room34Bounds;
    public BoxCollider2D room45Bounds;

    public int currentRoom = 3;
    public float cooldownLength = 3;
    public float cooldownTimer;
    public bool hasSwitched = false;

    Vector3 roomPosition;

    //temporary, this should get the leading player's collider
    public Collider2D leader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (hasSwitched == true) cooldownTimer = cooldownTimer + 1 * Time.deltaTime;
        if (cooldownTimer > cooldownLength)
        {
            hasSwitched = false;
            cooldownTimer = 0;
        }

        if (room23Bounds.IsTouching(leader))
        {
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
        if (room34Bounds.IsTouching(leader))
        {
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
}
