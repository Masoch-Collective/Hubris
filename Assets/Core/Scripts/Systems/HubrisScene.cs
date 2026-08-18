using System.Linq;
using UnityEngine;

namespace Systems {

    [CreateAssetMenu(menuName = "Scene/HubrisScene Wrapper")]
    public class HubrisScene : ScriptableObject {
            
        public string SceneName => sceneAssetPath.Split('/').Last().Replace(".unity", "");
        public string sceneAssetPath;
        public int pixelsPerUnit;

    }

}