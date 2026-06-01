using UnityEngine;
using UnityEngine.InputSystem;

namespace Utils {

    public static class Miscellaneous {

        #region Meshes
        private static readonly Vector3[] DebugArrowVertices = {
            new(-0.500000f      ,-2.250000f     ,0f),
            new(-0.500000f      , 0.085786f     ,0f),
            new(-1.414213f      ,-0.828427f     ,0f),
            new(-2.121320f      ,-0.121320f     ,0f),
            new( 0.000000f      , 2.000000f     ,0f),
            new( 2.121320f      ,-0.121320f     ,0f),
            new( 1.414213f      ,-0.828427f     ,0f),
            new( 0.500000f      , 0.085786f     ,0f),
            new( 0.500000f      ,-2.250000f     ,0f),
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

        private static readonly Vector3[] DebugExclamationVertices = {
            new(-0.300000f      , 1.000000f     ,0f),
            new(-0.300000f      ,-0.000000f     ,0f),
            new(-0.200000f      ,-0.400000f     ,0f),
            new( 0.200000f      ,-0.400000f     ,0f),
            new( 0.300000f      ,-0.000000f     ,0f),
            new( 0.300000f      , 1.000000f     ,0f),
            new(-0.250000f      ,-0.600000f     ,0f),
            new( 0.250000f      ,-0.600000f     ,0f),
            new( 0.250000f      ,-1.100000f     ,0f),
            new(-0.250000f      ,-1.100000f     ,0f),
        }; // (y-2)*3 = x  x/3 = y-2  (x/3)+2 = y
        private static readonly int[] DebugExclamationTriangles0 = {
            0, 5, 4,
            0, 4, 1,
            1, 4, 3,
            1, 3, 2,
        };
        private static readonly int[] DebugExclamationTriangles1 = {
            6, 7, 8,
            6, 8, 9,
        };
        #endregion
        
        private static Mesh DebugExclamationMesh {
            get {
                if (_debugExclamationMesh == null) {
                    _debugExclamationMesh = new Mesh {
                        name = "Debug Exclamation Mesh",
                        subMeshCount = 2
                    };
                    _debugExclamationMesh.SetVertices(DebugExclamationVertices);
                    _debugExclamationMesh.SetTriangles(DebugExclamationTriangles0, 0);
                    _debugExclamationMesh.SetTriangles(DebugExclamationTriangles1, 1);
                    _debugExclamationMesh.RecalculateNormals();
                }
                return _debugExclamationMesh;
            }
        }
        private static Mesh _debugExclamationMesh;
        
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

        /// <summary>
        /// Function to draw an exclamation mark gizmo at the given <paramref name="position"/>.
        /// </summary>
        /// <param name="position">Position to draw the exclamation mark.</param>
        /// <param name="color">Colour to draw the exclamation mark.</param>
        /// <param name="fillOpacity">Opacity of the fill of the exclamation mark.</param>
        /// <param name="scale">Size of the exclamation mark.</param>
        public static void DrawExclamationGizmo(Vector3 position, Color color, float fillOpacity = 1, float scale = 1) {
            Color fill = color;
            fill.a = fillOpacity;
            DrawExclamationGizmo(position, fill, color, scale);
        }
        /// <summary>
        /// Function to draw an exclamation mark gizmo at the given <paramref name="position"/>.
        /// </summary>
        /// <param name="position">Position to draw the exclamation mark.</param>
        /// <param name="fill">Colour to fill in the exclamation mark.</param>
        /// <param name="outline">Colour to draw the lines of the exclamation mark.</param>
        /// <param name="scale">Size of the exclamation mark.</param>
        public static void DrawExclamationGizmo(Vector3 position, Color fill, Color outline, float scale = 1) {
            if (fill != Color.clear) {
                Gizmos.color = fill;
                Gizmos.DrawMesh(DebugExclamationMesh, position, Quaternion.identity, Vector3.one * scale);
            }
            if (outline != Color.clear) {
                Gizmos.color = outline;
                Vector3[] points1 = new Vector3[DebugExclamationTriangles0.Length/3 + 2];
                for (int i = 0; i < points1.Length; i++)
                    points1[i] = Vector3.Scale(DebugExclamationVertices[i], Vector3.one * scale) + position;
                Vector3[] points2 = new Vector3[DebugExclamationTriangles1.Length/3 + 2];
                for (int i = 0; i < points2.Length; i++)
                    points2[i] = Vector3.Scale(DebugExclamationVertices[i + points1.Length], Vector3.one * scale) + position;
                Gizmos.DrawLineStrip(points1, true);
                Gizmos.DrawLineStrip(points2, true);
            }
        }

    }

}