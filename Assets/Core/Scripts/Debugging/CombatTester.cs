using System;
using Character;
using Systems;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Debugging {

    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(TextMeshPro))]
    public class CombatTester : MonoBehaviour, IDamageable {
        public Collider2D Hurtbox {
            get {
                if (_hurtbox == null)
                    _hurtbox = GetComponent<Collider2D>();
                return _hurtbox;
            }
        }
        [NonSerialized] private Collider2D _hurtbox;
        public TextMeshPro Label {
            get {
                if (_label == null)
                    _label = GetComponent<TextMeshPro>();
                return _label;
            }
        }
        [NonSerialized] private TextMeshPro _label;

        public float fadeSpeed;

        private void Update() {
            Color col = Label.color;
            col.a = Mathf.Lerp(col.a, 0, Time.deltaTime * fadeSpeed);
            Label.color = col;
        }

        public void ReceiveDamage(Object attacker, int type) {
            Color col = Label.color;
            col.a = 1;
            Label.color = col;
            Debug.Log($"Attack from {attacker} landed on tester {name}", this);
            Label.text = ($"Attack landed at {Time.time}\nAttacked by: {attacker.name}\nAttack type: {type}");
        }

    }

}