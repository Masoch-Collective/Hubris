using System;
using UnityEngine;

namespace Utils {

    public enum WorldValueModes {

        Pixels,
        Float

    }

    /// <summary>
    /// Custom float wrapper that allows setting its value as a number of pixels in the inspector
    /// </summary>
    /// <remarks>My most twelve-billion IQ utility yet! Has a custom PropertyDrawer to neatly show either the pixel int or the direct float, plus an inline dropdown to switch between the two!</remarks>
    [Serializable]
    public class PixelPerfectFloat {

        public const int PixelsPerUnit = 16;

        [SerializeField]
        private WorldValueModes mode;
        /// <summary>
        /// Whether this float is being controlled directly or via the Pixels int
        /// </summary>
        public WorldValueModes Mode => mode;
        [SerializeField]
        private int pixels;
        /// <summary>
        /// Value represented as the number of pixels (null if mode == float)
        /// </summary>
        public int? Pixels => mode == WorldValueModes.Pixels ? pixels : null;
        /// <summary>
        /// Float value (implicit cast available)
        /// </summary>
        public float value;

        public static implicit operator float(PixelPerfectFloat ppf) {
            return ppf.value;
        }

    }

}