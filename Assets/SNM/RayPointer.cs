using System.Collections.Generic;
using UnityEngine;

namespace SNM
{
    public class MonoRayPointer : MonoBehaviour
    {
        private RayPointer _rayPointer;

        private void Start()
        {
            _rayPointer = RayPointer.Instance;
            _rayPointer.SetCamera(Camera.main);
        }

        public void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                _rayPointer.ProcessMouse(Input.mousePosition);
            }
        }
    }

    public class RayPointer
    {
        private readonly List<IRaycastTarget> _listeners = new();
        private Camera _camera;
        private Camera _defaultCamera;
        private static RayPointer _instance;

        public static RayPointer Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = new RayPointer();
                new GameObject(nameof(RayPointer)).AddComponent<MonoRayPointer>();
                return _instance;
            }
        }

        public void SetCamera(Camera cam)
        {
            _camera = cam;
        }

        public void Clear()
        {
            _listeners.Clear();
        }

        public void Register(IRaycastTarget target)
        {
            _listeners.Add(target);
        }

        public void Unregister(IRaycastTarget target)
        {
            _listeners.Remove(target);
        }

        public void ProcessMouse(Vector3 position)
        {
            if (_camera == null)
            {
                _camera = _defaultCamera;
            }

            var ray = _camera.ScreenPointToRay(position);

            var minDistance = float.MaxValue;
            IRaycastTarget selectedTarget = null;

            foreach (var l in _listeners)
            {
                if (!l.Bounds.IntersectRay(ray, out var distance)) continue;
                if (minDistance <= distance) continue;
                minDistance = distance;
                selectedTarget = l;
            }

            selectedTarget?.OnHit(ray, minDistance);
        }

        public interface IRaycastTarget
        {
            Bounds Bounds { get; }
            void OnHit(Ray ray, float distance);
        }
    }
}