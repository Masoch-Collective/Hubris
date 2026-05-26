using Character;
using UnityEngine;

namespace Systems {

    public interface IDamageable {
        
        
        public Collider2D Hurtbox { get; }
        public CharacterCore.ActionType ReceiveDamage(CharacterCore attacker, CharacterCore.ActionType type);

    }

}