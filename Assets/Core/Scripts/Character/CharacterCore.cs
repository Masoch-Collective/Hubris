using System;
using UnityEditor;
using UnityEngine;

namespace Character {
    
    [RequireComponent(typeof(Controller))]
    public class CharacterCore : MonoBehaviour {
        
        [NonSerialized]
        private Collider2D _hurtbox;
        public Collider2D Hurtbox {
            get {
                if (_hurtbox == null)
                    _hurtbox = GetComponent<Collider2D>();
                return _hurtbox;
            }
        }
        
    }

}