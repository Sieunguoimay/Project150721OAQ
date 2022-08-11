using System.Collections.Generic;
using UnityEngine;

namespace SNM
{
    public class RayPointer
    {
        private readonly List<ITarget> _listeners = new();
        private readonly Camera _camera;

        public RayPointer()
        {
            _camera = Camera.main;
        }

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

        public void Update(float deltaTime)
        {
            if (Input.GetMouseButtonUp(0))
            {
                ProcessMouse(Input.mousePosition);
            }
        }

        private void ProcessMouse(Vector3 position)
        {
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

                Debug.Log((l as MonoBehaviour)?.name + " " + distance);
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