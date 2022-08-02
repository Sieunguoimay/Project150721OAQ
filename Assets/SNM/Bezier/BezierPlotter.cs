using System;
using System.Collections.Generic;
using UnityEngine;

namespace SNM.Bezier
{
    public class BezierPlotter : MonoBehaviour
    {
        [NonSerialized] private Vector3[] _points;

        public void Setup()
        {
            CollectPoints(GetTransforms());
        }

        private void CollectPoints(IReadOnlyList<Transform> transforms)
        {
            _points = new Vector3[transforms.Count];

            for (var i = 0; i < _points.Length; i++)
            {
                _points[i] = transforms[i].position;
            }
        }

        private Transform[] GetTransforms()
        {
            var n = transform.childCount;
            var ts = new Transform[n];
            for (var i = 0; i < n; i++)
            {
                ts[i] = transform.GetChild(i).transform;
            }

            return ts;
        }

        public Vector3[] GetPoints() => _points;

        public static float CalculateT(Vector3[] points, Vector3 position)
        {
            float t = -1;
            var minDistance = float.MaxValue;
            for (var i = 0.0f; i <= 1.0f; i += 0.02f)
            {
                var p = Bezier.ComputeBezierCurve3D(points, Mathf.Min(1f, i));
                var diff = position - p;
                var dist = diff.sqrMagnitude;
                if (!(dist < minDistance)) continue;
                
                minDistance = diff.sqrMagnitude;
                t = i;
            }

            return t;
        }


#if UNITY_EDITOR
        [ContextMenu("Plot")]
        public void Plot()
        {
            var transforms = GetTransforms();
            if (transforms.Length < 3) return;
            CollectPoints(transforms);
        }

        private void OnDrawGizmos()
        {
            if (_points == null || _points.Length < 3) return;

            Vector3 from = _points[0], to;
            for (var i = 0.0f; i <= 1.0f; i += 0.02f)
            {
                to = Bezier.ComputeBezierCurve3D(_points, Mathf.Min(1f, i));
                Gizmos.DrawLine(from, to);
                from = to;
            }

            to = Bezier.ComputeBezierCurve3D(_points, 1f);
            Gizmos.DrawLine(from, to);
        }
#endif
    }
}