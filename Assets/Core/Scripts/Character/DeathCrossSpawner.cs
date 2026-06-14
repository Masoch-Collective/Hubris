using System;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Character {

    public class DeathCrossSpawner : MonoBehaviour {

        private CharacterCore Core {
            get {
                if (_core == null)
                    _core = GetComponent<CharacterCore>();
                return _core;
            }
        }
        [NonSerialized] private CharacterCore _core;
        
        public bool tester;

        private void Update() {
            if (tester && Keyboard.current.cKey.wasPressedThisFrame)
                SpawnDeathCross(Core);
        }

        public void SpawnDeathCross(CharacterCore attacker) {
            if (attacker == null)
                return;
            DeathCross.NewCross(transform.position, transform.position.x - attacker.transform.position.x, transform.parent, ((IDamageable)Core).Hurtbox, ((IDamageable)attacker).Hurtbox);
        }

    }

}