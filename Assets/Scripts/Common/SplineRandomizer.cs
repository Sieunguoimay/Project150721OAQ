using System;
using Curve;
using UnityEngine;

namespace Common
{
    public class SplineRandomizer
    {
        private readonly BezierSpline _spline;
        private readonly Config _config;

        [Serializable]
        public class Config
        {
            [SerializeField] private float radius = 1f;
            [SerializeField] private int segmentNum = 3;

            public float Radius => radius;

            public int SegmentNum => segmentNum;
        }

        public SplineRandomizer(Config config, BezierSpline spline)
        {
            _spline = spline;
            _config = config;
        }

        public void Randomize(Vector3 startPoint, Vector3 endPoint)
        {
            _spline.Reset();
            _spline.GetPointOnSegment(0, 0);
        }
    }
}