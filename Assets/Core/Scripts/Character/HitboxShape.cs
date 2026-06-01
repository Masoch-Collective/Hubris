using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Character {
    
    [Serializable]
    public class HitboxShape : ScriptableObject {
        
        [field: SerializeField]
        public Vector2[] Points { get; private set; }

        public void SetPoints(Vector2[] points) {
            Points = new Vector2[points.Length];
            points.CopyTo(Points, 0);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
            Debug.Log($"Writing {points.Length} point(s) to {name}.");
        }

        public void Clear() => Points = null;

    }

}