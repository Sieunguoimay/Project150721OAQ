using System.Collections.Generic;
using Common;
using Common.ResolveSystem;
using Gameplay;
using TMPro.Examples;
using UnityEngine;

namespace SNM
{
    public class RayPointer : Singleton<RayPointer>
    {
        private readonly List<IRaycastTarget> _listeners = new();
        private Camera _camera;

        public void SetCamera(Camera cam)
        {
            _camera = cam;
        }

        public void Reset()
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

        public void Update()
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
            IRaycastTarget selectedTarget = null;

            for (var i = 0; i < _listeners.Count; i++)
            {
                var l = _listeners[i];
                if (!l.Bounds.IntersectRay(ray, out var distance)) continue;
                if (minDistance > distance)
                {
                    minDistance = distance;
                    selectedTarget = l;
                }
            }

            selectedTarget?.OnHit(ray, minDistance);
            for (var i = 0; i < _listeners.Count; i++)
            {
                if (_listeners[i] != selectedTarget && _listeners[i] is IRaycastMiss miss)
                {
                    miss.OnMiss();
                }
            }
        }

        public interface IRaycastTarget
        {
            Bounds Bounds { get; }
            void OnHit(Ray ray, float distance);
        }

        public interface IRaycastMiss : IRaycastTarget
        {
            void OnMiss();
        }
    }
}