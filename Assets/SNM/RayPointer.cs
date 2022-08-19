using System.Collections.Generic;
using Common;
using Common.ResolveSystem;
using TMPro.Examples;
using UnityEngine;

namespace SNM
{
    public class RayPointer : Singleton<RayPointer>
    {
        private readonly List<ITarget> _listeners = new();
        private Camera _camera;

        public void Reset()
        {
            _listeners.Clear();
        }

        public void Register(ITarget target)
        {
            _listeners.Add(target);
        }

        public void Unregister(ITarget target)
        {
            _listeners.Remove(target);
        }

        public void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                ProcessMouse(Input.mousePosition);
            }
        }

        private void ProcessMouse(Vector3 position)
        {
            if (_camera == null)
            {
                // _camera = Resolver.Instance.Resolve<CameraManager>().Camera;
            }

            var ray = _camera.ScreenPointToRay(position);

            var minDistance = float.MaxValue;
            ITarget selectedTarget = null;

            foreach (var l in _listeners)
            {
                if (!l.Bounds.IntersectRay(ray, out var distance)) continue;
                if (minDistance > distance)
                {
                    minDistance = distance;
                    selectedTarget = l;
                }

                // Debug.Log((l as MonoBehaviour)?.name + " " + distance);
            }

            selectedTarget?.OnHit(ray, minDistance);
        }

        public interface ITarget
        {
            Bounds Bounds { get; }

            void OnHit(Ray ray, float distance);
        }
    }
}