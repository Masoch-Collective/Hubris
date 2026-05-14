using UnityEngine;

namespace Systems {

    public interface IDamageable {
        
        
        public Collider2D Hurtbox { get; }
        public void Damage(Object attacker);

    }

}