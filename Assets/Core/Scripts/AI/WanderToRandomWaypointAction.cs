using System;
using System.Collections.Generic;
using Character;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace AI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wander To Random Waypoint",
        description: "Finds a random WaypointNode in the scene, follows waypoint links to it, then repeats.",
        story: "[Agent] wanders to random waypoints",
        category: "Action/Hubris",
        id: "e0523cdbb09d4fb0b73f8a2a6f693315")]
    public partial class WanderToRandomWaypointAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<float> ArrivalRadius = new BlackboardVariable<float>(0.35f);
        [SerializeReference] public BlackboardVariable<float> HorizontalStopDistance = new BlackboardVariable<float>(0.1f);
        [SerializeReference] public BlackboardVariable<float> JumpHoldDuration = new BlackboardVariable<float>(0.8f);
        [SerializeReference] public BlackboardVariable<float> JumpForceMultiplier = new BlackboardVariable<float>(1.2f);
        [SerializeReference] public BlackboardVariable<float> LinkTimeout = new BlackboardVariable<float>(3f);
        [SerializeReference] public BlackboardVariable<float> StuckCheckInterval = new BlackboardVariable<float>(0.5f);
        [SerializeReference] public BlackboardVariable<float> StuckMinProgress = new BlackboardVariable<float>(0.08f);
        [SerializeReference] public BlackboardVariable<int> MaxStuckChecks = new BlackboardVariable<int>(3);
        [SerializeReference] public BlackboardVariable<float> MaxNearestWaypointVerticalDifference = new BlackboardVariable<float>(0.8f);

        private readonly List<WaypointLink> _path = new List<WaypointLink>();

        private CharacterCore _characterCore;
        private WaypointNode[] _waypoints;

        private WaypointNode _currentWaypoint;
        private WaypointNode _nextWaypoint;
        private WaypointNode _destinationWaypoint;
        private WaypointLink _currentLink;

        private int _pathIndex;

        private float _linkTimer;
        private float _jumpHoldTimer;
        private float _stuckCheckTimer;
        private float _lastDistanceToNext;
        private int _stuckCheckCount;

        private bool _jumpRequested;

        protected override Status OnStart()
        {
            if (Agent == null || Agent.Value == null)
                return Status.Failure;

            _characterCore = Agent.Value.GetComponent<CharacterCore>();

            if (_characterCore == null)
                return Status.Failure;

            _characterCore.UseAIInput = true;
            RefreshWaypoints();

            return StartNewPath() ? Status.Running : Status.Failure;
        }

        protected override Status OnUpdate()
        {
            if (_characterCore == null || Agent == null || Agent.Value == null)
                return Status.Failure;

            if (_nextWaypoint == null)
                return StartNewPath() ? Status.Running : Status.Failure;

            _linkTimer += Time.deltaTime;

            MoveToward(_nextWaypoint, _currentLink);

            if (HasArrivedAt(_nextWaypoint))
            {
                ReachNextWaypoint();
                return Status.Running;
            }

            if (_linkTimer >= LinkTimeout.Value || IsStuck())
                return StartNewPath() ? Status.Running : Status.Failure;

            return Status.Running;
        }

        protected override void OnEnd()
        {
            StopInput();

            if (_characterCore != null)
                _characterCore.UseAIInput = false;

            _characterCore = null;
            _waypoints = null;
            _currentWaypoint = null;
            _nextWaypoint = null;
            _destinationWaypoint = null;
            _currentLink = null;
            _path.Clear();
        }

        private void RefreshWaypoints()
        {
            _waypoints = UnityEngine.Object.FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);
        }

        private bool StartNewPath()
        {
            ResetMovementProgress();

            if (_waypoints == null || _waypoints.Length < 2)
                RefreshWaypoints();

            if (_waypoints == null || _waypoints.Length < 2)
            {
                StopInput();
                return false;
            }

            _currentWaypoint = FindNearestWaypoint();

            if (_currentWaypoint == null)
            {
                StopInput();
                return false;
            }

            _destinationWaypoint = PickReachableRandomWaypoint(_currentWaypoint);

            if (_destinationWaypoint == null)
            {
                StopInput();
                _nextWaypoint = null;
                return false;
            }

            _pathIndex = 0;
            SetNextLink();
            return _nextWaypoint != null;
        }

        private void ReachNextWaypoint()
        {
            _currentWaypoint = _nextWaypoint;
            _pathIndex++;

            if (_pathIndex >= _path.Count)
            {
                StartNewPath();
                return;
            }

            SetNextLink();
        }

        private void SetNextLink()
        {
            if (_pathIndex < 0 || _pathIndex >= _path.Count)
            {
                StartNewPath();
                return;
            }

            _currentLink = _path[_pathIndex];
            _nextWaypoint = _currentLink.target;
            ResetMovementProgress();
        }

        private void MoveToward(WaypointNode waypoint, WaypointLink link)
        {
            if (waypoint == null || _characterCore == null || Agent == null || Agent.Value == null)
                return;

            Vector2 position = Agent.Value.transform.position;
            Vector2 target = waypoint.transform.position;
            float deltaX = target.x - position.x;

            _characterCore.AIHorizontal = Mathf.Abs(deltaX) > HorizontalStopDistance.Value
                ? (int)Mathf.Sign(deltaX)
                : 0;

            _characterCore.AIVertical = GetVerticalInput(position, target, link);

            UpdateJumpInput(link);
        }

        private int GetVerticalInput(Vector2 position, Vector2 target, WaypointLink link)
        {
            if (link != null && link.type == WaypointLinkType.Jump)
                return 1;

            if (target.y > position.y)
                return 1;

            if (target.y < position.y)
                return -1;

            return 0;
        }

        private void UpdateJumpInput(WaypointLink link)
        {
            if (_characterCore == null)
                return;

            _characterCore.AIJumpHeld = false;

            if (link == null || link.type != WaypointLinkType.Jump)
                return;

            if (!_jumpRequested && _characterCore.Rigidbody.grounded)
            {
                _characterCore.Controller.RequestJump(JumpForceMultiplier.Value);
                _jumpHoldTimer = JumpHoldDuration.Value;
                _jumpRequested = true;
            }

            if (_jumpHoldTimer > 0f)
            {
                _jumpHoldTimer -= Time.deltaTime;
                _characterCore.AIJumpHeld = true;
            }
        }

        private bool HasArrivedAt(WaypointNode waypoint)
        {
            if (waypoint == null || Agent == null || Agent.Value == null)
                return false;

            return Vector2.Distance(Agent.Value.transform.position, waypoint.transform.position) <= ArrivalRadius.Value;
        }

        private bool IsStuck()
        {
            if (_nextWaypoint == null || Agent == null || Agent.Value == null)
                return false;

            _stuckCheckTimer += Time.deltaTime;

            if (_stuckCheckTimer < StuckCheckInterval.Value)
                return false;

            _stuckCheckTimer = 0f;

            float currentDistance = Vector2.Distance(Agent.Value.transform.position, _nextWaypoint.transform.position);
            float progress = _lastDistanceToNext - currentDistance;

            if (progress < StuckMinProgress.Value)
                _stuckCheckCount++;
            else
                _stuckCheckCount = 0;

            _lastDistanceToNext = currentDistance;

            return _stuckCheckCount >= Math.Max(1, MaxStuckChecks.Value);
        }

        private void ResetMovementProgress()
        {
            _linkTimer = 0f;
            _jumpHoldTimer = 0f;
            _jumpRequested = false;
            _stuckCheckTimer = 0f;
            _stuckCheckCount = 0;

            if (_nextWaypoint == null || Agent == null || Agent.Value == null)
                _lastDistanceToNext = float.PositiveInfinity;
            else
                _lastDistanceToNext = Vector2.Distance(Agent.Value.transform.position, _nextWaypoint.transform.position);

            StopInput();
        }

        private bool TryFindPathBfs(WaypointNode start, WaypointNode goal, List<WaypointLink> path)
        {
            path.Clear();

            if (start == null || goal == null || start == goal)
                return false;

            Queue<WaypointNode> queue = new Queue<WaypointNode>();
            HashSet<WaypointNode> visited = new HashSet<WaypointNode>();
            Dictionary<WaypointNode, WaypointNode> cameFromNode = new Dictionary<WaypointNode, WaypointNode>();
            Dictionary<WaypointNode, WaypointLink> cameFromLink = new Dictionary<WaypointNode, WaypointLink>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                WaypointNode current = queue.Dequeue();

                if (current == goal)
                {
                    ReconstructPath(start, goal, cameFromNode, cameFromLink, path);
                    return path.Count > 0;
                }

                foreach (WaypointLink link in GetShuffledLinks(current))
                {
                    if (link == null || link.target == null)
                        continue;

                    if (visited.Contains(link.target))
                        continue;

                    visited.Add(link.target);
                    cameFromNode[link.target] = current;
                    cameFromLink[link.target] = link;
                    queue.Enqueue(link.target);
                }
            }

            return false;
        }

        private static void ReconstructPath(
            WaypointNode start,
            WaypointNode goal,
            Dictionary<WaypointNode, WaypointNode> cameFromNode,
            Dictionary<WaypointNode, WaypointLink> cameFromLink,
            List<WaypointLink> path)
        {
            path.Clear();

            WaypointNode current = goal;

            while (current != start)
            {
                if (!cameFromNode.ContainsKey(current))
                {
                    path.Clear();
                    return;
                }

                WaypointLink link = cameFromLink[current];
                path.Add(link);
                current = cameFromNode[current];
            }

            path.Reverse();
        }

        private static List<WaypointLink> GetShuffledLinks(WaypointNode waypoint)
        {
            List<WaypointLink> links = waypoint.links == null
                ? new List<WaypointLink>()
                : new List<WaypointLink>(waypoint.links);

            for (int i = 0; i < links.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, links.Count);

                WaypointLink temp = links[i];
                links[i] = links[randomIndex];
                links[randomIndex] = temp;
            }

            return links;
        }

        private WaypointNode FindNearestWaypoint()
        {
            WaypointNode nearest = null;
            float nearestDistance = float.PositiveInfinity;
            Vector2 position = Agent.Value.transform.position;

            foreach (WaypointNode waypoint in _waypoints)
            {
                if (waypoint == null)
                    continue;

                Vector2 waypointPosition = waypoint.transform.position;
                float verticalDifference = Mathf.Abs(waypointPosition.y - position.y);

                if (verticalDifference > MaxNearestWaypointVerticalDifference.Value)
                    continue;

                float distance = Vector2.Distance(position, waypointPosition);

                if (distance >= nearestDistance)
                    continue;

                nearest = waypoint;
                nearestDistance = distance;
            }

            return nearest;
        }

        private WaypointNode PickReachableRandomWaypoint(WaypointNode start)
        {
            List<WaypointNode> candidates = new List<WaypointNode>();

            foreach (WaypointNode waypoint in _waypoints)
            {
                if (waypoint != null && waypoint != start)
                    candidates.Add(waypoint);
            }

            Shuffle(candidates);

            foreach (WaypointNode candidate in candidates)
            {
                if (TryFindPathBfs(start, candidate, _path))
                    return candidate;
            }

            _path.Clear();
            return null;
        }

        private static void Shuffle<T>(List<T> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, values.Count);
                T temp = values[i];
                values[i] = values[randomIndex];
                values[randomIndex] = temp;
            }
        }

        private void StopInput()
        {
            if (_characterCore == null)
                return;

            _characterCore.AIHorizontal = 0;
            _characterCore.AIVertical = 0;
            _characterCore.AIJumpHeld = false;
        }
    }
}
