using System;
using UnityEngine;

namespace Utils {

    public abstract class Singleton<T> : MonoBehaviour where T : UnityEngine.Object {

        private static T _instance;

        public static T Instance {
            get {
                if (_instance == null) // If no instance exists, instantiate one
                    try {
                        string path = $"Singletons/{typeof(T)}";
                        GameObject prefab = (GameObject)Resources.Load(path);
                        if (prefab == null)
                            throw new Exception($"Resource \"{path}\" could not be loaded.");
                        T component;
                        if ((component = prefab.GetComponent<T>()) == null)
                            throw new Exception($"The {typeof(T)} singleton prefab did not have a {typeof(T)} component attached to the root GameObject.");
                        Debug.Log($"No instance of {typeof(T)} exists; instantiating one now.");
                        _instance = Instantiate(component);
                    }
                    catch (Exception e) {
                        Debug.LogError($"Error instantiating Singleton<{typeof(T)}>.\n{e}");
                    }
                return _instance;
            }
        }

    }

}