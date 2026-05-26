using System;
using UnityEngine;

namespace Utils {

    public abstract class Singleton<T> : MonoBehaviour where T : UnityEngine.Object {

        private static T _instance;

        public static T Instance {
            get {
                if (_instance == null) // If no instance exists, instantiate one
                    try {
                        _instance = Instantiate(Resources.Load<T>($"Singletons/{nameof(T)}"));
                    }
                    catch (Exception e) {
                        Debug.LogError($"Error instantiating Singleton<{typeof(T)}>. Does the prefab \"Resources/Singletons/{typeof(T)}\" exist?\n{e}");
                    }
                return _instance;
            }
        }

    }

}