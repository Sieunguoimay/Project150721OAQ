using System;
using UnityEngine;

namespace Common.DrawLine
{
    public class DrawingSurface : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshFilter meshFilter;

        private readonly DrawMesh _drawMesh = new();
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
            var mesh = _drawMesh.CreateNew();
            meshFilter.mesh = mesh;
        }

        private void Update()
        {
            var drawPoint = GetWorldDrawPoint();

            var pos = meshFilter.transform.position;
            pos.x = drawPoint.x;
            pos.y = 0f;
            pos.z = drawPoint.z;
            meshFilter.transform.position = pos;

            Debug.Log(drawPoint + " " + Input.mousePosition);
        }

        private Vector3 GetWorldDrawPoint()
        {
            return GetMouseWorldSpace(transform, _camera, Input.mousePosition);
        }

        private static Vector3 GetMouseWorldSpace(Transform space, Camera worldCamera, Vector3 screenPosition)
        {
            var ray = worldCamera.ScreenPointToRay(new Vector3(screenPosition.x,
                screenPosition.y, worldCamera.farClipPlane));

            return new Plane(space.up, space.position).Raycast(ray, out var hit) ? ray.GetPoint(hit) : Vector3.zero;
        }
    }
}