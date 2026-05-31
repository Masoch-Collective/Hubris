using System;
using Systems;
using UnityEditor;

namespace Character.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(RespawnSystem))]
    public class RespawnSystemEditor : UnityEditor.Editor {

        public RespawnSystem RespawnSystem {
            get {
                if (_respawnSystem == null)
                    _respawnSystem = (RespawnSystem)target;
                return _respawnSystem;
            }
        }
        [NonSerialized] private RespawnSystem _respawnSystem;
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
            switch ((RespawnSystem.RespawnModes)_propMode.enumValueIndex) {

                case RespawnSystem.RespawnModes.OnInputActionPerformed:
                    if (RespawnSystem.RespawnAction == null)
                        EditorGUILayout.LabelField("No InputAction Set!");
                    else
                        EditorGUILayout.LabelField("InputAction: " + RespawnSystem.RespawnAction.name);
                    EditorGUILayout.PropertyField(_propMinRespawnTime);
                    break;

                case RespawnSystem.RespawnModes.Timed:
                    EditorGUILayout.PropertyField(_propRespawnTimeout);
                    break;

                case RespawnSystem.RespawnModes.TimedWithInterruption:
                    if (RespawnSystem.RespawnAction == null)
                        EditorGUILayout.LabelField("No InputAction Set!");
                    else
                        EditorGUILayout.LabelField("InputAction: " + RespawnSystem.RespawnAction.name);
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