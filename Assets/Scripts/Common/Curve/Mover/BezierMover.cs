﻿using System;
using Common.Curve;
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
            [SerializeField] private BezierSplineMono initialPath;
            [SerializeField] private bool loop;
            public BezierSplineMono InitialPath => initialPath;
            public bool Loop => loop;
        }

        private BezierSpline _spline;
        private bool _moving;
        private float _time;
        private float _duration;
        public event Action OnComplete = delegate { };

        private void Start()
        {
            if (_spline == null)
            {
                _spline = config.InitialPath.CreateSpline();
            }

            config.InitialPath.UpdateCurveLength();
        }

        [ContextMenu("Move")]
        public void Move(float duration)
        {
            _moving = true;
            _time = 0f;
            _duration = duration;
            onMove?.Invoke();
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            _moving = false;
        }

        private void Update()
        {
            if (!_moving || _spline == null) return;

            _time += Time.deltaTime;

            var pos3D = _spline.GetPosition(_time / _duration);
            var dir = _spline.GetDirection(_time / _duration);

            transform.position = pos3D;
            transform.rotation = Quaternion.LookRotation(dir);

            if (!(_time >= _duration)) return;

            if (config.Loop)
            {
                Move(_duration);
            }
            else
            {
                _moving = false;
                OnComplete?.Invoke();
            }
        }
    }
}