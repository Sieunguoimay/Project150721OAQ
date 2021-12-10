using System;
using Curve;
using UnityEngine;
using UnityEngine.Events;

namespace Common
{
    public class BezierMover : MonoBehaviour
    {
        [SerializeField] private Config config;
        [SerializeField] private UnityEvent onMove;

        [Serializable]
        public class Config
        {
            [SerializeField, Range(0.01f, 1f)] private float speed = 0.1f;
            [SerializeField] private BezierSpline initialPath;
            [SerializeField] private bool loop;
            public float Speed => speed;
            public BezierSpline InitialPath => initialPath;
            public bool Loop => loop;
        }

        private BezierSpline _path;
        private bool _moving;
        private float _position;

        private void Start()
        {
            ChangePath(config.InitialPath);
            _path.UpdateCurveLength();
        }

        public void ChangePath(BezierSpline path)
        {
            _path = path;
        }

        [ContextMenu("Move")]
        public void Move()
        {
            _moving = true;
            _position = 0f;
            onMove?.Invoke();
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            _moving = false;
        }

        private void Update()
        {
            if (_moving && _path != null)
            {
                _position += Time.deltaTime * config.Speed;

                var pos3D = _path.GetPosition(_position);
                var dir = _path.GetDirection(_position);

                transform.position = pos3D;
                transform.rotation = Quaternion.LookRotation(dir);

                if (_position >= 1f)
                {
                    if (config.Loop)
                    {
                        Move();
                    }
                    else
                    {
                        _moving = false;
                    }
                }
            }
        }
    }
}