using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Character.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Hitbox))]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class HitboxEditor : UnityEditor.Editor {
        private const string PrefsKeyDebugging = "HitboxDebugging";
        private const string PrefsKeyHitboxDebuggingColPrefix = "HitboxDebuggingCol";

        private static List<PolygonCollider2DVisualizer> _visualisers;
        
        public PolygonCollider2DVisualizer DebugVisualizer {
            get {
                if (_debugVisualizer == null) {
                    if (Hitbox == null || (_debugVisualizer = Hitbox.GetComponent<PolygonCollider2DVisualizer>()) == null)
                        return null;
                    _debugVisualizer.Collider.isTrigger = true;
                    Hitbox.StatusChanged.AddListener(UpdateStatus);
                }
                return _debugVisualizer;
            }
        }
        [NonSerialized] private PolygonCollider2DVisualizer _debugVisualizer;
        public Hitbox Hitbox {
            get {
                if (_hitbox == null)
                    try {
                        _hitbox = (Hitbox)target;
                    }
                    catch (Exception e) {
                        if (target == null)
                            Debug.LogError($"Hitbox editor target was null.");
                        else 
                            Debug.LogError($"Hitbox editor could not parse \"target\" {target.name} of type {target.GetType()}.", target);
                    }
                return _hitbox;
            }
        }
        [NonSerialized] private Hitbox _hitbox;
        
        private void UpdateStatus(Hitbox.AttackStatus status, Hitbox.AttackType _ = Hitbox.AttackType.Upwards) {
            if (!DebugVisualizer)
                return;
            Color col = status switch {
                Hitbox.AttackStatus.Idle => ColIdle,
                Hitbox.AttackStatus.Windup => ColWindup,
                Hitbox.AttackStatus.Hurting => ColHurting,
                Hitbox.AttackStatus.Cooldown => ColCooldown,
                _ => Color.black
            };
            DebugVisualizer.outlineColor = col;
            col.a = _opacity;
            DebugVisualizer.fillColor = col;
        }

        /// <summary>
        /// Utility to load a colour from EditorPrefs. Returns <paramref name="def"/> if prefs string is not valid colour.
        /// </summary>
        /// <param name="name">Name to append to "HitboxDebuggingCol"; used as preference key.</param>
        /// <param name="def">Default colour</param>
        /// <returns>Colour from EditorPrefs, or <paramref name="def"/> if the former is invalid.</returns>
        private static Color GetHitboxDebuggingColor(string name, Color def) {
            string prefsValue = EditorPrefs.GetString(PrefsKeyHitboxDebuggingColPrefix+name);
            if (ColorUtility.TryParseHtmlString(prefsValue, out var col))
                return col;
            // Debug.LogWarning($"No valid colour for {name} hitbox debugging found, defaulting to {def}.");
            return def;
        }
        private static void SaveColorPrefs() {
            EditorPrefs.SetString(PrefsKeyHitboxDebuggingColPrefix+"Idle", ColorUtility.ToHtmlStringRGBA(_colIdle));
            EditorPrefs.SetString(PrefsKeyHitboxDebuggingColPrefix+"Windup", ColorUtility.ToHtmlStringRGBA(_colWindup));
            EditorPrefs.SetString(PrefsKeyHitboxDebuggingColPrefix+"Hurting", ColorUtility.ToHtmlStringRGBA(_colHurting));
            EditorPrefs.SetString(PrefsKeyHitboxDebuggingColPrefix+"Cooldown", ColorUtility.ToHtmlStringRGBA(_colCooldown));
        }
        
        public static Color ColIdle =>     _colIdle ==     default ? _colIdle =        GetHitboxDebuggingColor("Idle",     Color.gray6) :          _colIdle;
        [NonSerialized]private static Color _colIdle;
        public static Color ColWindup =>   _colWindup ==   default ? _colWindup =      GetHitboxDebuggingColor("Windup",   Color.gold) :           _colWindup;
        [NonSerialized]private static Color _colWindup;
        public static Color ColHurting =>  _colHurting ==  default ? _colHurting =     GetHitboxDebuggingColor("Hurting",  Color.deepPink) :       _colHurting;
        [NonSerialized]private static Color _colHurting;
        public static Color ColCooldown => _colCooldown == default ? _colCooldown =    GetHitboxDebuggingColor("Cooldown", Color.deepSkyBlue) :    _colCooldown;
        [NonSerialized]private static Color _colCooldown;
        private static float _opacity = 0.2f;
        
        private static bool _debug = true;
        private static bool _debugDelta;
        private string _animatorTrigger;
        private float _duration;
        private float _hurt;
        private static event Action UpdateVisualizer;
        
        private void OnEnable() {
            _debug = EditorPrefs.GetBool(PrefsKeyDebugging, true);
            UpdateVisualizer += () => {
                if (DebugVisualizer)
                    DebugVisualizer.enabled = _debug;
                UpdateStatus(Hitbox ? Hitbox.Status : Hitbox.AttackStatus.Idle);
            };
            UpdateVisualizer?.Invoke();
            _duration = Hitbox.windup + Hitbox.hurtDuration + Hitbox.cooldown;
            _hurt = Hitbox.windup + Hitbox.hurtDuration;
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
                
                _duration = Hitbox.windup + Hitbox.hurtDuration + Hitbox.cooldown;
                _hurt = Hitbox.windup + Hitbox.hurtDuration;
                
                EditorGUILayout.MinMaxSlider(ref Hitbox.windup, ref _hurt, 0, _duration);
                
                Hitbox.hurtDuration = _hurt - Hitbox.windup;
                Hitbox.cooldown = _duration - _hurt;
                
                Hitbox.windup = Mathf.Max(EditorGUILayout.FloatField("Windup", Hitbox.windup), 0);
                Hitbox.hurtDuration = Mathf.Max(EditorGUILayout.FloatField("Hurt Duration", Hitbox.hurtDuration), 0);
                Hitbox.cooldown = Mathf.Max(EditorGUILayout.FloatField("Cooldown", Hitbox.cooldown), 0);
                
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
                    EditorStyles.label.normal.textColor = ColIdle;
                    groundedStatus = new GUIContent("Idle");
                    break;

                case Hitbox.AttackStatus.Windup:
                    EditorStyles.label.normal.textColor = ColWindup;
                    groundedStatus = new GUIContent("Winding Up");
                    break;

                case Hitbox.AttackStatus.Hurting:
                    EditorStyles.label.normal.textColor = ColHurting;
                    groundedStatus = new GUIContent("Hurting");
                    break;

                case Hitbox.AttackStatus.Cooldown:
                    EditorStyles.label.normal.textColor = ColCooldown;
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
            _debug = EditorGUILayout.BeginFoldoutHeaderGroup(_debug, _debug ? "Debugging Enabled" : "Enable Debugging");
            if (_debugDelta != _debug) {
                EditorPrefs.SetBool(PrefsKeyDebugging, _debug);
            }
            _debugDelta = _debug;
            if (_debug) {
                Color col =
                _colIdle        = EditorGUILayout.ColorField("Idle",     ColIdle       );
                _colWindup      = EditorGUILayout.ColorField("Windup",   ColWindup     );
                _colHurting     = EditorGUILayout.ColorField("Hurting",  ColHurting    );
                _colCooldown    = EditorGUILayout.ColorField("Cooldown", ColCooldown   );
                _opacity        = EditorGUILayout.Slider(_opacity, 0, 1);
                EditorGUI.BeginDisabledGroup(true);
                if (GUILayout.Button("Save Colour Preferences"))
                    SaveColorPrefs();
                EditorGUI.EndDisabledGroup();
            }
            #endregion -------------
            
            UpdateVisualizer?.Invoke();
            
            serializedObject.ApplyModifiedProperties();
            
        }

    }

}