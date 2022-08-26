using System;
using UnityEngine;

namespace Common.DrawLine
{
    public class DrawingSurface : MonoBehaviour
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

#if UNITY_EDITOR
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var point = GetWorldDrawPoint();
                DrawBegin(new Vector2(point.x, point.z));
            }

            if (Input.GetMouseButton(0))
            {
                var point = GetWorldDrawPoint();
                Draw(new Vector2(point.x, point.z), 0.1f, 0.2f);
            }
        }

        private Vector3 GetWorldDrawPoint()
        {
            var point = GetMouseWorldSpace(transform, _camera, Input.mousePosition);
            point.y = 0;
            return point;
        }

        private static Vector3 GetMouseWorldSpace(Transform space, Camera worldCamera, Vector3 screenPosition)
        {
            var ray = worldCamera.ScreenPointToRay(new Vector3(screenPosition.x,
                screenPosition.y, worldCamera.farClipPlane));

            return new Plane(space.up, space.position).Raycast(ray, out var hit) ? ray.GetPoint(hit) : Vector3.zero;
        }
#endif
    }
}

/*

*/