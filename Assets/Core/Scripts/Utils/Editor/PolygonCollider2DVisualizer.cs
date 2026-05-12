using System;
using System.Linq;
using UnityEngine;

namespace Utils.Editor {

    [RequireComponent(typeof (PolygonCollider2D))]
    public class PolygonCollider2DVisualizer : MonoBehaviour {

        public Color outlineColor;
        public Color fillColor;

        [NonSerialized]
        private PolygonCollider2D _collider;

        [NonSerialized]
        private Mesh _m;
        private Mesh _mrev;

        public PolygonCollider2D Collider {
            get {
                if (_collider == null)
                    _collider = GetComponent<PolygonCollider2D>();
                return _collider;
            }
        }

        private void OnDrawGizmos() {
            if (enabled)
                DrawPolygon(
                    outlineColor,
                    fillColor);
        }

        private void OnDrawGizmosSelected() {
            if (enabled)
                DrawPolygon(
                    Color.Lerp(outlineColor, Color.white, 0.1f),
                    Color.Lerp(fillColor, Color.white, 0.1f));
        }

        private void DrawPolygon(Color outlineCol, Color fillCol) {
            Vector3[] points = new Vector3[Collider.points.Length];

            for (int i = 0; i < Collider.points.Length; i++)
                points[i] = Vector3.Scale(Collider.points[i], transform.lossyScale) + transform.position;

            Gizmos.color = outlineCol;
            Gizmos.DrawLineStrip(points, true);

            _m = new Mesh {
                name = "Polygon Collider 2D Shape",
                vertices = points,
            };

            int[] triangles = new int[(points.Length - 2) * 3];
            int ind = 1;

            for (int i = 0; i < triangles.Length; i += 3) {
                triangles[i] = 0;
                triangles[i + 1] = ind;
                triangles[i + 2] = ++ind;
            }

            _m.SetIndices(triangles, MeshTopology.Triangles, 0);
            _m.RecalculateNormals();
            Gizmos.color = fillCol;
            Gizmos.DrawMesh(_m);
            _m.triangles = _m.triangles.Reverse().ToArray();
            _m.RecalculateNormals();
            Gizmos.DrawMesh(_m);

        }

    }

}