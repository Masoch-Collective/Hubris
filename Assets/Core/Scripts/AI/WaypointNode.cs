using System;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    public List<WaypointLink> links;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.12f);
    }
}
[System.Serializable]
public class WaypointLink
{
    public WaypointNode target;
    public WaypointLinkType type;
}

public enum WaypointLinkType
{
    Walk, 
    Jump,
    Drop
}