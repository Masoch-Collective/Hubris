using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Character.Editor {
    
    [CustomEditor(typeof(HitboxShape))]
    public class HitboxShapeEditor : UnityEditor.Editor {

        private const float AspectRatio = 1f;
        private const float DefaultScale = 0.2f;
        private const float DefaultOffsetX = 1f;
        private const float DefaultOffsetY = 2.5f;
        
        public Material Material {
            get {
                if (_material == null) {
                    _material = Resources.Load<Material>("Materials/Line");
                    _material.shader = Shader.Find("Hidden/Internal-Colored");
                }
                return _material;
            }
        }
        private Material _material;
        public HitboxShape HitboxShape {
            get {
                if (_hitboxShape == null)
                    _hitboxShape = (HitboxShape)target;
                return _hitboxShape;
            }
        }
        private HitboxShape _hitboxShape;

        private float _scale;
        private Vector2 _offset;
        private Color _backgroundColor = Color.gray1;
        private Color _lineColor = Color.greenYellow;

        private void OnEnable() {
            _offset = new Vector2(DefaultOffsetX, DefaultOffsetY);
            _scale = DefaultScale;
        }

        public override void OnInspectorGUI() {

            if (HitboxShape.Points == null || HitboxShape.Points.Length == 0) {
                EditorGUILayout.HelpBox($"Empty HitboxShape. Assign this HitboxShape to a Character Hitbox component, then press \"{HitboxEditor.UIStringSave}\" to save the PolygonCollider2D's shape to this HitboxShape.", MessageType.Error);
                return;
            }
            
            if (HitboxShape.Points.Length < 3) {
                EditorGUILayout.HelpBox($"Invalid HitboxShape. HitboxShapes must have at least three points.", MessageType.Error);
                if (GUILayout.Button("Clear"))
                    HitboxShape.Clear();
                return;
            }
            
            EditorGUILayout.LabelField($"Shape with {HitboxShape.Points.Length} points.");
            
            // Begin to draw a horizontal layout, using the helpBox EditorStyle
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Reserve GUI space with a width from 10 to 10000, and a fixed height of 200, and 
            // cache it as a rectangle.
            Rect layoutRectangle = GUILayoutUtility.GetAspectRect(AspectRatio);

            if (Event.current.type == EventType.Repaint) {
                // If we are currently in the Repaint event, begin to draw a clip of the size of 
                // previously reserved rectangle, and push the current matrix for drawing.
                GUI.BeginClip(layoutRectangle);
                GL.PushMatrix();

                // Clear the current render buffer, setting a new background colour, and set our
                // material for rendering.
                GL.Clear(true, false, _backgroundColor);
                Material.SetPass(0);

                // Start drawing in OpenGL Quads, to draw the background canvas. Set the
                // colour black as the current OpenGL drawing colour, and draw a quad covering
                // the dimensions of the layoutRectangle.
                GL.Begin(GL.QUADS);
                GL.Color(_backgroundColor);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(layoutRectangle.width, 0, 0);
                GL.Vertex3(layoutRectangle.width, layoutRectangle.height, 0);
                GL.Vertex3(0, layoutRectangle.height, 0);
                GL.End();

                // Start drawing in OpenGL Lines, to draw the lines of the grid.
                GL.Begin(GL.LINES);
                
                float scale = layoutRectangle.width * _scale;
                GL.Color(Color.red);
                GL.Vertex3(0, _offset.y * scale, 0);
                GL.Vertex3(layoutRectangle.width, _offset.y * scale, 0);
                GL.Color(Color.green);
                GL.Vertex3(_offset.x * scale, 0, 0);
                GL.Vertex3(_offset.x * scale, layoutRectangle.height, 0);
                GL.Color(_lineColor);
                for (int i = 0; i < HitboxShape.Points.Length; i++) {
                    Vector2 point = HitboxShape.Points[i];
                    Vector2 prevPoint = HitboxShape.Points[(i == 0 ? HitboxShape.Points.Length : i) - 1];
                    GL.Vertex3(
                        (prevPoint.x + _offset.x) * scale, 
                        (prevPoint.y - _offset.y) * -scale, 
                        0);
                    GL.Vertex3(
                        (point.x + _offset.x) * scale, 
                        (point.y - _offset.y) * -scale, 
                        0);
                }

                // End lines drawing.
                GL.End();

                // Pop the current matrix for rendering, and end the drawing clip.
                GL.PopMatrix();
                GUI.EndClip();
                
            }

            // End our horizontal 
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("View Settings", EditorStyles.boldLabel);
            _offset =   EditorGUILayout.Vector2Field(   "Offset", _offset);
            _scale =    EditorGUILayout.FloatField(     "Scale", _scale);
            _backgroundColor =  EditorGUILayout.ColorField("Background", _backgroundColor);
            _lineColor =        EditorGUILayout.ColorField("Line Colour", _lineColor);

        }

    }

}