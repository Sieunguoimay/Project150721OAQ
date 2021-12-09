using System;
using Curve;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Common
{
    public class SplineRandomizer
    {
        private readonly BezierSpline _spline;
        private readonly Config _config;

        [Serializable]
        public class Config
        {
            [SerializeField] private Vector2 radius = new Vector2(0.5f, 1f);
            public Vector2 Radius => radius;
        }

        public SplineRandomizer(Config config, BezierSpline spline)
        {
            _spline = spline;
            _config = config;
        }

        public void Randomize(Vector3 startPoint, Vector3 endPoint)
        {
            var diff = endPoint - startPoint;
            var quaternion = Quaternion.LookRotation(diff);
            var segmentSpace = Mathf.Sqrt(2f * _config.Radius.y) + _config.Radius.y;
            var distance = diff.magnitude;
            var segmentNum = Mathf.RoundToInt(distance / segmentSpace);
            _spline.Reset();
            var up = Random.Range(0f, 1f) > 0.5f;
            for (int i = 1; i < segmentNum; i++)
            {
                _spline.AddSegment();

                int pointIndex = _spline.GetIndexOfPointOnSegment(i, 0);
                var pos = startPoint + (diff / segmentNum) * i;

                _spline.SetPointMode(pointIndex, BezierPointMode.Mirrored);
                _spline.SetPoint(pointIndex, pos);
                _spline.SetPoint(pointIndex + (up ? 1 : -1), pos + GetRandomControlPoint(quaternion, !up));
                up = !up;
            }

            _spline.SetPointMode(0, BezierPointMode.Mirrored);
            _spline.SetPoint(0, startPoint);
            _spline.SetPoint(1,
                startPoint + 0.5f * (_spline.GetPoint(2) - startPoint));

            _spline.SetPointMode(_spline.PointCount - 1, BezierPointMode.Mirrored);
            _spline.SetPoint(_spline.PointCount - 1, endPoint);
            _spline.SetPoint(_spline.PointCount - 2,
                endPoint + 0.5f * (_spline.GetPoint(_spline.PointCount - 3) - endPoint));
        }

        private Vector3 GetRandomControlPoint(Quaternion rotation, bool up)
        {
            var randomRotation = Quaternion.Euler((up ? -1f : 1f) * Random.Range(30f, 60f), 0, Random.Range(-20f, 20f));
            return rotation * randomRotation *
                   (Vector3.up * Random.Range(_config.Radius.x, _config.Radius.y));
        }
    }
}