using System.Collections.Generic;
using Character;
using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(CharacterCore))]
    public class AIRandomWaypointRunner : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private WaypointNode currentWaypoint;
        [SerializeField] private WaypointNode[] waypoints;

        [Header("Movement")]
        [SerializeField] private float arrivalRadius = 0.35f;
        [SerializeField] private float horizontalStopDistance = 0.1f;
        [SerializeField] private float jumpHoldDuration = 0.55f;

        [Header("Fail Detection")]
        [SerializeField] private float linkTimeout = 3f;
        [SerializeField] private float stuckCheckInterval = 0.5f;
        [SerializeField] private float stuckMinProgress = 0.08f;
        [SerializeField] private int maxStuckChecks = 3;

        [Header("Nearest Waypoint")]
        [SerializeField] private float maxNearestWaypointVerticalDifference = 0.8f;

        [Header("Debug Markers")]
        [SerializeField] private bool drawTargetMarkers = true;
        [SerializeField] private Color nextWaypointMarkerColor = Color.yellow;
        [SerializeField] private Color destinationMarkerColor = Color.red;
        [SerializeField] private float nextWaypointMarkerSize = 18f;
        [SerializeField] private float destinationMarkerSize = 28f;

        private readonly List<WaypointLink> _path = new();

        private CharacterCore _characterCore;

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

        private static Texture2D _markerTexture;

        private void Awake()
        {
            _characterCore = GetComponent<CharacterCore>();
        }

        private void OnEnable()
        {
            if (_characterCore == null)
                _characterCore = GetComponent<CharacterCore>();

            _characterCore.UseAIInput = true;
        }

        private void OnDisable()
        {
            StopInput();

            if (_characterCore != null)
                _characterCore.UseAIInput = false;
        }

        private void Start()
        {
            waypoints = FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);

            if (waypoints == null || waypoints.Length < 2)
            {
                Debug.LogWarning($"{name}: Not enough waypoints.");
                enabled = false;
                return;
            }

            StartNewPath();
        }

        private void Update()
        {
            if (_nextWaypoint == null)
            {
                StartNewPath();
                return;
            }

            _linkTimer += Time.deltaTime;

            MoveToward(_nextWaypoint, _currentLink);

            if (HasArrivedAt(_nextWaypoint))
            {
                ReachNextWaypoint();
                return;
            }

            if (_linkTimer >= linkTimeout || IsStuck())
            {
                StartNewPath();
            }
        }

        private void StartNewPath()
        {
            ResetMovementProgress();

            currentWaypoint = FindNearestWaypoint();

            if (currentWaypoint == null)
            {
                StopInput();
                return;
            }

            _destinationWaypoint = PickRandomWaypointExcept(currentWaypoint);

            if (_destinationWaypoint == null)
            {
                StopInput();
                return;
            }

            if (!TryFindPathBfs(currentWaypoint, _destinationWaypoint, _path))
            {
                StopInput();
                _nextWaypoint = null;
                return;
            }

            _pathIndex = 0;
            SetNextLink();
        }

        private void ReachNextWaypoint()
        {
            currentWaypoint = _nextWaypoint;
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
            ResetMovementProgress();

            if (_pathIndex < 0 || _pathIndex >= _path.Count)
            {
                StartNewPath();
                return;
            }

            _currentLink = _path[_pathIndex];
            _nextWaypoint = _currentLink.target;
        }

        private void MoveToward(WaypointNode waypoint, WaypointLink link)
        {
            if (waypoint == null || _characterCore == null)
                return;

            Vector2 position = transform.position;
            Vector2 target = waypoint.transform.position;

            float deltaX = target.x - position.x;

            _characterCore.AIHorizontal = Mathf.Abs(deltaX) > horizontalStopDistance
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
                _characterCore.Controller.RequestJump();
                _jumpHoldTimer = jumpHoldDuration;
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
            if (waypoint == null)
                return false;

            return Vector2.Distance(transform.position, waypoint.transform.position) <= arrivalRadius;
        }

        private bool IsStuck()
        {
            if (_nextWaypoint == null)
                return false;

            _stuckCheckTimer += Time.deltaTime;

            if (_stuckCheckTimer < stuckCheckInterval)
                return false;

            _stuckCheckTimer = 0f;

            float currentDistance = Vector2.Distance(transform.position, _nextWaypoint.transform.position);
            float progress = _lastDistanceToNext - currentDistance;

            if (progress < stuckMinProgress)
                _stuckCheckCount++;
            else
                _stuckCheckCount = 0;

            _lastDistanceToNext = currentDistance;

            return _stuckCheckCount >= maxStuckChecks;
        }

        private void ResetMovementProgress()
        {
            _linkTimer = 0f;
            _jumpHoldTimer = 0f;
            _jumpRequested = false;

            _stuckCheckTimer = 0f;
            _lastDistanceToNext = _nextWaypoint == null
                ? float.PositiveInfinity
                : Vector2.Distance(transform.position, _nextWaypoint.transform.position);

            _stuckCheckCount = 0;

            StopInput();
        }

        private bool TryFindPathBfs(WaypointNode start, WaypointNode goal, List<WaypointLink> path)
        {
            path.Clear();

            if (start == null || goal == null || start == goal)
                return false;

            Queue<WaypointNode> queue = new();
            HashSet<WaypointNode> visited = new();

            Dictionary<WaypointNode, WaypointNode> cameFromNode = new();
            Dictionary<WaypointNode, WaypointLink> cameFromLink = new();

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
                int randomIndex = Random.Range(i, links.Count);

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
            Vector2 position = transform.position;

            foreach (WaypointNode waypoint in waypoints)
            {
                if (waypoint == null)
                    continue;

                Vector2 waypointPosition = waypoint.transform.position;
                float verticalDifference = Mathf.Abs(waypointPosition.y - position.y);

                if (verticalDifference > maxNearestWaypointVerticalDifference)
                    continue;

                float distance = Vector2.Distance(position, waypointPosition);

                if (distance >= nearestDistance)
                    continue;

                nearest = waypoint;
                nearestDistance = distance;
            }

            return nearest;
        }

        private WaypointNode PickRandomWaypointExcept(WaypointNode excludedWaypoint)
        {
            List<WaypointNode> candidates = new();

            foreach (WaypointNode waypoint in waypoints)
            {
                if (waypoint != null && waypoint != excludedWaypoint)
                    candidates.Add(waypoint);
            }

            if (candidates.Count == 0)
                return null;

            return candidates[Random.Range(0, candidates.Count)];
        }

        private void StopInput()
        {
            if (_characterCore == null)
                return;

            _characterCore.AIHorizontal = 0;
            _characterCore.AIVertical = 0;
            _characterCore.AIJumpHeld = false;
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!drawTargetMarkers)
                return;

            Camera camera = Camera.main;

            if (camera == null)
                return;

            if (_nextWaypoint != null)
            {
                DrawWorldMarker(
                    camera,
                    _nextWaypoint.transform.position,
                    nextWaypointMarkerColor,
                    nextWaypointMarkerSize,
                    "NEXT"
                );
            }

            if (_destinationWaypoint != null)
            {
                DrawWorldMarker(
                    camera,
                    _destinationWaypoint.transform.position,
                    destinationMarkerColor,
                    destinationMarkerSize,
                    "TARGET"
                );
            }
        }
#endif

        private static void DrawWorldMarker(
            Camera camera,
            Vector3 worldPosition,
            Color color,
            float size,
            string label)
        {
            Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);

            if (screenPosition.z <= 0f)
                return;

            EnsureMarkerTexture();

            Vector2 point = new(screenPosition.x, Screen.height - screenPosition.y);
            float halfSize = size * 0.5f;

            Rect horizontal = new(point.x - halfSize, point.y - 1f, size, 2f);
            Rect vertical = new(point.x - 1f, point.y - halfSize, 2f, size);
            Rect labelRect = new(point.x + halfSize + 4f, point.y - 10f, 72f, 20f);

            Color previousColor = GUI.color;

            GUI.color = color;
            GUI.DrawTexture(horizontal, _markerTexture);
            GUI.DrawTexture(vertical, _markerTexture);
            GUI.Label(labelRect, label);
            GUI.color = previousColor;
        }

        private static void EnsureMarkerTexture()
        {
            if (_markerTexture != null)
                return;

            _markerTexture = new Texture2D(1, 1);
            _markerTexture.SetPixel(0, 0, Color.white);
            _markerTexture.Apply();
        }
    }
}