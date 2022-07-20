using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Implementations.Transporter
{
    public class Mover : IMover, IMovingStyleHandler
    {
        private Quaternion _rotation;
        private Vector3 _position;
        private readonly List<IMoverListener> _listeners = new List<IMoverListener>();

        private bool _moving;
        private Vector3 _target;
        private readonly ConfigData _config;
        private IMovingStyle _style;

        [Serializable]
        public class ConfigData
        {
            [Range(0.1f, 100f)] public float speed = 1f;
        }

        public Mover(ConfigData config)
        {
            _config = config;
        }

        public void SetPosition(Vector3 position)
        {
            _position = position;
        }

        public Vector3 GetPosition()
        {
            return _position;
        }

        public void SetRotation(Quaternion rotation)
        {
            _rotation = rotation;
        }

        public Quaternion GetRotation()
        {
            return _rotation;
        }

        public void Attach(IMoverListener item)
        {
            _listeners.Add(item);
        }

        public void Detach(IMoverListener item)
        {
            _listeners.Remove(item);
        }

        public IEnumerable<IMoverListener> GetItems()
        {
            return _listeners;
        }

        public void MoveTo(Vector3 position)
        {
            _moving = true;
            _target = position;
            _style?.StartMoving(_target, _config.speed);
        }

        public void Loop(float deltaTime)
        {
            if (_moving)
            {
                _style?.Loop(deltaTime);
            }
        }

        public void SetMovingStyle(IMovingStyle style)
        {
            _style = style;
            _style.SetMover(this);
            _style.SetMovingStyleHandler(this);
        }

        private void NotifyListeners()
        {
            foreach (var l in _listeners)
            {
                l.OnReachTarget(this);
            }
        }

        public void OnMovingStyleResult()
        {
            if (_moving)
            {
                _moving = false;
                NotifyListeners();
            }
        }
    }
}