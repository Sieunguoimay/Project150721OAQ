using System.Linq;
using Curve;
using UnityEngine;

namespace Common.Curve
{
    public class BezierSplineCreator : MonoBehaviour
    {
        [SerializeField] private Vector3[] controlPoints;
        [SerializeField] private BezierPointMode[] modes;
        [SerializeField] private bool closed;

        public BezierSplineModifiable SplineModifiable { get; private set; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SplineModifiable = new BezierSplineModifiable(modes, closed);
            SplineModifiable.SetControlPoints(controlPoints);
        }

        public void Reset()
        {
            controlPoints = new[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                new Vector3(4f, 0f, 0f),
            };
            modes = new[] {BezierPointMode.Free, BezierPointMode.Free};
            SplineModifiable = new BezierSplineModifiable(modes, closed);
            SplineModifiable.SetControlPoints(controlPoints);
        }

        public void AddSegment()
        {
            SplineModifiable.AddSegment(1);
            SaveToSerializedField();
        }
#endif
        private void SaveToSerializedField()
        {
            controlPoints = SplineModifiable.ControlPoints.ToArray();
            modes = SplineModifiable.Modes.ToArray();
            closed = SplineModifiable.Closed;
        }

        public BezierSpline Spline => SplineModifiable;

        public void SetClosed(bool close)
        {
            closed = close;
            SplineModifiable.SetClosed(close);
        }
    }
}