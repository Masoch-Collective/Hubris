using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Character.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Hitbox))]
    public class HitboxEditor : UnityEditor.Editor {

        public const string UIStringNewShape = "New Shape";
        public const string UIStringLoad = "(Re)load Shape";
        public const string UIStringSave = "Save";

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
        private SerializedProperty _propColActive;
        private SerializedProperty _propColCooldown;
        private SerializedProperty _propOpponentLayerMask;
        private SerializedProperty _propShapeUpwards;
        private SerializedProperty _propShapeDownwards;
        private SerializedProperty _propUseAnimationEvents;
        private SerializedProperty _propUseVisualizer;
        private SerializedProperty _propWindupDuration;
        private SerializedProperty _propHurtDuration;
        private SerializedProperty _propCooldownDuration;
        private SerializedProperty _propVizOpacityEmpty;
        private SerializedProperty _propVizOpacityHasOpp;

        private readonly Vector2[] _emptyPoints = { Vector2.zero, Vector2.zero, Vector2.zero };
        private HitboxShape _loadedShape;
        private bool _shapeEditorFoldout;
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
            _propColActive = GetSerializedProperty("colActive");
            _propColCooldown = GetSerializedProperty("colCooldown");
            _propOpponentLayerMask = GetSerializedProperty("opponentLayerMask");
            _propShapeUpwards = GetSerializedProperty("shapeUpwards");
            _propShapeDownwards = GetSerializedProperty("shapeDownwards");
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

            // Opponent layer mask
            EditorGUILayout.PropertyField(_propOpponentLayerMask);
            Hitbox.Collider.excludeLayers = ~_propOpponentLayerMask.intValue;

            // Opponent(s) in hitbox
            EditorGUILayout.LabelField($"IDamageable Entered: {Hitbox.InHitbox.Count}");
            EditorGUILayout.LabelField($"IDamageable Damaged: {Hitbox.AlreadyDamaged.Count}");

            // Animated toggle
            EditorGUILayout.PropertyField(_propUseAnimationEvents, new GUIContent("Animated"));

            if (_propUseAnimationEvents.boolValue) {

                if (!Hitbox.Core.Animator)
                    EditorGUILayout.HelpBox("Animated mode requires an Animator component to be attached to this GameObject.",
                        MessageType.Error, false);
                else
                    EditorGUILayout.HelpBox("Configure Animator parameters from CharacterCore component.",
                        MessageType.Info, false);

                EditorGUILayout.HelpBox(
                    "Make sure your animation has an Animation Event that calls AttackEnd at the end.\n" +
                    "Additionally, add AttackActive and ActiveCooldown Animation Events to specify when the hitbox should be active.",
                    MessageType.Info, true);

            } else {
                
                EditorGUILayout.Space();

                // Timing config
                EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);

                _totalDuration = _propWindupDuration.floatValue + _propHurtDuration.floatValue +
                                 _propCooldownDuration.floatValue;

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
            EditorGUILayout.Space();

            // Shapes config
            // Upwards shape cluster
            EditorGUILayout.LabelField("HitboxShapes", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_propShapeUpwards, new GUIContent("Editing:"));
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            if (_propShapeUpwards.objectReferenceValue == null) {
                if (GUILayout.Button(UIStringNewShape))
                    _propShapeUpwards.objectReferenceValue =
                        SaveShape(CreateShape("UpwardsHitbox"), Hitbox.Collider);
            } else {
                if (GUILayout.Button("Edit")) {
                    _loadedShape = (HitboxShape)_propShapeUpwards.objectReferenceValue;
                    LoadShape(_loadedShape, Hitbox.Collider);
                    _shapeEditorFoldout = true;
                }
            }
            EditorGUI.EndDisabledGroup();
            // Downwards shape cluster
            EditorGUILayout.PropertyField(_propShapeDownwards);
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            if (_propShapeDownwards.objectReferenceValue == null){
                if (GUILayout.Button(UIStringNewShape))
                    _propShapeDownwards.objectReferenceValue =
                        SaveShape(CreateShape("DownwardsHitbox"), Hitbox.Collider);
            } else {
                if (GUILayout.Button("Edit")) {
                    _loadedShape = (HitboxShape)_propShapeDownwards.objectReferenceValue;
                    LoadShape(_loadedShape, Hitbox.Collider);
                    _shapeEditorFoldout = true;
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            // Editor
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            _shapeEditorFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_shapeEditorFoldout, "HitboxShape Editor");
            if (_shapeEditorFoldout) {
                _loadedShape = (HitboxShape)EditorGUILayout.ObjectField("Editing:", _loadedShape, typeof(HitboxShape), false);
                EditorGUI.BeginDisabledGroup(_loadedShape == null);
                if (GUILayout.Button(UIStringLoad))
                    LoadShape(_loadedShape, Hitbox.Collider);
                if (GUILayout.Button(UIStringSave))
                    SaveShape(_loadedShape, Hitbox.Collider);
                EditorGUILayout.HelpBox($"Use the PolygonCollider2D component to configure the hitbox to your liking, then press {UIStringSave} to save the shape to {_loadedShape.name}.", MessageType.Info);
                EditorGUI.EndDisabledGroup();
            } else {
                _loadedShape = null;
            }
            if (_loadedShape == null && !Application.isPlaying)
                Hitbox.Collider.points = _emptyPoints;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();

            #endregion -------------
            
            EditorGUILayout.Space();
            
            #region Status +++++++++
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            GUIContent groundedStatus = null;
            Color defaultTextCol = EditorStyles.label.normal.textColor;
            bool err = false;
            switch (Hitbox.Stage) {

                case CharacterCore.ActionStage.Idle:
                    EditorStyles.label.normal.textColor = _propColIdle.colorValue;
                    groundedStatus = new GUIContent("Idle");
                    break;

                case CharacterCore.ActionStage.Windup:
                    EditorStyles.label.normal.textColor = _propColWindup.colorValue;
                    groundedStatus = new GUIContent("Winding Up");
                    break;

                case CharacterCore.ActionStage.Active:
                    EditorStyles.label.normal.textColor = _propColActive.colorValue;
                    groundedStatus = new GUIContent("Active");
                    break;

                case CharacterCore.ActionStage.Cooldown:
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
            _propUseVisualizer.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(_propUseVisualizer.boolValue, "Visualization Config");
            if (_propUseVisualizer.boolValue) {
                EditorGUILayout.PropertyField(_propColIdle      );
                EditorGUILayout.PropertyField(_propColWindup    );
                EditorGUILayout.PropertyField(_propColActive   );
                EditorGUILayout.PropertyField(_propColCooldown  );
                EditorGUILayout.Slider(_propVizOpacityEmpty, 0, 1);
                EditorGUILayout.Slider(_propVizOpacityHasOpp, 0, 1);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion -------------
            
            serializedObject.ApplyModifiedProperties();
            
        }

        public static HitboxShape LoadShape(HitboxShape shape, PolygonCollider2D collider2D) {
            collider2D.SetPath(0, shape.Points);
            return shape;
        }
        public static HitboxShape SaveShape(HitboxShape shape, PolygonCollider2D collider2D) {
            shape.SetPoints(collider2D.points);
            AssetDatabase.SaveAssetIfDirty(shape);
            Debug.Log($"Saved {AssetDatabase.GetAssetPath(shape)}");
            return shape;
        }

        public static HitboxShape CreateShape(string defaultName = "New Hitbox") {
            HitboxShape shape = CreateInstance<HitboxShape>();
            string extension = "asset";
            string path = EditorUtility.SaveFilePanelInProject("Create HitboxShape", defaultName, extension, "", "Assets/Core/Settings");
            if (string.IsNullOrEmpty(path)) {
                Debug.Log("HitboxShape creation cancelled.");
                return null;
            }
            if (!path.EndsWith(extension)) {
                EditorUtility.DisplayDialog("Invalid Extension", $"File extension must be \"{extension}\".", "Ok");
                return CreateShape(defaultName);
            }
            Debug.Log($"Creating new HitboxShape at {path}.");
            AssetDatabase.CreateAsset(shape, path);
            AssetDatabase.SaveAssetIfDirty(shape);
            return shape;
        }

    }

}