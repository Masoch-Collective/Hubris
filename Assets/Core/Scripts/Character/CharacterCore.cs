using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character {
    
    [RequireComponent(typeof(Controller))]
    public class CharacterCore : MonoBehaviour {
        
        [NonSerialized]
        private Collider2D _hurtbox;
        public Collider2D Hurtbox {
            get {
                if (_hurtbox == null)
                    _hurtbox = GetComponent<Collider2D>();
                return _hurtbox;
            }
        }
        
        [NonSerialized]
        private Controller _controller;
        public Controller Controller {
            get {
                if (_controller == null)
                    _controller = GetComponent<Controller>();
                return _controller;
            }
        }
        
        [NonSerialized]
        private Rigidbody _rigidbody;
        public Rigidbody Rigidbody {
            get {
                if (_rigidbody == null)
                    _rigidbody = GetComponent<Rigidbody>();
                return _rigidbody;
            }
        }
        
        [Header("Input Config")]
        [SerializeField] private string actionSetName = "Player##";
        [SerializeField] private string actionNameJump = "Jump";
        [SerializeField] private string actionNameAttack = "Attack";
        [SerializeField] private string actionNameParry = "Parry";
        [SerializeField] private string actionNameHorizontal = "Horizontal";
        [SerializeField] private string actionNameVertical = "Vertical";
        [SerializeField] private float digitalAxisThreshold;

        [Header("Combat Config")]
        [SerializeField] private Hitbox hitGroundDown;
        [SerializeField] private Hitbox hitGroundUp;
        [SerializeField] private Hitbox hitAerialDown;
        [SerializeField] private Hitbox hitAerialUp;

        public InputActionMap PlayerActions => _playerActions ??= InputSystem.actions.FindActionMap(actionSetName);
        [NonSerialized] private InputActionMap _playerActions;
        #region Actions ++++++++
        public InputAction ActionJump => _actionJump ??= PlayerActions[actionNameJump];
        [NonSerialized] private InputAction _actionJump;
        public InputAction ActionAttack => _actionAttack ??= PlayerActions[actionNameAttack];
        [NonSerialized] private InputAction _actionAttack;
        public InputAction ActionParry => _actionParry ??= PlayerActions[actionNameParry];
        [NonSerialized] private InputAction _actionParry;
        public InputAction ActionHorizontal => _actionHorizontal ??= PlayerActions[actionNameHorizontal];
        [NonSerialized] private InputAction _actionHorizontal;
        public InputAction ActionVertical => _actionVertical ??= PlayerActions[actionNameVertical];
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
        
        [NonSerialized] private int _facing = 1;

        public void Start() {

            ActionAttack.started += Attack;

            ActionHorizontal.performed += context => {
                if (context.ReadValue<float>() < -digitalAxisThreshold)
                    _facing = -1;
                if (context.ReadValue<float>() > digitalAxisThreshold)
                    _facing = 1;
                // Make the hitbox face in the right direction;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * _facing;
                transform.localScale = scale;
            };

        }

        public void Attack(InputAction.CallbackContext c) {
            Hitbox h;
            // Determine which hitbox corresponds to the attack we want to perform
            if (DigitalAxisVertical > 0) {
                h = Rigidbody.grounded ? hitGroundUp : hitAerialUp;
                h.Attack(Hitbox.AttackType.Upwards);
            } else if (DigitalAxisVertical < 0) {
                h = Rigidbody.grounded ? hitGroundDown : hitAerialDown;
                h.Attack(Hitbox.AttackType.Downwards);
            } else
                // What do we do if an attack is initiated with neutral vertical?
                return; // For now, just ignore the attack.
            // Activate the hitbox
        }

    }

}