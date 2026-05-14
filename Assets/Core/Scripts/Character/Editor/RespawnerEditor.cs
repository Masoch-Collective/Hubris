using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Character.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Respawner))]
    public class RespawnerEditor : UnityEditor.Editor {

        public Respawner Respawner {
            get {
                if (_respawner == null)
                    _respawner = (Respawner)target;
                return _respawner;
            }
        }
        [NonSerialized] private Respawner _respawner;
        private SerializedProperty _propMode;
        private SerializedProperty _propMinRespawnTime;
        private SerializedProperty _propRespawnTimeout;
        
        private void OnEnable() {
            _propMode = serializedObject.FindProperty("mode");
            _propMinRespawnTime = serializedObject.FindProperty("minRespawnTime");
            _propRespawnTimeout = serializedObject.FindProperty("respawnTimeout");
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.PropertyField(_propMode);
            switch ((Respawner.RespawnModes)_propMode.enumValueIndex) {

                case Respawner.RespawnModes.OnInputActionPerformed:
                    if (Respawner.RespawnAction == null)
                        EditorGUILayout.LabelField("No InputAction Set!");
                    else
                        EditorGUILayout.LabelField("InputAction: " + Respawner.RespawnAction.name);
                    EditorGUILayout.PropertyField(_propMinRespawnTime);
                    break;

                case Respawner.RespawnModes.Timed:
                    EditorGUILayout.PropertyField(_propRespawnTimeout);
                    break;

                case Respawner.RespawnModes.TimedWithInterruption:
                    if (Respawner.RespawnAction == null)
                        EditorGUILayout.LabelField("No InputAction Set!");
                    else
                        EditorGUILayout.LabelField("InputAction: " + Respawner.RespawnAction.name);
                    float min = _propMinRespawnTime.floatValue;
                    float max = _propRespawnTimeout.floatValue;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.MinMaxSlider(ref min, ref max, 0, max);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.PropertyField(_propMinRespawnTime);
                    EditorGUILayout.PropertyField(_propRespawnTimeout);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            serializedObject.ApplyModifiedProperties();
        }

    }

}