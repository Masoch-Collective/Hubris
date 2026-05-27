using System;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// ReSharper disable MemberCanBePrivate.Global

namespace Character {
    
    public class CharacterCore : MonoBehaviour, IDamageable {

        private static Dictionary<Gamepad, CharacterCore> _gamepads;
        
        #region Enums & Structs
        [Flags] public enum CharacterStatus {
            Idle        = 1 << 0,
            Attacking   = 1 << 1,
            Parrying    = 1 << 2,
            Stunned     = 1 << 3,
            Dead        = 1 << 4
        }
        [Flags] public enum ActionStage {
            Idle        = 1 << 0,
            Windup      = 1 << 1,
            Active      = 1 << 2,
            Cooldown    = 1 << 3
        }
        public enum ActionType {
            Neutral = 0,
            Upwards = 1,
            Downwards = -1
        }
        #endregion

        #region Components +++++
        public Animator Animator {
            get {
                if (_animator == null)
                    _animator = GetComponent<Animator>();
                return _animator;
            }
        }
        [NonSerialized] private Animator _animator;
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

        public event Action<CharacterCore> OnDeath;
        public event Action OnStunEnd;
        public event Action<CharacterStatus> OnStatusChanged;

        
        public CharacterStatus Status {
            get => statusBackingField;
            private set {
                statusBackingField = value;
                if (value != statusBackingField && OnStatusChanged != null)
                    OnStatusChanged.Invoke(statusBackingField);
            }
        }
        [SerializeField] private CharacterStatus statusBackingField = CharacterStatus.Idle;
        public ActionType LastActionType {
            get {
                if (Status == CharacterStatus.Idle && DigitalAxisVertical != 0)
                    _actionType = (ActionType)DigitalAxisVertical;
                return _actionType;
            }
        }
        [NonSerialized] private ActionType _actionType = ActionType.Upwards;

        #region Config Fields ++

        [Header("Control Config")]
        [field:SerializeField] public ControlStatusConfig AllowFacingDirectionChanges { get; private set; }
        [field:SerializeField] public ControlStatusConfig AllowRunning { get; private set; }
        [field:SerializeField] public ControlStatusConfig AllowJumping { get; private set; }
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

        [Header("Animator Config")]
        [SerializeField] private string animParamIntActionType  = "I_Type"; // Flipped order of field and property here so the Header attribute works. I hate it, but... it is what it is!
        [field:NonSerialized] public int AnimHashIntActionType  {get; private set; }
        
        [field:NonSerialized] public int AnimHashBoolStunned    {get; private set; }
        [SerializeField] private string animParamBoolStunned    = "B_Stunned";
        [field:NonSerialized] public int AnimHashBoolGrounded   {get; private set; }
        [SerializeField] private string animParamBoolGrounded   = "B_Grounded";
        [field:NonSerialized] public int AnimHashBoolRunning    {get; private set; }
        [SerializeField] private string animParamBoolRunning    = "B_Running";
        [field:NonSerialized] public int AnimHashTriggerAttack  {get; private set; }
        [SerializeField] private string animParamTriggerAttack  = "T_Attack";
        [field:NonSerialized] public int AnimHashTriggerParry   {get; private set; }
        [SerializeField] private string animParamTriggerParry   = "T_Parry";

        [Header("Stun Config")]
        [SerializeField] private float stunDuration;
        [SerializeField, Range(0, 1)] private float stunTimerNormalized;
        [NonSerialized] private float _stunTimer;

        [Header("Debugging")]
        [SerializeField] private float debugArrowScale = 0.25f;
        [SerializeField] private float debugArrowOffset = 1;
        
        #endregion
        
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
            _allowedDevices = new List<InputDevice>(PlayerActions.devices != null ? 
                    PlayerActions.devices.Value.ToArray() : 
                    InputSystem.devices);
            for (int i = 0; i < _allowedDevices.Count;)
                if (_allowedDevices[i] is Gamepad)
                    _allowedDevices.RemoveAt(i);
                else
                    i++;
            PlayerActions.devices = new ReadOnlyArray<InputDevice>(_allowedDevices.ToArray());

            //Register events
            ActionAttack        .started        += Attack;
            ActionParry         .started        += Parry;
            SharedActionStart   .performed      += PairGamepad;
            ActionHorizontal    .performed      += UpdateFacingDirection;
            ActionVertical      .performed      += UpdateActionType;
            
            Hitbox              .OnAttackEnd    += ReturnToIdle;
            Parrying            .OnParryEnd     += ReturnToIdle;
            OnDeath += killer => CombatLoopManager.Instance.CharacterEliminated(killer, this);
            
        }

        private void UpdateFacingDirection(InputAction.CallbackContext _) => UpdateFacingDirection();
        private void UpdateFacingDirection() => UpdateFacingDirection(DigitalAxisHorizontal);
        private void UpdateFacingDirection(int direction) {
            // Don't change character's facing direction if stunned or input is within deadzone
            if (!AllowFacingDirectionChanges.Evaluate(this) || direction == 0) return;
            _facing = Math.Sign(direction);
            if (_facing == 0) _facing = 1; // Default to facing forward if for some reason facing is zero (which would result in zero-scale character)

            // Flip the character to reflect input direction
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * _facing;
            transform.localScale = scale;
        }

        private void UpdateActionType(InputAction.CallbackContext _) => UpdateActionType();
        private void UpdateActionType() {
            if (Animator)
                Animator.SetInteger(AnimHashIntActionType, (int)LastActionType);
        }

        private void Update() {
            if (Status == CharacterStatus.Stunned) {
                _stunTimer += Time.deltaTime;
                stunTimerNormalized = _stunTimer / stunDuration;
                if (_stunTimer >= stunDuration) {
                    ReturnToIdle(CharacterStatus.Stunned);
                    if (OnStunEnd != null) OnStunEnd.Invoke();
                }
            }
        }

        /// <summary>
        /// Attempts to pair whatever gamepad called this function to this character.
        /// </summary>
        /// <param name="context">InputAction CallbackContext containing Action info (including which gamepad triggered the event.)</param>
        private void PairGamepad(InputAction.CallbackContext context) {

            // Check if this character is already paired to a gamepad
            if (Gamepad != null)
                return;

            Gamepad gamepad = context.control.device as Gamepad;

            // Ignore if this action was not called by a gamepad (shouldn't be possible; ensure only gamepad inputs are registered to the Start InputAction
            if (context.control == null || context.control.device == null || gamepad == null) {
                Debug.LogWarning("Attempted to pair but action was not performed by a gamepad.", this);
                return;
            }

            // Ignore if this gamepad is in the list of paired gamepads
            if (_gamepads != null && _gamepads.TryGetValue(gamepad, out var pairedChar)) {
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
            if (Status != CharacterStatus.Idle)
                return; // Abort parry if not idle
            Parrying.Parry(LastActionType);
            Status = CharacterStatus.Parrying;
        }

        private void Attack(InputAction.CallbackContext c) {
            if (Status != CharacterStatus.Idle)
                return; // Abort attack if not idle
            Hitbox.Attack(LastActionType);
            Status = CharacterStatus.Attacking;
        }

        public ActionType ReceiveDamage(CharacterCore attacker, ActionType type) {
            if (Status == CharacterStatus.Attacking && Hitbox.Stage <= ActionStage.Active) {
                Stun();
                // This might not be ideal, since technically we're not differentiating between a parry-induced stun and a simultaneous attack mutual stun
                // Though when evaluating the returned value we can just check if this Core's status is Attacking
                return attacker.Hitbox.Type;
            }
            if (Status == CharacterStatus.Parrying && Parrying.Stage == ActionStage.Active) {
                if (Parrying.Type == type) {
                    // ========= Perfect parry! ========= //
                    PerfectParried(attacker);
                } else {
                    // ========= Bad parry! ========= //
                    BadParried(attacker);
                }
            } else {
                // ========= No parry! ========= //
                Die(attacker);
            }
            return Status != CharacterStatus.Parrying ? ActionType.Neutral : Parrying.Type;
        }
        
        private void PerfectParried(CharacterCore opponent) {
        }

        private void BadParried(CharacterCore opponent) {
            Stun();
        }
        
        public void Stun() {
            // Reset actions
            Hitbox.ForceReset();
            Parrying.ForceReset();
            // Reset stun timer
            _stunTimer = 0;
            // Enter state
            Status = CharacterStatus.Stunned;
            if (Animator)
                Animator.SetBool(AnimHashBoolStunned, true);
        }

        public void Die(CharacterCore opponent) {
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
            
            if (OnDeath != null) OnDeath.Invoke(opponent);
            
            Reset(); // Important order of operations: Reset must occur before Status = Dead, as the former sets Status to Idle
            Status = CharacterStatus.Dead;
        }

        public void Spawned() {
            Reset();
            ReturnToIdle(CharacterStatus.Dead);
        }

        private void Reset() {
            Hitbox.ForceReset();
            Parrying.ForceReset();
            _stunTimer = 0;
        }

        /// <summary>
        /// Sets the player's state to idle. Pass Idle state as parameter to force return to idle from any state.
        /// </summary>
        /// <param name="from">State the player needs to be in to return to idle.</param>
        public void ReturnToIdle(CharacterStatus from) {
            Status = CharacterStatus.Idle;
            UpdateFacingDirection();
            UpdateActionType();
            if (Animator)
                Animator.SetBool(AnimHashBoolStunned, false);
        }

        private void OnDrawGizmos() {
            if (Status == CharacterStatus.Stunned) {
                Utils.Miscellaneous.DrawExclamationGizmo(transform.position + Vector3.up, Color.orangeRed, 0.25f, 0.5f);
            }
            // If not attacking/parrying, and vertical axis is neutral, don't draw debugging symbols
            if (Status == CharacterStatus.Idle && DigitalAxisVertical == 0)
                return;
            // Debug draw the direction of the attack/parry being performed (or the direction being held if no attack is in progress)
            Vector3Int offset;
            Color colOutline;
            Color colFill = Color.clear;
            if (Status == CharacterStatus.Attacking || Status == CharacterStatus.Parrying) {
                // If parrying or attacking, draw the arrow with the colour matching the status and the direction of the current action
                ActionStage stage = Status switch {
                    CharacterStatus.Parrying => Parrying.Stage,
                    CharacterStatus.Attacking => Hitbox.Stage,
                    _ => ActionStage.Idle
                };
                colFill = colOutline = Hitbox.VizColor(stage);
                colFill.a = Hitbox.vizOpacityEmpty;
                offset = (Status == CharacterStatus.Attacking ? Hitbox.Type : Parrying.Type) switch {
                    ActionType.Upwards => Vector3Int.up,
                    ActionType.Downwards => Vector3Int.down,
                    _ => default
                };
            } else {
                offset = Vector3Int.up * (int)LastActionType;
                colOutline = LastActionType == 0 ? Color.clear : Hitbox.colIdle;
            }
            
            Utils.Miscellaneous.DrawArrowGizmo(transform.position, colFill, colOutline, offset.y == 1 ? 0 : 180, debugArrowScale, debugArrowOffset);
            
        }

        //TODO: Verify that this works in builds; if not, remove NonSerialized attributes
        private void OnValidate() {
            AnimHashIntActionType = Animator.StringToHash(animParamIntActionType);
            AnimHashBoolStunned   = Animator.StringToHash(animParamBoolStunned);
            AnimHashBoolGrounded  = Animator.StringToHash(animParamBoolGrounded);
            AnimHashBoolRunning   = Animator.StringToHash(animParamBoolRunning);
            AnimHashTriggerAttack = Animator.StringToHash(animParamTriggerAttack);
            AnimHashTriggerParry  = Animator.StringToHash(animParamTriggerParry);
        }

    }

}