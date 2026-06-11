using UnityEngine;

namespace Utils {

    public class JuiceInvoke : MonoBehaviour {

        public void Shake(float amount) => Juice.Instance.AddShake(amount);
        public void HitFreeze() => Juice.Instance.InvokeHitFreeze();
        public void HitFreeze(float duration) => Juice.Instance.InvokeHitFreeze(duration);

    }

}