using System;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class RoomManager : MonoBehaviour
{
    public GameObject room2;
    public GameObject room3;
    public GameObject room4;

    public BoxCollider2D room12Bounds;
    public BoxCollider2D room23Bounds;
    public BoxCollider2D room34Bounds;
    public BoxCollider2D room45Bounds;

    public int currentRoom = 3;
    public bool hasSwitched = false;

    //temporary, this should get the leading player's collider
    public Collider2D leader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (room23Bounds.IsTouching(leader))
        {
            if (currentRoom == 2 && hasSwitched == false)
            {
                currentRoom = 3;
                this.transform.SetParent(room3.transform, true);
                hasSwitched = true;
            }


            if (currentRoom == 3 && hasSwitched == false)
            {
                currentRoom = 2;
                this.transform.SetParent(room2.transform, true);
                hasSwitched = true;
            }
        }
        if (room34Bounds.IsTouching(leader))
        {
            if (currentRoom == 3 && hasSwitched == false)
            {
                currentRoom = 4;
                this.transform.SetParent(room4.transform, true);
                hasSwitched = true;
            }

            if (currentRoom == 4 && hasSwitched == false)
            {
                currentRoom = 3;
                this.transform.SetParent(room3.transform, true);
                hasSwitched = true;
            }
        }
    }
}
