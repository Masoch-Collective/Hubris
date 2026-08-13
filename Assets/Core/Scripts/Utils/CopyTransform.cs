using System;
using UnityEngine;

namespace Utils {

    public class CopyTransform : MonoBehaviour {

        [SerializeField] private Space spaceTo;
        [SerializeField] private Space spaceFrom;
        [SerializeField] private Transform copyFrom;
        [SerializeField] private bool position;
        [SerializeField] private bool rotation;
        [SerializeField] private bool scale;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() { }

        // Update is called once per frame
        void Update() {
            if (!copyFrom)
                return;
            if (position)
                if (spaceTo == Space.Self)
                    transform.localPosition = spaceFrom switch {
                        Space.Self => copyFrom.localPosition,
                        Space.World => copyFrom.position,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                else
                    transform.position = spaceFrom switch {
                        Space.Self => copyFrom.localPosition,
                        Space.World => copyFrom.position,
                        _ => throw new ArgumentOutOfRangeException()
                    };
            if (rotation)
                if (spaceTo == Space.Self)
                    transform.localRotation = spaceFrom switch {
                        Space.Self => copyFrom.localRotation,
                        Space.World => copyFrom.rotation,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                else
                    transform.rotation = spaceFrom switch {
                        Space.Self => copyFrom.localRotation,
                        Space.World => copyFrom.rotation,
                        _ => throw new ArgumentOutOfRangeException()
                    };
            if (scale)
                transform.localScale = spaceFrom switch {
                    Space.Self => copyFrom.localScale,
                    Space.World => copyFrom.lossyScale,
                    _ => throw new ArgumentOutOfRangeException()
                };
        }

    }

}