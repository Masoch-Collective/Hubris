using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Character.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Hitbox))]
    public class HitboxEditor : UnityEditor.Editor {
        public Hitbox Hitbox {
            get {
                if (_hitbox == null)
                    _hitbox = (Hitbox)target;
                return _hitbox;
            }
        }
        [NonSerialized] private Hitbox _hitbox; 
        
        private string _animatorTrigger;
        private float _totalDuration;
        private float _hurtEnd;
        
        private void OnEnable() {
            _totalDuration = Hitbox.windupDuration + Hitbox.hurtDuration + Hitbox.cooldownDuration;
            _hurtEnd = Hitbox.windupDuration + Hitbox.hurtDuration;
        }

        public override void OnInspectorGUI() {

            #region Config +++++++++
            Hitbox.opponentLayerIndex = EditorGUILayout.LayerField("Opponent Layer", Hitbox.opponentLayerIndex);
            Hitbox.Collider.includeLayers = 1 << Hitbox.opponentLayerIndex;
            EditorGUI.BeginDisabledGroup(true);
            if (Hitbox.OpponentInHitbox)
                EditorGUILayout.ObjectField("In Hitbox", Hitbox.Opponent?.Hurtbox.gameObject, typeof(GameObject), true);
            else
                EditorGUILayout.TextField("In Hitbox", "None");
            EditorGUI.EndDisabledGroup();
            Hitbox.useAnimationEvents = EditorGUILayout.Toggle("Animated", Hitbox.useAnimationEvents);
            if (Hitbox.useAnimationEvents) {
                
                Hitbox.animator = (Animator)EditorGUILayout.ObjectField("Animator", Hitbox.animator, typeof(Animator), Hitbox.animator);
                if (!Hitbox.animator)
                    EditorGUILayout.HelpBox("Animated mode requires an Animator component to trigger on attack.", MessageType.Error, false);
                else
                    Hitbox.animationTriggerHash =
                        Animator.StringToHash(EditorGUILayout.TextField("Trigger", _animatorTrigger));
                EditorGUILayout.HelpBox("Make sure your animation has an Animation Event that calls AttackEnd at the end.\n" +
                                        "Additionally, add HurtStart and HurtEnd (or HurtForSeconds) Animation Events to specify when the hitbox should be active.", MessageType.Info, true);
                
            } else {
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
                
                _totalDuration = Hitbox.windupDuration + Hitbox.hurtDuration + Hitbox.cooldownDuration;
                _hurtEnd = Hitbox.windupDuration + Hitbox.hurtDuration;
                
                EditorGUILayout.MinMaxSlider(ref Hitbox.windupDuration, ref _hurtEnd, 0, _totalDuration);
                
                Hitbox.hurtDuration = _hurtEnd - Hitbox.windupDuration;
                Hitbox.cooldownDuration = _totalDuration - _hurtEnd;
                
                Hitbox.windupDuration = Mathf.Max(EditorGUILayout.FloatField("Windup", Hitbox.windupDuration), 0);
                Hitbox.hurtDuration = Mathf.Max(EditorGUILayout.FloatField("Hurt Duration", Hitbox.hurtDuration), 0);
                Hitbox.cooldownDuration = Mathf.Max(EditorGUILayout.FloatField("Cooldown", Hitbox.cooldownDuration), 0);
                
            }
            #endregion -------------
            
            EditorGUILayout.Space();
            
            #region Status +++++++++
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            GUIContent groundedStatus = null;
            Color defaultTextCol = EditorStyles.label.normal.textColor;
            bool err = false;
            switch (Hitbox.Status) {

                case Hitbox.AttackStatus.Idle:
                    EditorStyles.label.normal.textColor = Hitbox.colIdle;
                    groundedStatus = new GUIContent("Idle");
                    break;

                case Hitbox.AttackStatus.Windup:
                    EditorStyles.label.normal.textColor = Hitbox.colWindup;
                    groundedStatus = new GUIContent("Winding Up");
                    break;

                case Hitbox.AttackStatus.Hurting:
                    EditorStyles.label.normal.textColor = Hitbox.colHurting;
                    groundedStatus = new GUIContent("Hurting");
                    break;

                case Hitbox.AttackStatus.Cooldown:
                    EditorStyles.label.normal.textColor = Hitbox.colCooldown;
                    groundedStatus = new GUIContent("Cooling Down");
                    break;

                default:
                    err = true;
                    EditorGUILayout.HelpBox("Invalid status?!", MessageType.Error);
                    break;
                
            }
            if (!err)
                EditorGUILayout.LabelField(groundedStatus);
            EditorStyles.label.normal.textColor = defaultTextCol;
            #endregion -------------
            
            EditorGUILayout.Space();
            
            #region Debugging ++++++
            Hitbox.useVisualizer = EditorGUILayout.BeginFoldoutHeaderGroup(Hitbox.useVisualizer, Hitbox.useVisualizer ? "Visualization Enabled" : "Enable Visualization");
            if (Hitbox.useVisualizer) {
                Hitbox.colIdle          = EditorGUILayout.ColorField("Idle",     Hitbox.colIdle       );
                Hitbox.colWindup        = EditorGUILayout.ColorField("Windup",   Hitbox.colWindup     );
                Hitbox.colHurting       = EditorGUILayout.ColorField("Hurting",  Hitbox.colHurting    );
                Hitbox.colCooldown      = EditorGUILayout.ColorField("Cooldown", Hitbox.colCooldown   );
                Hitbox.vizOpacityEmpty  = EditorGUILayout.Slider("Opacity (Empty)", Hitbox.vizOpacityEmpty, 0, 1);
                Hitbox.vizOpacityHasOpp = EditorGUILayout.Slider("Opacity (Opponent in Trigger)", Hitbox.vizOpacityHasOpp, 0, 1);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion -------------
            
            serializedObject.ApplyModifiedProperties();
            
        }

    }

}