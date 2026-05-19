using System;
using UnityEngine;

namespace Character {

    [RequireComponent(typeof(CharacterCore))]
    public class CharacterComponent : MonoBehaviour {

        [NonSerialized]
        private CharacterCore _core;
        public CharacterCore Core {
            get {
                if (_core == null)
                    _core = GetComponent<CharacterCore>();
                return _core;
            }
        }

        public void Start() {
            Core.OnDeath += OnDeath;
        }

        protected virtual void OnDeath() { }

    }

}