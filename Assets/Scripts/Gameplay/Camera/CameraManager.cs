using System;
using Framework.DependencyInversion;
using UnityEngine;

namespace Gameplay.Camera
{
    public class CameraManager : SelfBindingDependencyInversionMonoBehaviour
    {
        private UnityEngine.Camera _camera = null;
        public UnityEngine.Camera Camera => _camera ? _camera : _camera = GetComponent<UnityEngine.Camera>();
        

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var plane = new Plane(Vector3.up, Vector3.zero);
            var points = GetFarPlaneCorners(Camera);
            var camPos = Camera.transform.position;
            var hitPoints = new Vector3[4];
            for (var i = 0; i < points.Length; i++)
            {
                var cornerPoint = points[i];
                var diff = cornerPoint - camPos;
                var dir = diff.normalized;
                var ray = new Ray(camPos, dir);
                if (plane.Raycast(ray, out var hitDistance) && hitDistance * hitDistance < diff.sqrMagnitude)
                {
                    hitPoints[i] = camPos + dir * hitDistance;
                }
                else
                {
                    return;
                }
            }

            for (var i = 0; i < hitPoints.Length; i++)
            {
                Gizmos.DrawLine(camPos, hitPoints[i]);
                Gizmos.DrawLine(hitPoints[i], hitPoints[(i + 1) % hitPoints.Length]);
            }
        }

        public static Vector3[] GetFarPlaneCorners(UnityEngine.Camera camera)
        {
            var camTransform = camera.transform;
            var farClipPlane = camera.farClipPlane;
            var halfVerticalSize = farClipPlane * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView / 2f);
            var halfHorizontalSize = halfVerticalSize * camera.aspect;

            var centerPoint = camTransform.position + camTransform.forward * farClipPlane;
            var up = camTransform.up;
            var right = camTransform.right;
            return new[]
            {
                centerPoint - up * halfVerticalSize - right * halfHorizontalSize,
                centerPoint + up * halfVerticalSize - right * halfHorizontalSize,
                centerPoint + up * halfVerticalSize + right * halfHorizontalSize,
                centerPoint - up * halfVerticalSize + right * halfHorizontalSize
            };
        }
#endif
    }
}