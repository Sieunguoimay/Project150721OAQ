using System.Linq;
using Curve;
using UnityEngine;

namespace Common.Curve
{
    public abstract class BezierSplineMono : MonoBehaviour
    {
        public abstract BezierSpline Spline { get; }
    }

    public abstract class BezierSplineModifiableMono : BezierSplineMono
    {
        public abstract BezierSplineModifiable SplineModifiable { get; }
    }
    
    public class BezierSplineCreator : BezierSplineModifiableMono
    {
        [SerializeField] private Vector3[] controlPoints;
        [SerializeField] private BezierPointMode[] modes;
        [SerializeField] private bool closed;

        public override BezierSplineModifiable SplineModifiable => _splineModifiable;
        
        private BezierSplineModifiable _splineModifiable;

        private void OnValidate()
        {
            _splineModifiable = new BezierSplineModifiable(modes, closed);
            SplineModifiable.SetControlPoints(controlPoints);
        }

#if UNITY_EDITOR

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
            _splineModifiable = new BezierSplineModifiable(modes, closed);
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

        public override BezierSpline Spline
        {
            get
            {
                if (SplineModifiable == null)
                {
                    OnValidate();
                }
                return SplineModifiable;
            }
        }

        public void SetClosed(bool close)
        {
            closed = close;
            SplineModifiable.SetClosed(close);
        }
    }
}