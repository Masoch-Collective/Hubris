using UnityEngine;

namespace Utils {

    public class Instantiator : MonoBehaviour {
        
        [SerializeField] private bool thisLocation;
        [SerializeField] private bool thisRotation;
        [SerializeField] private bool child;
        [SerializeField] private bool sibling;

        public void Instantiate(GameObject prefab) {
            Instantiate(
                prefab,
                thisLocation ? transform.position : prefab.transform.position,
                thisRotation ? transform.rotation : prefab.transform.rotation,
                child ? transform : sibling ? transform.parent : null
            );
        }

    }

}