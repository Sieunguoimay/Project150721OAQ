using System.Linq;
using Common.Curve;
using Common.Curve.Mover;
using UnityEngine;

namespace Common.Animation.ScriptingAnimation
{
    public class BezierAnimation : MovingAnimation, ISplineCreator
    {
        [SerializeField] private Vector3[] controlPoints;
        [SerializeField] private bool faceMoveDirection;
        [field: SerializeField] public bool WorldSpace { get; private set; }
        [field: SerializeField, Min(0.01f)] public float VertexDistance { get; private set; } = 0.1f;

        public BezierSplineModifiable SplineModifiable
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

        private IAnimationMover _mover;

        private void OnValidate()
        {
            if (_splineModifiable != null && controlPoints.Length == _splineModifiable.ControlPoints.Count) return;

            _splineModifiable = new BezierSplineModifiable(false);
            var modes = new BezierPointMode[controlPoints.Length / 2];
            for (var i = 0; i < modes.Length; i++)
            {
                modes[i] = BezierPointMode.Free;
            }

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
            var modes = new[] {BezierPointMode.Free, BezierPointMode.Free};
            _splineModifiable = new BezierSplineModifiable(false);
            SplineModifiable.SetModesAndControlPoints(modes, controlPoints);
        }

#endif

        public Transform Transform => transform;
        public Object SerializeObject => this;

        public void SetClosed(bool close)
        {
        }

        public void AddSegment()
        {
            SplineModifiable.AddSegment(1, BezierPointMode.Free);
            SaveToSerializedField();

            void SaveToSerializedField()
            {
                controlPoints = SplineModifiable.ControlPoints.ToArray();
            }
        }

        protected override IAnimationMover GetMover(Transform target)
        {
            if (_mover == null)
            {
                _mover = new BezierMoverByDistance(this, target, SplineModifiable);
            }

            return _mover;
        }

        private class BezierMoverByDistance : BaseAnimationMover
        {
            private readonly BezierAnimation _animation;
            private readonly BezierSplineWithDistance _splineWithDistance;
            private readonly Vector3 _positionOrigin;
            private readonly Quaternion _rotationOrigin;

            public BezierMoverByDistance(BezierAnimation animation, Transform target, BezierSpline spline) : base(target)
            {
                _animation = animation;
                _splineWithDistance = new BezierSplineWithDistance(spline, animation.VertexDistance);
                _positionOrigin = _animation.transform.position;
                _rotationOrigin = _animation.transform.rotation;
            }

            public sealed override void Move(float progress)
            {
                var t = _splineWithDistance.GetTAtDistance(progress * _splineWithDistance.ArcLength);
                Target.position = _animation.WorldSpace
                    ? _positionOrigin + _splineWithDistance.Spline.GetPoint(t)
                    : _animation.transform.TransformPoint(_splineWithDistance.Spline.GetPoint(t));

                if (_animation.faceMoveDirection)  
                {
                    Target.rotation = Quaternion.LookRotation(
                        _animation.WorldSpace
                            ? _rotationOrigin * _splineWithDistance.Spline.GetVelocity(t)
                            : _animation.transform.TransformDirection(_splineWithDistance.Spline.GetVelocity(t)), Vector3.right);
                }
            }
        }
    }
}