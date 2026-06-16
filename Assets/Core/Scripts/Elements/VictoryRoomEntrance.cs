using System;
using Character;
using Systems;
using UnityEngine;

namespace Elements {

    public class VictoryRoomEntrance : MonoBehaviour {

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.GetComponent<CharacterCore>())
                CombatLoopManager.Instance.hideGodRays = true;
        }

    }

}