using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        
        private SerializedProperty _propColIdle;
        private SerializedProperty _propColWindup;
        private SerializedProperty _propColHurting;
        private SerializedProperty _propColCooldown;
        private SerializedProperty _propAnimator;
        private SerializedProperty _propOpponentLayerMask;
        private SerializedProperty _propAnimationTriggerHash;
        private SerializedProperty _propUseAnimationEvents;
        private SerializedProperty _propUseVisualizer;
        private SerializedProperty _propWindupDuration;
        private SerializedProperty _propHurtDuration;
        private SerializedProperty _propCooldownDuration;
        private SerializedProperty _propVizOpacityEmpty;
        private SerializedProperty _propVizOpacityHasOpp;
        
        private string _animatorTrigger;
        private float _totalDuration;
        private float _hurtEnd;

        private Dictionary<string, SerializedProperty> _props;
        private SerializedProperty GetSerializedProperty(string propName) {
            if ((_props??=new()).TryGetValue(propName, out var property))
                return property;
            SerializedProperty prop = serializedObject.FindProperty(propName);
            if (prop == null) {
                Debug.LogError($"Could not find serialized property named {propName}.");
                return null;
            }
            _props.Add(propName, prop);
            return prop;
        }
        
        private void OnEnable() {
            _propColIdle = GetSerializedProperty("colIdle");
            _propColWindup = GetSerializedProperty("colWindup");
            _propColHurting = GetSerializedProperty("colHurting");
            _propColCooldown = GetSerializedProperty("colCooldown");
            _propAnimator = GetSerializedProperty("animator");
            _propOpponentLayerMask = GetSerializedProperty("opponentLayerMask");
            _propAnimationTriggerHash = GetSerializedProperty("animationTriggerHash");
            _propUseAnimationEvents = GetSerializedProperty("useAnimationEvents");
            _propUseVisualizer = GetSerializedProperty("useVisualizer");
            _propWindupDuration = GetSerializedProperty("windupDuration");
            _propHurtDuration = GetSerializedProperty("hurtDuration");
            _propCooldownDuration = GetSerializedProperty("cooldownDuration");
            _propVizOpacityEmpty = GetSerializedProperty("vizOpacityEmpty");
            _propVizOpacityHasOpp = GetSerializedProperty("vizOpacityHasOpp");
        }

        public override void OnInspectorGUI() {

            #region Config +++++++++
            
            EditorGUILayout.PropertyField(_propOpponentLayerMask);
            Hitbox.Collider.excludeLayers = ~_propOpponentLayerMask.intValue;
            EditorGUI.BeginDisabledGroup(true);
            if (Hitbox.OpponentInHitbox)
                EditorGUILayout.ObjectField("In Hitbox", Hitbox.Opponent?.Hurtbox.gameObject, typeof(GameObject), true);
            else
                EditorGUILayout.TextField("In Hitbox", "None");
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(_propUseAnimationEvents, new GUIContent("Animated"));
            if (_propUseAnimationEvents.boolValue) {
                EditorGUILayout.PropertyField(_propAnimator);
                if (!_propAnimator.objectReferenceValue)
                    EditorGUILayout.HelpBox("Animated mode requires an Animator component to trigger on attack.", MessageType.Error, false);
                else
                    _propAnimationTriggerHash.intValue = Animator.StringToHash(EditorGUILayout.TextField("Trigger", _animatorTrigger));
                EditorGUILayout.HelpBox("Make sure your animation has an Animation Event that calls AttackEnd at the end.\n" +
                                        "Additionally, add HurtStart and HurtEnd (or HurtForSeconds) Animation Events to specify when the hitbox should be active.", MessageType.Info, true);
                
            } else {
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
                
                _totalDuration = _propWindupDuration.floatValue + _propHurtDuration.floatValue + _propCooldownDuration.floatValue;
                _hurtEnd = _propWindupDuration.floatValue + _propHurtDuration.floatValue;
                float windup = _propWindupDuration.floatValue;
                
                EditorGUILayout.MinMaxSlider(ref windup, ref _hurtEnd, 0, _totalDuration);

                _propWindupDuration.floatValue = windup;
                _propHurtDuration.floatValue = _hurtEnd - windup;
                _propCooldownDuration.floatValue = _totalDuration - _hurtEnd;
                
                EditorGUILayout.PropertyField(_propWindupDuration);
                EditorGUILayout.PropertyField(_propHurtDuration);
                EditorGUILayout.PropertyField(_propCooldownDuration);
                
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
                    EditorStyles.label.normal.textColor = _propColIdle.colorValue;
                    groundedStatus = new GUIContent("Idle");
                    break;

                case Hitbox.AttackStatus.Windup:
                    EditorStyles.label.normal.textColor = _propColWindup.colorValue;
                    groundedStatus = new GUIContent("Winding Up");
                    break;

                case Hitbox.AttackStatus.Hurting:
                    EditorStyles.label.normal.textColor = _propColHurting.colorValue;
                    groundedStatus = new GUIContent("Hurting");
                    break;

                case Hitbox.AttackStatus.Cooldown:
                    EditorStyles.label.normal.textColor = _propColCooldown.colorValue;
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
            
            #region Visualization ++
            _propUseVisualizer.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(_propUseVisualizer.boolValue, _propUseVisualizer.boolValue ? "Visualization Enabled" : "Enable Visualization");
            if (_propUseVisualizer.boolValue) {
                EditorGUILayout.PropertyField(_propColIdle      );
                EditorGUILayout.PropertyField(_propColWindup    );
                EditorGUILayout.PropertyField(_propColHurting   );
                EditorGUILayout.PropertyField(_propColCooldown  );
                EditorGUILayout.Slider(_propVizOpacityEmpty, 0, 1);
                EditorGUILayout.Slider(_propVizOpacityHasOpp, 0, 1);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion -------------
            
            serializedObject.ApplyModifiedProperties();
            
        }

    }

}