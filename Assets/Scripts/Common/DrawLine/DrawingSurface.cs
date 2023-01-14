using System;
using UnityEngine;

namespace Common.DrawLine
{
    public interface IDrawingSurface
    {
        void DrawBegin(Vector2 point);
        void Draw(Vector2 point, float lineThickness, float minDistance);
        void DryInk(string meshName = "(Static)mesh");
        Vector3 Get3DPoint(Vector2 point);
    }

    public class DrawingSurface : MonoBehaviour, IDrawingSurface
    {
        [SerializeField] private MeshFilter meshFilter;

        private readonly DrawMesh _drawMesh = new();

        public void DrawBegin(Vector2 point)
        {
            meshFilter.mesh = _drawMesh.CreateNew(point);
        }

        public void Draw(Vector2 point, float lineThickness, float minDistance)
        {
            _drawMesh.Draw(point, lineThickness, minDistance);
        }

        public void ResetAll()
        {
            meshFilter.mesh = null;
        }
        public void DryInk(string meshName = "(Static)mesh")
        {
            meshFilter.mesh = _drawMesh.GenerateStaticMesh(meshName);
            meshFilter.mesh.RecalculateBounds();
        }

        public Vector3 Get3DPoint(Vector2 point)
        {
            return transform.TransformPoint(new Vector3(point.x, 0, point.y));
        }
    }
}