﻿using System.Collections.Generic;
using UnityEngine;

namespace SNM
{
    public class RayPointer
    {
        private readonly List<IListener> _listeners = new();
        private readonly Camera _camera;

        public RayPointer()
        {
            _camera = Camera.main;
        }

        public void Reset()
        {
            _listeners.Clear();
        }

        public void Register(IListener listener)
        {
            _listeners.Add(listener);
        }

        public void Unregister(IListener listener)
        {
            _listeners.Remove(listener);
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
            IListener selectedListener = null;

            foreach (var l in _listeners)
            {
                if (!l.Bounds.IntersectRay(ray, out var distance)) continue;
                if (minDistance > distance)
                {
                    minDistance = distance;
                    selectedListener = l;
                }

                Debug.Log((l as MonoBehaviour)?.name + " " + distance);
            }

            selectedListener?.OnHit(ray, minDistance);
        }

        public interface IListener
        {
            Bounds Bounds { get; }

            void OnHit(Ray ray, float distance);
        }
    }
}