using System;
using UnityEditor;
using UnityEngine;

namespace Character.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Hitbox))]
    public class HitboxEditor : UnityEditor.Editor {

        private Hitbox _hitbox;
        private string _animatorTrigger;
        private float _duration;
        private float _hurt;

        private void OnEnable() {
            _hitbox = (Hitbox)target;
            _duration = _hitbox.windup + _hitbox.hurtDuration + _hitbox.cooldown;
            _hurt = _hitbox.windup + _hitbox.hurtDuration; 
        }

        public override void OnInspectorGUI() {

            _hitbox.opponentTag = EditorGUILayout.TextField("Opponent Tag", _hitbox.opponentTag);

            _hitbox.useAnimationEvents = EditorGUILayout.Toggle("Animated", _hitbox.useAnimationEvents);

            if (_hitbox.useAnimationEvents) {
                
                _hitbox.animator = (Animator)EditorGUILayout.ObjectField("Animator", _hitbox.animator, typeof(Animator), _hitbox.animator);
                if (!_hitbox.animator)
                    EditorGUILayout.HelpBox("Animated mode requires an Animator component to trigger on attack.", MessageType.Error, false);
                else
                    _hitbox.animationTriggerHash =
                        Animator.StringToHash(EditorGUILayout.TextField("Trigger", _animatorTrigger));
                EditorGUILayout.HelpBox("Make sure your animation has an Animation Event that calls AttackEnd at the end.\n" +
                                        "Additionally, add HurtStart and HurtEnd (or HurtForSeconds) Animation Events to specify when the hitbox should be active.", MessageType.Info, true);
                
            } else {
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
                
                _duration = _hitbox.windup + _hitbox.hurtDuration + _hitbox.cooldown;
                _hurt = _hitbox.windup + _hitbox.hurtDuration;
                
                EditorGUILayout.MinMaxSlider(ref _hitbox.windup, ref _hurt, 0, _duration);
                
                _hitbox.hurtDuration = _hurt - _hitbox.windup;
                _hitbox.cooldown = _duration - _hurt;
                
                _hitbox.windup = Mathf.Max(EditorGUILayout.FloatField("Windup", _hitbox.windup), 0);
                _hitbox.hurtDuration = Mathf.Max(EditorGUILayout.FloatField("Hurt Duration", _hitbox.hurtDuration), 0);
                _hitbox.cooldown = Mathf.Max(EditorGUILayout.FloatField("Cooldown", _hitbox.cooldown), 0);
                
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            
            GUIContent groundedStatus = null;
            Color defaultTextCol = EditorStyles.label.normal.textColor;
            bool err = false;
            switch (_hitbox.Status) {

                case Hitbox.AttackStatus.Idle:
                    EditorStyles.label.normal.textColor = Color.black;
                    groundedStatus = new GUIContent("Idle");
                    break;

                case Hitbox.AttackStatus.Windup:
                    EditorStyles.label.normal.textColor = Color.gold;
                    groundedStatus = new GUIContent("Attack Winding Up");
                    break;

                case Hitbox.AttackStatus.Hurting:
                    EditorStyles.label.normal.textColor = Color.deepPink;
                    groundedStatus = new GUIContent("Attack Hurting");
                    break;

                case Hitbox.AttackStatus.Cooldown:
                    EditorStyles.label.normal.textColor = Color.deepSkyBlue;
                    groundedStatus = new GUIContent("Attack Hurting");
                    break;

                default:
                    err = true;
                    EditorGUILayout.HelpBox("Invalid status!", MessageType.Error);
                    break;
                
            }
            if (!err)
                EditorGUILayout.LabelField(groundedStatus);
            EditorStyles.label.normal.textColor = defaultTextCol;
            
            serializedObject.ApplyModifiedProperties();
            
        }

    }

}