using System.Linq;
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

    public interface ISplineCreator
    {
        BezierSplineModifiable SplineModifiable { get; }
        Transform Transform { get; }
        UnityEngine.Object SerializeObject { get; }
        void SetClosed(bool close);
        void AddSegment();
    }

    public class BezierSplineCreator : BezierSplineModifiableMono, ISplineCreator
    {
        [SerializeField] private Vector3[] controlPoints;
        [SerializeField] private BezierPointMode[] modes;
        [SerializeField] private bool closed;

        public override BezierSplineModifiable SplineModifiable
        {
            get
            {
                if (_splineModifiable == null)
                {
                    OnValidate();
                }

                return _splineModifiable;
            }
        }

        private BezierSplineModifiable _splineModifiable;

        private void OnValidate()
        {
            _splineModifiable = new BezierSplineModifiable(closed);
            SplineModifiable.SetModesAndControlPoints(modes, controlPoints);
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
            _splineModifiable = new BezierSplineModifiable(closed);
            SplineModifiable.SetModesAndControlPoints(modes, controlPoints);
        }

#endif

        public override BezierSpline Spline => SplineModifiable;
        public Transform Transform => transform;
        public Object SerializeObject => this;

        public void SetClosed(bool close)
        {
            SplineModifiable.SetClosed(close);
            closed = close;
        }

        public void AddSegment()
        {
            SplineModifiable.AddSegment(1, BezierPointMode.Free);
            SaveToSerializedField();

            void SaveToSerializedField()
            {
                controlPoints = SplineModifiable.ControlPoints.ToArray();
                modes = SplineModifiable.Modes.ToArray();
                closed = SplineModifiable.Closed;
            }
        }
    }
}