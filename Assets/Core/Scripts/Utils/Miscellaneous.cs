using UnityEngine;
using UnityEngine.InputSystem;

namespace Utils {

    public static class Miscellaneous {

        private static readonly Vector3[] DebugArrowVertices = {
            new(-0.500000f      ,-2.250000f     ,0.000000f),
            new(-0.500000f      ,0.085786f      ,0.000000f),
            new(-1.414213f      ,-0.828427f     ,-0.000000f),
            new(-2.121320f      ,-0.121320f     ,-0.000000f),
            new(0.000000f       ,2.000000f      ,0.000000f),
            new(2.121320f       ,-0.121320f     ,-0.000000f),
            new(1.414213f       ,-0.828427f     ,-0.000000f),
            new(0.500000f       ,0.085786f      ,0.000000f),
            new(0.500000f       ,-2.250000f     ,0.000000f),
        };
        private static readonly int[] DebugArrowTriangles = {
            1, 2, 3,
            1, 3, 4,
            1, 4, 0,
            0, 4, 8,
            8, 4, 7,
            6, 7, 4,
            6, 4, 5
        };
        private static Mesh DebugArrowMesh {
            get {
                if (_debugArrowMesh == null) {
                    _debugArrowMesh = new Mesh { name = "Debug Arrow Mesh" };
                    _debugArrowMesh.SetVertices(DebugArrowVertices);
                    _debugArrowMesh.SetTriangles(DebugArrowTriangles, 0);
                    _debugArrowMesh.RecalculateNormals();
                }
                return _debugArrowMesh;
            }
        }
        private static Mesh _debugArrowMesh;
        
        public delegate void ActionFunctionDelegate(InputAction.CallbackContext context);
        /// <summary>
        /// Middleman for InputAction callbacks, which calls the given <paramref name="function"/> if the gamepad that triggered the Action matches the <paramref name="filter"/> gamepad; or if <paramref name="allowNonGamepads"/> is true and the Action was triggered by anything other than a gamepad.
        /// </summary>
        /// <param name="context">Action callback context.</param>
        /// <param name="function">Function to call.</param>
        /// <param name="filter">Gamepad instance allowed to call <paramref name="function"/>; aborts if null.</param>
        /// <param name="allowNonGamepads">Whether to call <paramref name="function"/> if Action was not triggered by a gamepad.</param>
        public static void GamepadFilter(InputAction.CallbackContext context, ActionFunctionDelegate function, Gamepad filter, bool allowNonGamepads = true) {
            if (ValidateContext(context, filter, allowNonGamepads))
                function(context);
        }

        private static bool ValidateContext(InputAction.CallbackContext context, Gamepad filter, bool allowNonGamepads) {
            return context.control == null || context.control.device == null || (context.control.device is Gamepad pad && filter != null && pad == filter) || allowNonGamepads;
        }
        
        /// <summary>
        /// Function to draw an arrow gizmo at the given <paramref name="position"/> and <paramref name="rotation"/>.
        /// </summary>
        /// <param name="position">Position to draw the arrow.</param>
        /// <param name="fill">Colour to fill in the arrow.</param>
        /// <param name="outline">Colour to draw the lines of the arrow.</param>
        /// <param name="rotation">Angle of the arrow.</param>
        /// <param name="scale">Size of the arrow.</param>
        /// <param name="offset">Moves the arrow by this much in the given <paramref name="rotation"/>.</param>
        public static void DrawArrowGizmo(Vector3 position, Color fill, Color outline, float rotation = 0, float scale = 1, float offset = 1) {
            Quaternion direction = Quaternion.Euler(0, 0, rotation);
            if (fill != Color.clear) {
                Gizmos.color = fill;
                Gizmos.DrawMesh(DebugArrowMesh, 0, position + direction * (Vector3.up * offset), direction, Vector3.one * scale);
            }
            if (outline != Color.clear) {
                Gizmos.color = outline;
                Vector3[] points = new Vector3[DebugArrowVertices.Length];
                for (int i = 0; i < points.Length; i++)
                    points[i] = direction * (Vector3.Scale(DebugArrowVertices[i], Vector3.one * scale) + Vector3.up * offset) + position;
                Gizmos.DrawLineStrip(points, true);
            }
        }

    }

}