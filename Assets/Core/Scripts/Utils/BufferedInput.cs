using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utils {

    /// <summary>
    /// Buffered Input is a system designed to facilitate buffered input registration, i.e., inputs will be "remembered" for a specified duration. Accessing Value will return true if this input was buffered recently enough. Allows Unity InputSystem Actions to be registered for automatic buffering.
    /// </summary>
    [Serializable]
    public class BufferedInput {

        public enum BufferContext {
            /// <summary>
            /// Buffer duration is measured in seconds
            /// </summary>
            Time,
            /// <summary>
            /// Buffer duration is measured in update frames
            /// </summary>
            UpdateFrames,
            /// <summary>
            /// Buffer duration is measured according to custom time
            /// </summary>
            Custom
        }
        
        /// <summary>
        /// What time metric to use for calculations
        /// </summary>
        public BufferContext mode;
        /// <summary>
        /// How long the buffer window lasts.
        /// </summary>
        public float bufferDuration;
        /// <summary>
        /// Use this variable to specify the current time for BufferContext.Custom mode.
        /// </summary>
        public int customTime;
        /// <summary>
        /// If non-null, only this gamepad may buffer this input
        /// </summary>
        public Gamepad Filter;
        
        private float _lastBufferTime;
        private float CurrentTime => mode switch {
            BufferContext.Time => Time.time,
            BufferContext.UpdateFrames => Time.frameCount,
            BufferContext.Custom => customTime,
            _ => 0
        };

        /// <summary>
        /// Whether this input was last pressed within the buffer window duration.
        /// Setting to true will update last buffered time to current time (determined by mode.)
        /// Setting to false will clear last buffered input.
        /// </summary>
        public bool Value => CurrentTime - _lastBufferTime <= bufferDuration;

        /// <summary>
        /// Register an InputAction to automatically buffer this input when performed.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="filter"></param>
        public void SetAction(InputAction action, Gamepad filter = null) {
            action.Enable();
            action.started += Buffer;
        }

        public void ClearAction(InputAction action) {
            action.Disable();
            action.started -= Buffer;
        }

        /// <summary>
        /// Register current time as the time at which this input was last performed.
        /// </summary>
        public void Buffer(InputAction.CallbackContext _ = default) => _lastBufferTime = CurrentTime;

        /// <summary>
        /// Clear buffer, making value false regardless of how recently this input was performed
        /// </summary>
        /// <remarks>
        /// Use current time minus buffer duration to ensure that last buffer time is outside buffer window.
        /// Make it one less than that, since value is calculated with less than or equal.
        /// Potentially overkill, but covers edge case of negative times (can occur with custom frame mode)
        /// </remarks>
        public void ClearBuffer() => _lastBufferTime = CurrentTime -bufferDuration - 1;

        /// <summary>
        /// Implicit cast override returns Value
        /// </summary>
        /// <param name="bufferedInput">Buffered input instance to obtain Value from</param>
        /// <returns>Value of given <paramref name="bufferedInput"/></returns>
        public static implicit operator bool(BufferedInput bufferedInput) => bufferedInput.Value;

    }

}