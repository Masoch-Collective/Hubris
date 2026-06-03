using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class WaypointNode : MonoBehaviour
    {
        public List<WaypointLink> links = new();

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.12f);

            if (links == null)
                return;

            foreach (WaypointLink link in links)
            {
                if (link.target == null)
                    continue;

                Gizmos.color = link.type switch
                {
                    WaypointLinkType.Walk => Color.yellow,
                    WaypointLinkType.Jump => Color.magenta,
                    WaypointLinkType.Drop => Color.green,
                    _ => Color.white
                };
                Gizmos.DrawLine(transform.position, link.target.transform.position);
            }
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

}
