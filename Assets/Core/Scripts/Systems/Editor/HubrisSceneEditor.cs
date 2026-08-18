using System;
using UnityEditor;

namespace Systems.Editor {

    [CustomEditor(typeof(HubrisScene))]
    public class HubrisSceneEditor : UnityEditor.Editor {

        private SerializedProperty _propPath;
        private SerializedProperty _propScenePpu;
        private SceneAsset _sceneAsset;

        private void OnEnable() {
            _propPath = serializedObject.FindProperty(nameof(HubrisScene.sceneAssetPath));
            _propScenePpu = serializedObject.FindProperty(nameof(HubrisScene.pixelsPerUnit));
        }

        public override void OnInspectorGUI() {

            _sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(_propPath.stringValue);
            
            _sceneAsset = (SceneAsset)EditorGUILayout.ObjectField(_sceneAsset, typeof(SceneAsset), false);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_propPath);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(_propScenePpu);
            
            _propPath.stringValue = AssetDatabase.GetAssetPath(_sceneAsset);

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(target);

        }

    }

}