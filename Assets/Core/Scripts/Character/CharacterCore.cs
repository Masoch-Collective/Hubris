using System;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

// ReSharper disable MemberCanBePrivate.Global

namespace Character {
    
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Controller))]
    [RequireComponent(typeof(Hitbox))]
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterCore : MonoBehaviour, IDamageable {

        private static Dictionary<CharacterCore, Gamepad> _gamepads;

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
        #endregion -------------
        
        [Header("Input Config")]
        [SerializeField] private string actionSetName           = "Player##";
        [SerializeField] private string actionNameJump          = "Jump";
        [SerializeField] private string actionNameAttack        = "Attack";
        [SerializeField] private string actionNameParry         = "Parry";
        [SerializeField] private string actionNameHorizontal    = "Horizontal";
        [SerializeField] private string actionNameVertical      = "Vertical";
        [SerializeField, Range(0, 1)] private float digitalAxisThreshold = 0.25f;
        
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
        [field:NonSerialized] public Gamepad Gamepad { get; private set; }
        [NonSerialized] private int _facing = 1;
        private InputAction _respawnCompoundAction;

        public void Start() {

            ActionAttack.started += context => Utils.Miscellaneous.GamepadFilter(context, Attack, Gamepad);

            ActionHorizontal.performed += context => Utils.Miscellaneous.GamepadFilter(context, _ => {
                if (context.ReadValue<float>() < -digitalAxisThreshold)
                    _facing = -1;
                if (context.ReadValue<float>() > digitalAxisThreshold)
                    _facing = 1;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * _facing;
                transform.localScale = scale;
            }, Gamepad);

        }

        private void Attack(InputAction.CallbackContext c) {
            if (Hitbox.Status != Hitbox.AttackStatus.Idle)
                return; // Abort attack if already attacking
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
            }

            Respawner.Enqueue(this, _respawnCompoundAction);
        }

    }

}