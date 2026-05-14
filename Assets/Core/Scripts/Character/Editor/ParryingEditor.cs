using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Character.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Parrying))]
    public class ParryingEditor : UnityEditor.Editor {
        public Parrying Parrying {
            get {
                if (_parrying == null)
                    _parrying = (Parrying)target;
                return _parrying;
            }
        }
        [NonSerialized] private Parrying _parrying;
        
        private SerializedProperty _propAnimator;
        private SerializedProperty _propAnimationTriggerHash;
        private SerializedProperty _propUseAnimationEvents;
        private SerializedProperty _propWindupDuration;
        private SerializedProperty _propParryDuration;
        private SerializedProperty _propCooldownDuration;
        
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
            _propAnimator = GetSerializedProperty("animator");
            _propAnimationTriggerHash = GetSerializedProperty("animationTriggerHash");
            _propUseAnimationEvents = GetSerializedProperty("useAnimationEvents");
            _propWindupDuration = GetSerializedProperty("windupDuration");
            _propParryDuration = GetSerializedProperty("parryDuration");
            _propCooldownDuration = GetSerializedProperty("cooldownDuration");
        }

        public override void OnInspectorGUI() {

            #region Config +++++++++
            
            EditorGUILayout.PropertyField(_propUseAnimationEvents, new GUIContent("Animated"));
            if (_propUseAnimationEvents.boolValue) {
                EditorGUILayout.PropertyField(_propAnimator);
                if (!_propAnimator.objectReferenceValue)
                    EditorGUILayout.HelpBox("Animated mode requires an Animator component to trigger on attack.", MessageType.Error, false);
                else
                    _propAnimationTriggerHash.intValue = Animator.StringToHash(EditorGUILayout.TextField("Trigger", _animatorTrigger));
                EditorGUILayout.HelpBox("Make sure your animation has an Animation Event that calls ParryEnd at the end.\n" +
                                        "Additionally, add ActiveStart and ActiveCooldown (or ActiveForSeconds) Animation Events to specify when the parry should be active.", MessageType.Info, true);
                
            } else {
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
                
                _totalDuration = _propWindupDuration.floatValue + _propParryDuration.floatValue + _propCooldownDuration.floatValue;
                _hurtEnd = _propWindupDuration.floatValue + _propParryDuration.floatValue;
                float windup = _propWindupDuration.floatValue;
                
                EditorGUILayout.MinMaxSlider(ref windup, ref _hurtEnd, 0, _totalDuration);

                _propWindupDuration.floatValue = windup;
                _propParryDuration.floatValue = _hurtEnd - windup;
                _propCooldownDuration.floatValue = _totalDuration - _hurtEnd;
                
                EditorGUILayout.PropertyField(_propWindupDuration);
                EditorGUILayout.PropertyField(_propParryDuration);
                EditorGUILayout.PropertyField(_propCooldownDuration);
                
            }
            #endregion -------------
            
            EditorGUILayout.Space();
            
            #region Status +++++++++
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            GUIContent groundedStatus = null;
            Color defaultTextCol = EditorStyles.label.normal.textColor;
            bool err = false;
            switch (Parrying.Status) {

                case Hitbox.AttackStatus.Idle:
                    EditorStyles.label.normal.textColor = Parrying.Core.Hitbox.colIdle;
                    groundedStatus = new GUIContent("Idle");
                    break;

                case Hitbox.AttackStatus.Windup:
                    EditorStyles.label.normal.textColor = Parrying.Core.Hitbox.colWindup;
                    groundedStatus = new GUIContent("Winding Up");
                    break;

                case Hitbox.AttackStatus.Active:
                    EditorStyles.label.normal.textColor = Parrying.Core.Hitbox.colActive;
                    groundedStatus = new GUIContent("Active");
                    break;

                case Hitbox.AttackStatus.Cooldown:
                    EditorStyles.label.normal.textColor = Parrying.Core.Hitbox.colCooldown;
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
            
            serializedObject.ApplyModifiedProperties();
            
        }

    }

}