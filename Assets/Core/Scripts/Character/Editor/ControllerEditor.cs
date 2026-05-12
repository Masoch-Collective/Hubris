using System;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

namespace Character.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Controller))]
    public class ControllerEditor : UnityEditor.Editor {

        private Controller _controller;
        private SerializedProperty _grounded;
        private bool _expandCoyoteSection;

        private int _morbin;

        private void OnEnable() {
            _controller = (Controller)target;
        }

        public override void OnInspectorGUI() {

            DrawDefaultInspector();
            
            EditorGUILayout.Space();

            _expandCoyoteSection = EditorGUILayout.BeginFoldoutHeaderGroup(
                _expandCoyoteSection, 
                _expandCoyoteSection ? _morbin == 1 ? "It's Morbin' Time!" : "It's Coyote Time!" : "Coyote Time");

            if (_expandCoyoteSection) {

                if (_morbin == 0)
                    _morbin = Random.Range(1, 10);
                    
                _controller.coyoteTimeDuration = EditorGUILayout.IntField("Duration", _controller.coyoteTimeDuration);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);

                EditorGUI.BeginDisabledGroup(true);

                GUIContent groundedStatus;
                Color defaultTextCol = EditorStyles.label.normal.textColor;

                if (_controller.LastGroundedFrame == int.MinValue) {
                    EditorStyles.label.normal.textColor = Color.darkRed;
                    groundedStatus = new GUIContent(
                        "Takeoff",
                        "Grounded manually set to false.");
                } else if (_controller.FramesSinceLastGrounded == 0) {
                    EditorStyles.label.normal.textColor = Color.green;
                    groundedStatus = new GUIContent(
                        "Grounded",
                        "Currently touching the ground.");
                } else if (_controller.FramesSinceLastGrounded > 0 &&
                           _controller.FramesSinceLastGrounded <= _controller.coyoteTimeDuration) {
                    EditorStyles.label.normal.textColor = Color.orange;
                    groundedStatus = new GUIContent(
                        "Coyote Floating!",
                        "Last grounded time occurred within coyote time.");
                } else if (_controller.FramesSinceLastGrounded > _controller.coyoteTimeDuration) {
                    EditorStyles.label.normal.textColor = Color.red;
                    groundedStatus = new GUIContent(
                        "Coyote Falling!",
                        "Last grounded time occurred too long ago, coyote time no longer in effect.");
                } else {
                    EditorStyles.label.normal.textColor = Color.black;
                    groundedStatus = new GUIContent(
                        "Last grounded frame is in the future???",
                        $"Last grounded frame ({_controller.LastGroundedFrame}) is somehow greater than current time.");
                }

                EditorGUILayout.LabelField(groundedStatus);

                EditorStyles.label.normal.textColor = defaultTextCol;

                EditorGUILayout.IntSlider(
                    "Remaining Frames",
                    _controller.coyoteTimeDuration - _controller.FramesSinceLastGrounded,
                    0,
                    _controller.coyoteTimeDuration);

                EditorGUILayout.Toggle("Can Jump", _controller.CanJump);

                EditorGUILayout.TextField(
                    "Last Grounded",
                    _controller.LastGroundedFrame == int.MinValue ? "Never" : _controller.LastGroundedFrame.ToString());

                EditorGUI.EndDisabledGroup();

            } else
                _morbin = 0;

            EditorGUI.EndFoldoutHeaderGroup();
            
            serializedObject.ApplyModifiedProperties();
            
        }
        
    }

}