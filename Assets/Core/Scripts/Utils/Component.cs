using UnityEngine;

namespace Utils {
    
    /// <summary>
    /// Wrapper for Unity Components. Will automatically get the attached component of type <typeparamref name="T"/>, and cache its value.
    /// </summary>
    /// <typeparam name="T">The type of Unity Component to handle.</typeparam>
    public abstract class Component<T> : Component {

        private T _value;
        public T Value {
            get {
                _value ??= GetComponent<T>();
                if (_value == null)
                    throw new MissingComponentException($"No {nameof(T)} was attached to {name}.");
                return _value;
            }
        }

        public static implicit operator T (Component<T> component) {
            return component.Value;
        }

    }

}