using UnityEditor;
using UnityEngine;

namespace Utils.Editor {

    [CustomPropertyDrawer(typeof(PixelPerfectFloat))]
    public class PixelPerfectFloatDrawer : PropertyDrawerUtil {

        private const float DropdownWidth = 64;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return GetPropertyHeight(1);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            EditorGUI.BeginProperty(position, label, property);
            
            var mode = property.FindPropertyRelative("mode");
            var pixels = property.FindPropertyRelative("pixels");
            var value = property.FindPropertyRelative(nameof(PixelPerfectFloat.value));

            // Draw the pixels or value field according to mode, using the full width minus the width reserved for the mode dropdown
            Rect fieldRect = position;
            fieldRect.width -= DropdownWidth;
            EditorGUI.PropertyField(fieldRect, (WorldValueModes)mode.enumValueIndex == WorldValueModes.Pixels ? pixels : value, label);
            
            // If in pixel mode, set value to calculated pixel value
            if ((WorldValueModes)mode.enumValueIndex == WorldValueModes.Pixels)
                value.floatValue = (float)pixels.intValue / PixelPerfectFloat.PixelsPerUnit;
            else
                pixels.intValue = Mathf.RoundToInt(value.floatValue * PixelPerfectFloat.PixelsPerUnit);
            
            // Move the rect to the right of the previous field, set the width to the remaining space, then draw the dropdown
            fieldRect.position += Vector2.right * (fieldRect.width + EditorGUIUtility.standardVerticalSpacing);
            fieldRect.width = DropdownWidth - EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(fieldRect, mode, GUIContent.none);

            EditorGUI.EndProperty();
            
        }

    }

}