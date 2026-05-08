using UnityEditor;
using UnityEngine;

namespace Utils.Editor {
    
    // [CustomPropertyDrawer(typeof())]
    
    public abstract class PropertyDrawerUtil : PropertyDrawer {

        private int _lines = 2;

        protected float GetPropertyHeight(int lines) {
            _lines = lines;
            return (EditorGUIUtility.singleLineHeight * _lines) +
                   (EditorGUIUtility.standardVerticalSpacing * (_lines - 1));
        }
        
        protected static SerializedProperty GetProperty(SerializedProperty property, string name) {
            return property.FindPropertyRelative(name);
        }

        /// <summary>
        /// Used to calculate the vertical position of a rect by line number
        /// </summary>
        /// <param name="line">Number of lines</param>
        /// <returns>Vertical position in pixels</returns>
        protected static float LineVerticalPosition(int line) {
            return (EditorGUIUtility.singleLineHeight * line) +
                   (EditorGUIUtility.standardVerticalSpacing * line);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(position, property, label);
            
            EditorGUI.EndProperty();
        }

    }

}