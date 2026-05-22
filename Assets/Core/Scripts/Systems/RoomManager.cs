using System;
using System.Collections.Generic;
using System.Linq;
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

    public List<GameObject> leaderboard;

    public int currentRoom = 3;
    public float cooldownLength = 3;
    public float cooldownTimer;
    public bool hasSwitched = false;

    Vector3 roomPosition;

    //temporary, this should get the leading player's collider
    public Collider2D leaderCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        SwapLeader();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x != 0) transform.position = new Vector3(0, roomPosition.y, -10);

        if (hasSwitched == true) cooldownTimer = cooldownTimer + 1 * Time.deltaTime;
        if (cooldownTimer > cooldownLength)
        {
            hasSwitched = false;
            cooldownTimer = 0;
        }

        if (room23Bounds.IsTouching(leaderCollider))
        {
            leaderboard.ElementAt(0).transform.SetParent(this.transform);
            leaderboard.ElementAt(1).transform.SetParent(this.transform);

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
        if (room34Bounds.IsTouching(leaderCollider))
        {
            leaderboard.ElementAt(0).transform.SetParent(this.transform);
            leaderboard.ElementAt(1).transform.SetParent(this.transform);

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
    private void SwapLeader()
    {
        leaderboard.Add(leaderboard.ElementAt(0));
        leaderboard.Remove(leaderboard.ElementAt(0));
        leaderCollider = leaderboard.ElementAt(0).GetComponent<Collider2D>();
    }

    private void LateUpdate()
    {
        leaderboard.ElementAt(0).transform.SetParent(null);
        leaderboard.ElementAt(1).transform.SetParent(null);
    }
}
