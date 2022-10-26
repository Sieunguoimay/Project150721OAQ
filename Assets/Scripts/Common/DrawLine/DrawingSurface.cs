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

        public void DryInk(string meshName = "(Static)mesh")
        {
            meshFilter.mesh = _drawMesh.GenerateStaticMesh(meshName);
            meshFilter.mesh.RecalculateBounds();
        }

        public Vector3 Get3DPoint(Vector2 point)
        {
            return transform.TransformPoint(new Vector3(point.x, 0, point.y));
        }


        // #if UNITY_EDITOR
//         private Camera _camera;
//
//         private void Start()
//         {
//             _camera = Camera.main;
//         }
//
//         private void Update()
//         {
//             if (Input.GetMouseButtonDown(0))
//             {
//                 var point = GetWorldDrawPoint();
//                 DrawBegin(new Vector2(point.x, point.z));
//             }
//
//             if (Input.GetMouseButton(0))
//             {
//                 var point = GetWorldDrawPoint();
//                 Draw(new Vector2(point.x, point.z), 0.1f, 0.2f);
//             }
//         }
//
//         private Vector3 GetWorldDrawPoint()
//         {
//             var point = GetMouseWorldSpace(transform, _camera, Input.mousePosition);
//             point.y = 0;
//             return point;
//         }
//
//         private static Vector3 GetMouseWorldSpace(Transform space, Camera worldCamera, Vector3 screenPosition)
//         {
//             var ray = worldCamera.ScreenPointToRay(new Vector3(screenPosition.x,
//                 screenPosition.y, worldCamera.farClipPlane));
//
//             return new Plane(space.up, space.position).Raycast(ray, out var hit) ? ray.GetPoint(hit) : Vector3.zero;
//         }
// #endif
    }
}