using System;
using UnityEngine;

namespace Utils {

    public abstract class Singleton<T> : MonoBehaviour where T : UnityEngine.Object {

        public static T Prefab {
            get {
                string path = $"Singletons/{typeof(T)}";
                GameObject prefab = (GameObject)Resources.Load(path);
                if (prefab == null)
                    throw new Exception($"Resource \"{path}\" could not be loaded.");
                T component;
                if ((component = prefab.GetComponent<T>()) == null)
                    throw new Exception($"The {typeof(T)} singleton prefab did not have a {typeof(T)} component attached to the root GameObject.");

                return component;
            }
        }

        private static T _instance;

        public static T Instance {
            get {
                if (_instance == null) // If no instance exists, try to find one in the scene (useful for singletons that need to be placed manually in the scene instead of instantiated at runtime)
                    _instance = FindAnyObjectByType<T>();
                if (_instance == null) // If no instance could be found in the scene, instantiate one (used for managers and the like, which are often just a GameObject with a single component)
                    try {
                        Debug.Log($"No instance of {typeof(T)} exists; instantiating one now.");
                        _instance = Instantiate(Prefab);
                    }
                    catch (Exception e) {
                        Debug.LogError($"Error instantiating Singleton<{typeof(T)}>.\n{e}");
                    }
                return _instance;
            }
        }

    }

}