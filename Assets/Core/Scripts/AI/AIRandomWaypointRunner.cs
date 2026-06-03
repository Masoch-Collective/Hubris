using Character;
using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(CharacterCore))]
    public class AIRandomWaypointRunner : MonoBehaviour
    {
        [SerializeField] private WaypointNode currentWaypoint;
        [SerializeField] private float arrivalRadius = 0.35f;
        [SerializeField] private float horizontalStopDistance = 0.1f;
        [SerializeField] private float jumpHoldDuration = 0.55f;
        [SerializeField] private float linkTimeout = 4f;

        private CharacterCore _characterCore;
        private WaypointLink _currentLink;
        private WaypointNode _targetWaypoint;
        private float _linkTimer;
        private float _jumpHoldTimer;
        private bool _jumpRequested;

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
            if (_characterCore == null)
                return;

            _characterCore.AIHorizontal = 0;
            _characterCore.AIVertical = 0;
            _characterCore.AIJumpHeld = false;
            _characterCore.UseAIInput = false;
        }

        private void Start()
        {
            currentWaypoint ??= FindNearestWaypoint();
            PickNextLink();
        }

        private void Update()
        {
            if (_targetWaypoint == null)
                PickNextLink();

            if (_targetWaypoint == null)
            {
                StopInput();
                return;
            }

            _linkTimer += Time.deltaTime;
            if (_linkTimer >= linkTimeout)
            {
                currentWaypoint = FindNearestWaypoint();
                PickNextLink();
                return;
            }

            Vector2 position = transform.position;
            Vector2 target = _targetWaypoint.transform.position;
            float deltaX = target.x - position.x;

            _characterCore.AIHorizontal = Mathf.Abs(deltaX) > horizontalStopDistance ? (int)Mathf.Sign(deltaX) : 0;
            _characterCore.AIVertical = _currentLink != null && _currentLink.type == WaypointLinkType.Jump
                ? 1
                : target.y > position.y ? 1 : target.y < position.y ? -1 : 0;

            UpdateJumpInput();

            if (Vector2.Distance(position, target) <= arrivalRadius)
            {
                currentWaypoint = _targetWaypoint;
                PickNextLink();
            }
        }

        private void UpdateJumpInput()
        {
            _characterCore.AIJumpHeld = false;

            if (_currentLink == null || _currentLink.type != WaypointLinkType.Jump)
                return;

            if (!_jumpRequested && _characterCore.Rigidbody.grounded)
            {
                _characterCore.Controller.RequestJump();
                _jumpHoldTimer = jumpHoldDuration;
                _jumpRequested = true;
            }

            if (_jumpHoldTimer > 0)
            {
                _jumpHoldTimer -= Time.deltaTime;
                _characterCore.AIJumpHeld = true;
            }
        }

        private void PickNextLink()
        {
            _currentLink = null;
            _targetWaypoint = null;
            _linkTimer = 0;
            _jumpHoldTimer = 0;
            _jumpRequested = false;

            if (currentWaypoint == null || currentWaypoint.links == null || currentWaypoint.links.Count == 0)
                currentWaypoint = FindNearestWaypoint();

            if (currentWaypoint == null || currentWaypoint.links == null || currentWaypoint.links.Count == 0)
                return;

            for (int attempts = 0; attempts < currentWaypoint.links.Count; attempts++)
            {
                WaypointLink link = currentWaypoint.links[Random.Range(0, currentWaypoint.links.Count)];
                if (link.target == null)
                    continue;

                _currentLink = link;
                _targetWaypoint = link.target;
                return;
            }
        }

        private WaypointNode FindNearestWaypoint()
        {
            WaypointNode[] waypoints = FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);
            WaypointNode nearest = null;
            float nearestDistance = float.PositiveInfinity;
            Vector2 position = transform.position;

            foreach (WaypointNode waypoint in waypoints)
            {
                float distance = Vector2.Distance(position, waypoint.transform.position);
                if (distance >= nearestDistance)
                    continue;

                nearest = waypoint;
                nearestDistance = distance;
            }

            return nearest;
        }

        private void StopInput()
        {
            _characterCore.AIHorizontal = 0;
            _characterCore.AIVertical = 0;
            _characterCore.AIJumpHeld = false;
        }
    }

}
