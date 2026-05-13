using System;
using System.Linq;
using UnityEngine;

namespace Utils {

    [RequireComponent(typeof (PolygonCollider2D))]
    public class PolygonCollider2DVisualizer : MonoBehaviour {

        public Color outlineColor;
        public Color fillColor;

        public PolygonCollider2D Collider {
            get {
                if (_collider == null)
                    _collider = GetComponent<PolygonCollider2D>();
                return _collider;
            }
        }
        [NonSerialized] private PolygonCollider2D _collider;
        [NonSerialized] private Mesh _m;

        private Vector3[] _points;
        private int[] _triangles;

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

            if (_m == null || _points.Length != Collider.points.Length) {
                _m = new();
                _m.subMeshCount = 2;
                _points = new Vector3[Collider.points.Length];
                _triangles = new int[(_points.Length - 2) * 3];
            }
            
            for (int i = 0, j = 1; i < _triangles.Length; i += 3, j++) {
                _triangles[i] = 0;
                _triangles[i + 1] = j;
                _triangles[i + 2] = j + 1;
            }

            for (int i = 0; i < Collider.points.Length; i++)
                _points[i] = Vector3.Scale(Collider.points[i], transform.lossyScale) + transform.position;

            _m.SetVertices(_points);
            _m.SetIndices(_triangles, MeshTopology.Triangles, 0);
            Array.Reverse(_triangles);
            _m.SetIndices(_triangles, MeshTopology.Triangles, 1);
            _m.RecalculateNormals();

            Gizmos.color = outlineCol;
            Gizmos.DrawLineStrip(_points, true);
            Gizmos.color = fillCol;
            Gizmos.DrawMesh(_m);

        }

    }

}