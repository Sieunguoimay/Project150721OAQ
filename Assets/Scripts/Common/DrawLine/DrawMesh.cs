using System;
using UnityEngine;

namespace Common.DrawLine
{
    public class DrawMesh
    {
        private Mesh _mesh;
        private Vector3 _lastDrawPoint;
        private Vector3 _lastForwardVector;

        public Mesh CreateNew(Vector2 point)
        {
            var drawPoint = new Vector3(point.x, 0f, point.y);
            _lastDrawPoint = drawPoint;
            _mesh = new Mesh();

            var vertices = new Vector3[4];
            var normals = new Vector3[4];
            var uv = new Vector2[4];
            var triangles = new int[6];

            vertices[0] = drawPoint; // + new Vector3(-1f, +0f, +1f) * lineThickness;
            vertices[1] = drawPoint; // + new Vector3(-1f, +0f, -1f) * lineThickness;
            vertices[2] = drawPoint; // + new Vector3(+1f, +0f, -1f) * lineThickness;
            vertices[3] = drawPoint; // + new Vector3(+1f, +0f, +1f) * lineThickness;

            normals[0] = Vector3.up;
            normals[1] = Vector3.up;
            normals[2] = Vector3.up;
            normals[3] = Vector3.up;

            uv[0] = Vector2.zero;
            uv[1] = Vector2.zero;
            uv[2] = Vector2.zero;
            uv[3] = Vector2.zero;

            triangles[0] = 0;
            triangles[1] = 3;
            triangles[2] = 1;
            triangles[3] = 1;
            triangles[4] = 3;
            triangles[5] = 2;

            _mesh.vertices = vertices;
            _mesh.normals = normals;
            _mesh.uv = uv;
            _mesh.triangles = triangles;

            _mesh.MarkDynamic();

            return _mesh;
        }

        public void Draw(Vector2 point, float lineThickness, float minDistance)
        {
            var drawPoint = new Vector3(point.x, 0f, point.y);

            lineThickness = Mathf.Max(0.01f, lineThickness);
            minDistance = Mathf.Max(0.01f, minDistance);

            var newForward = (drawPoint - _lastDrawPoint).normalized;

            var exceedMinDistance = Vector3.Distance(drawPoint, _lastDrawPoint) > minDistance;
            var exceedMinDirection = Math.Abs(Vector3.Dot(_lastForwardVector, newForward) - 1.0f) > 0.001f;

            if (exceedMinDirection)
            {
                if (Vector3.Dot(_lastForwardVector, newForward) > -0.5f)
                {
                    UpdateLastPoint(_lastDrawPoint, (newForward + _lastForwardVector).normalized, lineThickness);
                }

                DrawNewPoint(drawPoint, newForward, lineThickness);
                _lastDrawPoint = drawPoint;
                _lastForwardVector = newForward;
            }
            else if (exceedMinDistance)
            {
                UpdateLastPoint(drawPoint, newForward, lineThickness);
                _lastDrawPoint = drawPoint;
            }
        }

        private void UpdateLastPoint(Vector3 drawPoint, Vector3 forwardVector, float lineThickness)
        {
            var vertices = _mesh.vertices;

            var vertexIndex0 = vertices.Length - 2;
            var vertexIndex1 = vertices.Length - 1;

            var normal2D = new Vector3(0, -1f, 0);
            var newVertexUp = drawPoint + Vector3.Cross(forwardVector, normal2D) * lineThickness;
            var newVertexDown = drawPoint + Vector3.Cross(forwardVector, normal2D * -1f) * lineThickness;

            vertices[vertexIndex0] = newVertexDown;
            vertices[vertexIndex1] = newVertexUp;

            _mesh.vertices = vertices;
        }

        private void DrawNewPoint(Vector3 drawPoint, Vector3 forwardVector, float lineThickness)
        {
            var vertices = new Vector3[_mesh.vertices.Length + 2];
            var normals = new Vector3[_mesh.normals.Length + 2];
            var uv = new Vector2[_mesh.uv.Length + 2];
            var triangles = new int[_mesh.triangles.Length + 6];

            _mesh.vertices.CopyTo(vertices, 0);
            _mesh.normals.CopyTo(normals, 0);
            _mesh.uv.CopyTo(uv, 0);
            _mesh.triangles.CopyTo(triangles, 0);

            var vertexIndex0 = vertices.Length - 4;
            var vertexIndex1 = vertexIndex0 + 1;
            var vertexIndex2 = vertexIndex0 + 2;
            var vertexIndex3 = vertexIndex0 + 3;

            var normal2D = new Vector3(0, -1f, 0);
            var newVertexUp = drawPoint + Vector3.Cross(forwardVector, normal2D) * lineThickness;
            var newVertexDown = drawPoint + Vector3.Cross(forwardVector, normal2D * -1f) * lineThickness;

            vertices[vertexIndex2] = newVertexDown;
            vertices[vertexIndex3] = newVertexUp;

            normals[vertexIndex2] = Vector3.up;
            normals[vertexIndex3] = Vector3.up;

            uv[vertexIndex2] = Vector3.zero;
            uv[vertexIndex3] = Vector3.zero;

            var triangleIndex = triangles.Length - 6;
            triangles[triangleIndex + 0] = vertexIndex0;
            triangles[triangleIndex + 1] = vertexIndex2;
            triangles[triangleIndex + 2] = vertexIndex1;

            triangles[triangleIndex + 3] = vertexIndex1;
            triangles[triangleIndex + 4] = vertexIndex2;
            triangles[triangleIndex + 5] = vertexIndex3;

            _mesh.vertices = vertices;
            _mesh.normals = normals;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
        }

        public Mesh GenerateStaticMesh(string name)
        {
            var mesh = _mesh;
            _mesh = null;
            return new Mesh
            {
                vertices = mesh.vertices, uv = mesh.uv, triangles = mesh.triangles, normals = mesh.normals, name = name
            };
        }
    }
}