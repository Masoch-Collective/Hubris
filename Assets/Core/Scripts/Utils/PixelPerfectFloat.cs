using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils {

    public enum WorldValueModes {

        Units,
        Pixels

    }

    [Serializable]
    public class PixelPerfectFloat {

        public const int PixelsPerUnit = 16;

        [SerializeField]
        private WorldValueModes mode;
        public WorldValueModes Mode => mode;
        [SerializeField]
        private int pixels;
        public int? Pixels => mode == WorldValueModes.Pixels ? pixels : null;
        public float value;

    }

}