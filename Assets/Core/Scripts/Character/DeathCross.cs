using System;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Character {

    public class DeathCross : MonoBehaviour, IDamageable {

        public static DeathCross Prefab(int playerNumber) {
            if (!Prefabs.ContainsKey(playerNumber)) {
                string path = $"Prefabs/{typeof(DeathCross)}_P{playerNumber}";
                GameObject prefab = (GameObject)Resources.Load(path);
                DeathCross component;
                if (prefab == null)
                    throw new Exception($"Resource \"{path}\" could not be loaded.");
                if ((component = prefab.GetComponent<DeathCross>()) == null)
                    throw new Exception(
                        $"The DeathCross prefab did not have a DeathCross component attached to the root GameObject.");
                Prefabs.Add(playerNumber, component);
            }
            return Prefabs[playerNumber];
        }

        [NonSerialized] private static readonly Dictionary<int, DeathCross> Prefabs = new();
        public Collider2D Hurtbox {
            get {
                if (_hurtbox == null)
                    _hurtbox = GetComponent<Collider2D>();
                return _hurtbox;
            }
        }
        [NonSerialized] private Collider2D _hurtbox;
        public Rigidbody2D Rigidbody {
            get {
                if (_rigidbody == null)
                    _rigidbody = GetComponent<Rigidbody2D>();
                return _rigidbody;
            }
        }
        [NonSerialized] private Rigidbody2D _rigidbody;

        public float randomizeStartAngle;
        public float rotationalForce;
        public float rotationalForceOnHit;
        public Vector2 linearForce;
        public float linearForceOnHit;
        [Tooltip("How long to wait after spawning to register hits (prevents the attack that killed the player from immediately also hitting the cross.)")]
        public float hitGracePeriod;
        public UnityEvent onHit;
        public UnityEvent onLodged;
        public UnityEvent onLodgedHit;

        private float _spawnTime;
        private bool _wasHit;
        
        public CharacterCore.InteractionType ReceiveDamage(CharacterCore attacker) {
            if (Time.time < _spawnTime + hitGracePeriod)
                return CharacterCore.InteractionType.Whiffed;
            float dir = Mathf.Sign(transform.position.x - attacker.transform.position.x);
            Rigidbody.angularVelocity = rotationalForceOnHit * dir;
            Rigidbody.linearVelocity = (transform.position - attacker.transform.position).normalized * linearForceOnHit;
            _wasHit = true;
            onHit.Invoke();
            return CharacterCore.InteractionType.Attacked;
        }

        private void OnCollisionEnter2D(Collision2D other) {
            if (!other.collider.CompareTag("LevelGeometry"))
                return;
            Destroy(_rigidbody);
            Destroy(this);
            if (_wasHit)
                onLodgedHit.Invoke();
            else
                onLodged.Invoke();
        }

        public static DeathCross NewCross(int playerNumber, Vector3 position, float direction, Transform parent, params Collider2D[] ignore) {
            direction = Mathf.Sign(direction);
            DeathCross newInstance = Instantiate(Prefab(playerNumber), position, Prefab(playerNumber).transform.rotation, parent);
            newInstance.transform.Rotate(Vector3.forward * Random.Range(-newInstance.randomizeStartAngle, newInstance.randomizeStartAngle));
            newInstance.Rigidbody.angularVelocity = newInstance.rotationalForce * direction;
            newInstance.linearForce.x *= direction;
            newInstance.Rigidbody.linearVelocity = newInstance.linearForce;
            newInstance._spawnTime = Time.time;
            foreach (Collider2D coll in ignore)
                Physics2D.IgnoreCollision(((IDamageable)newInstance).Hurtbox, coll);
            return newInstance;
        }

    }

}