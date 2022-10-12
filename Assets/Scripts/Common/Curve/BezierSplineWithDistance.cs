using System.Collections.Generic;
using UnityEngine;

namespace Common.Curve
{
    public class BezierSplineWithDistance
    {
        private VertexData[] _vertices;
        private readonly float _unitLength;
        public float ArcLength { get; private set; }
        public IReadOnlyList<VertexData> Vertices => _vertices;
        public BezierSpline Spline { get; }

        public class VertexData
        {
            public Vector3 Vertex;
            public float T;
        }

        public BezierSplineWithDistance(BezierSpline spline, float unitLength = 0.1f)
        {
            Spline = spline;
            _unitLength = unitLength;
            ForceRegenerateVertices();
        }

        public Vector3 GetPointAtDistance(float distance)
        {
            return Spline.GetPoint(GetTAtDistance(distance));
        }
        
        public float GetTAtDistance(float distance)
        {
            distance = Mathf.Min(distance, ArcLength);

            var vertexIndex = Mathf.FloorToInt(distance / _unitLength);
            if (vertexIndex >= _vertices.Length - 1) return 1f;

            var redundant = distance - vertexIndex * _unitLength;
            var vertex1 = Vertices[vertexIndex];
            var vertex2 = Vertices[vertexIndex + 1];

            return Mathf.Lerp(vertex1.T, vertex2.T, redundant / _unitLength);
        }

        public void ForceRegenerateVertices()
        {
            _vertices = GenerateVerticesEvenly(Spline.ControlPoints, out var totalLength, _unitLength);
            ArcLength = totalLength;
        }

        private static VertexData[] GenerateVerticesEvenly(IReadOnlyList<Vector3> controlPoints, out float totalLength,
            float unitLength, int iterationsPerUnit = 4)
        {
            totalLength = 0f;

            var vertices = new List<VertexData>();
            if (controlPoints.Count < 4) return new VertexData[0];
            var segmentCount = (controlPoints.Count - 1) / 3;

            //Make sure that, the given unitLength is not greater than a segment min length, not sure why, I feel so.
            var minSegmentLength = float.MaxValue;
            for (var i = 0; i < segmentCount; i++)
            {
                var controlPointIndex = i * 3;
                var estimatedSegmentLength = EstimateCurveLength(controlPoints[controlPointIndex],
                    controlPoints[controlPointIndex + 1], controlPoints[controlPointIndex + 2],
                    controlPoints[controlPointIndex + 3]);
                minSegmentLength = Mathf.Min(minSegmentLength, estimatedSegmentLength);
            }

            unitLength = Mathf.Min(unitLength, minSegmentLength);

            var lastVertex = controlPoints[0];
            var lastPoint = controlPoints[0];
            for (var i = 0; i < segmentCount; i++)
            {
                var controlPointIndex = i * 3;

                var estimatedSegmentLength = EstimateCurveLength(controlPoints[controlPointIndex],
                    controlPoints[controlPointIndex + 1], controlPoints[controlPointIndex + 2],
                    controlPoints[controlPointIndex + 3]);
                var vertexNum = estimatedSegmentLength / unitLength;
                var iterationStepLength = 1f / (vertexNum * iterationsPerUnit);
                for (var t = 0f; t <= 1f; t += iterationStepLength)
                {
                    var p = GetSegmentPoint(controlPoints, i, t);
                    var sqrDistanceToLastVertex = Vector3.SqrMagnitude(lastVertex - p);
                    if (sqrDistanceToLastVertex > unitLength * unitLength)
                    {
                        var overshootDistance = Mathf.Sqrt(sqrDistanceToLastVertex) - unitLength;
                        var newVertex = p + (lastPoint - p).normalized * overshootDistance;
                        vertices.Add(new VertexData {Vertex = newVertex, T = (t + i) / segmentCount});
                        totalLength += Vector3.Distance(lastVertex, newVertex);
                        lastVertex = newVertex;
                    }

                    lastPoint = p;
                }
            }

            vertices.Add(new VertexData {Vertex = controlPoints[^1], T = 1f});
            totalLength += Vector3.Distance(lastVertex, controlPoints[^1]);
            return vertices.ToArray();
        }

        private static Vector3 GetSegmentPoint(IReadOnlyList<Vector3> controlPoints, int segmentIndex, float t)
        {
            var index = segmentIndex * 3;
            return Bezier.GetPoint(controlPoints[index], controlPoints[index + 1], controlPoints[index + 2],
                controlPoints[index + 3], t);
        }

        private static float EstimateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var controlNetLength = (p0 - p1).magnitude + (p1 - p2).magnitude + (p2 - p3).magnitude;
            var estimatedCurveLength = (p0 - p3).magnitude + controlNetLength / 2f;
            return estimatedCurveLength;
        }
    }
}