using System;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Object = UnityEngine.Object;

// ReSharper disable MemberCanBePrivate.Global

namespace Character {
    
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Controller))]
    [RequireComponent(typeof(Hitbox))]
    [RequireComponent(typeof(Parrying))]
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterCore : MonoBehaviour, IDamageable {

        private static Dictionary<Gamepad, CharacterCore> _gamepads;

        #region Components +++++
        public Collider2D Hurtbox {
            get {
                if (_hurtbox == null)
                    _hurtbox = GetComponent<Collider2D>();
                return _hurtbox;
            }
        }
        [NonSerialized] private Collider2D _hurtbox;
        public Controller Controller {
            get {
                if (_controller == null)
                    _controller = GetComponent<Controller>();
                return _controller;
            }
        }
        [NonSerialized] private Controller _controller;
        public Hitbox Hitbox {
            get {
                if (_hitbox == null)
                    _hitbox = GetComponent<Hitbox>();
                return _hitbox;
            }
        }
        [NonSerialized] private Hitbox _hitbox;
        public Parrying Parrying {
            get {
                if (_parrying == null)
                    _parrying = GetComponent<Parrying>();
                return _parrying;
            }
        }
        [NonSerialized] private Parrying _parrying;
        public Rigidbody Rigidbody {
            get {
                if (_rigidbody == null)
                    _rigidbody = GetComponent<Rigidbody>();
                return _rigidbody;
            }
        }
        [NonSerialized] private Rigidbody _rigidbody;
        public InputActionMap PlayerActions => _playerActions ??= InputSystem.actions.FindActionMap(actionSetName);
        [NonSerialized] private InputActionMap _playerActions;
        public InputActionMap SharedPlayerActions => _sharedPlayerActions ??= InputSystem.actions.FindActionMap(sharedActionSetName);
        [NonSerialized] private InputActionMap _sharedPlayerActions;
        #endregion -------------

        [Header("Input Config")]
        [SerializeField] private string actionSetName           = "Player##";
        [SerializeField] private string actionNameJump          = "Jump";
        [SerializeField] private string actionNameAttack        = "Attack";
        [SerializeField] private string actionNameParry         = "Parry";
        [SerializeField] private string actionNameHorizontal    = "Horizontal";
        [SerializeField] private string actionNameVertical      = "Vertical";
        [SerializeField] private string sharedActionSetName     = "AllPlayers";
        [SerializeField] private string sharedActionNameStart   = "Start";
        [SerializeField, Range(0, 1)] private float digitalAxisThreshold = 0.25f;

        [Header("Debugging")]
        [SerializeField] private float debugArrowScale = 0.25f;
        [SerializeField] private float debugArrowOffset = 1;
        
        #region Actions ++++++++
        public InputAction ActionJump       => _actionJump          ??= PlayerActions[actionNameJump];
        [NonSerialized] private InputAction _actionJump;
        public InputAction ActionAttack     => _actionAttack        ??= PlayerActions[actionNameAttack];
        [NonSerialized] private InputAction _actionAttack;
        public InputAction ActionParry      => _actionParry         ??= PlayerActions[actionNameParry];
        [NonSerialized] private InputAction _actionParry;
        public InputAction ActionHorizontal => _actionHorizontal    ??= PlayerActions[actionNameHorizontal];
        [NonSerialized] private InputAction _actionHorizontal;
        public InputAction ActionVertical   => _actionVertical      ??= PlayerActions[actionNameVertical];
        [NonSerialized] private InputAction _actionVertical;
        public InputAction SharedActionStart=> _sharedActionStart   ??= SharedPlayerActions[sharedActionNameStart];
        [NonSerialized] private InputAction _sharedActionStart;
        #endregion -------------

        public int DigitalAxisHorizontal {
            get {
                float value = ActionHorizontal.ReadValue<float>();
                if (value < -digitalAxisThreshold)
                    return -1;
                if (value > digitalAxisThreshold)
                    return 1;
                return 0;
            }
        }
        public int DigitalAxisVertical {
            get {
                float value = ActionVertical.ReadValue<float>();
                if (value < -digitalAxisThreshold)
                    return -1;
                if (value > digitalAxisThreshold)
                    return 1;
                return 0;
            }
        }

        /// <summary>
        /// List of devices that are allowed to control this character (Gamepads get culled on Start(), then added on RegisterGamepad() so that only one gamepad may control this character.)
        /// </summary>
        [field: NonSerialized] private List<InputDevice> _allowedDevices;
        [field:NonSerialized] public Gamepad Gamepad { get; private set; }
        [NonSerialized] private int _facing = 1;
        private InputAction _respawnCompoundAction;

        public void Start() {
            
            // Create a list of devices that are allowed to control this character, then subtract gamepads from it
            if (PlayerActions.devices != null) 
                _allowedDevices = new List<InputDevice>(PlayerActions.devices.Value.ToArray());
            else {
                Debug.LogWarning($"PlayerActions {PlayerActions.name} devices list was null?! Defaulting to all.");
                _allowedDevices = new List<InputDevice>(InputSystem.devices);
            }
            for (int i = 0; i < _allowedDevices.Count;)
                if (_allowedDevices[i] is Gamepad) {
                    Debug.Log($"Culled {_allowedDevices[i].name}");
                    _allowedDevices.RemoveAt(i);
                } else
                    i++;
            PlayerActions.devices = new ReadOnlyArray<InputDevice>(_allowedDevices.ToArray());

            //Register events
            SharedActionStart.performed += PairGamepad;
            ActionAttack.started += Attack;
            ActionParry.started += Parry;
            ActionHorizontal.performed += context => {
                if (context.ReadValue<float>() < -digitalAxisThreshold)
                    _facing = -1;
                if (context.ReadValue<float>() > digitalAxisThreshold)
                    _facing = 1;
                // Flip the character to reflect input direction
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * _facing;
                transform.localScale = scale;
            };

        }

        /// <summary>
        /// Attempts to pair whatever gamepad called this function to this character.
        /// </summary>
        /// <param name="context">InputAction CallbackContext containing Action info (including which gamepad triggered the event.)</param>
        private void PairGamepad(InputAction.CallbackContext context) {

            // Check if this character is already paired to a gamepad
            if (Gamepad != null) {
                Debug.LogWarning($"Attempted to pair, but {name} is already paired to {Gamepad.name}");
                return;
            }

            Gamepad gamepad = context.control.device as Gamepad;

            // Ignore if this action was not called by a gamepad (shouldn't be possible; ensure only gamepad inputs are registered to the Start InputAction
            if (context.control == null || context.control.device == null || gamepad == null) {
                Debug.LogWarning("Attempted to pair but action was not performed by a gamepad.", this);
                return;
            }

            // Ignore if this gamepad is in the list of paired gamepads
            if (_gamepads != null && _gamepads.TryGetValue(gamepad, out var pairedChar)) {
                Debug.LogWarning($"Attempted to pair {gamepad.name} to {name} but it was already paired to {pairedChar.name}");
                if (pairedChar.Gamepad != null && pairedChar.Gamepad != gamepad)
                    Debug.LogError($"Found {pairedChar.name} in the _gamepads list with the key {gamepad.name}, but its gamepad is {pairedChar.Gamepad.name}?!");
                return;
            }

            // Initialize _gamepads list if it does not exist
            if (_gamepads == null) {
                Debug.Log("No gamepad/character database exists. Creating one now and adding GamepadDisconnected to onDeviceChange event...");
                // Since _gamepads is static, we know that if it's null, then we haven't added GamepadDisconnected to the onDeviceChange event
                InputSystem.onDeviceChange += InputDevicesChanged;
                _gamepads = new Dictionary<Gamepad, CharacterCore>();
            }
            
            // Action was called by a valid, unclaimed gamepad! Pair this character to this gamepad
            Gamepad = gamepad;
            _gamepads.Add(Gamepad, this);
            // Add gamepad to list of devices that that our InputActionMap listens to
            _allowedDevices.Add(Gamepad);
            PlayerActions.devices = new ReadOnlyArray<InputDevice>(_allowedDevices.ToArray());
            Debug.Log($"Paired Gamepad {Gamepad.name} to {name}.");
            
        }

        /// <summary>
        /// Unpairs this character from its gamepad
        /// </summary>
        private void UnpairGamepad() {
            Debug.Log($"Gamepad {Gamepad.name}, belonging to {name}, was disconnected.");
            // Remove gamepad from list of devices that our InputActionMap listens to
            _allowedDevices.Remove(Gamepad);
            PlayerActions.devices = new ReadOnlyArray<InputDevice>(_allowedDevices.ToArray());
            _gamepads.Remove(Gamepad);
            Gamepad = null;
        }

        /// <summary>
        /// Handle InputSystem device change events. If the event is a gamepad disconnecting, it will be unpaired.
        /// </summary>
        /// <param name="device">Provided by event.</param>
        /// <param name="change">Provided by event.</param>
        private static void InputDevicesChanged(InputDevice device, InputDeviceChange change) {
            switch (change) {
                case InputDeviceChange.Disabled:
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Removed:
                    if (device is Gamepad gamepad && _gamepads.ContainsKey(gamepad))
                            _gamepads[gamepad].UnpairGamepad();
                    break;
            }
        }

        private void Parry(InputAction.CallbackContext c) {
            if (Hitbox.Status != Hitbox.AttackStatus.Idle || Parrying.Status != Hitbox.AttackStatus.Idle)
                return; // Abort parry if parrying or attacking
            switch (DigitalAxisVertical) {
                case < 0:
                    Parrying.Parry(Hitbox.AttackType.Downwards);
                    break;
                case > 0:
                    Parrying.Parry(Hitbox.AttackType.Upwards);
                    break;
                default: {
                    // What do we do if a parry is initiated with neutral vertical?
                    return; // For now, just ignore the parry.
                }
            }
        }

        private void Attack(InputAction.CallbackContext c) {
            if (Hitbox.Status != Hitbox.AttackStatus.Idle || Parrying.Status != Hitbox.AttackStatus.Idle)
                return; // Abort attack if parrying or attacking
            switch (DigitalAxisVertical) {
                case < 0:
                    Hitbox.Attack(Hitbox.AttackType.Downwards);
                    break;
                case > 0:
                    Hitbox.Attack(Hitbox.AttackType.Upwards);
                    break;
                default: {
                    // What do we do if an attack is initiated with neutral vertical?
                    return; // For now, just ignore the attack.
                }
            }
        }

        public void Damage(Object attacker) {
            Debug.Log($"Attack from {attacker} landed on {name}", this);
            // Implement parry handling here
            if (Parrying.Status == Hitbox.AttackStatus.Active)
                return;
            Die();
        }

        public void Die() {
            // Spawn death VFX and stuff here ig!
            gameObject.SetActive(false);

            // Combine Jump, Attack and Parry bindings into one InputAction so any of the three can be used to respawn
            if (_respawnCompoundAction == null) {
                _respawnCompoundAction = new InputAction($"Compound Action from {actionSetName} ActionMap", InputActionType.Button);
                foreach (var binding in ActionJump.bindings)
                    _respawnCompoundAction.AddBinding(binding);
                foreach (var binding in ActionAttack.bindings)
                    _respawnCompoundAction.AddBinding(binding);
                foreach (var binding in ActionParry.bindings)
                    _respawnCompoundAction.AddBinding(binding);
                _respawnCompoundAction.Enable();
            }

            Respawner.Enqueue(this, _respawnCompoundAction);
        }

        private void OnDrawGizmos() {
            
            // If not attacking/parrying, and vertical axis is neutral, don't draw debugging symbols
            if (Hitbox.Status == Hitbox.AttackStatus.Idle && Parrying.Status == Hitbox.AttackStatus.Idle && DigitalAxisVertical == 0)
                return;
            // Debug draw the direction of the attack/parry being performed (or the direction being held if no attack is in progress)
            Vector3Int offset;
            Color colOutline;
            Color colFill = Color.clear;
            if (Hitbox.Status != Hitbox.AttackStatus.Idle || Parrying.Status != Hitbox.AttackStatus.Idle) {
                // If parrying or attacking, draw the arrow with the colour matching the status and the direction of the current action
                bool attacking = Hitbox.Status != Hitbox.AttackStatus.Idle;
                Hitbox.AttackStatus status = attacking ? Hitbox.Status : Parrying.Status;
                colFill = colOutline = Hitbox.VizColor(status);
                colFill.a = Hitbox.vizOpacityEmpty;
                offset = (attacking ? Hitbox.Type : Parrying.Type) switch {
                    Hitbox.AttackType.Upwards => Vector3Int.up,
                    Hitbox.AttackType.Downwards => Vector3Int.down,
                    _ => default
                };
            } else {
                offset = Vector3Int.up * DigitalAxisVertical;
                colOutline = DigitalAxisVertical == 0 ? Color.clear : Hitbox.colIdle;
            }
            
            Utils.Miscellaneous.DrawArrowGizmo(transform.position, colFill, colOutline, offset.y == 1 ? 0 : 180, debugArrowScale, debugArrowOffset);
            
        }

    }

}