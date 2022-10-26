using UnityEngine;

namespace Common.Curve.Mover
{
    public abstract class ABezierMover : MonoBehaviour
    {
        [SerializeField] protected BezierSplineMono bezierSpline;

        public BezierSplineWithDistance SplineWithDistance { get; protected set; }

        public float Displacement { get; private set; }

        public float SetToDisplacement(float displacement)
        {
            Displacement = displacement;

            var t = SplineWithDistance.GetTAtDistance(displacement);

            Transform tr;

            var globalPosition = (tr = bezierSpline.transform).TransformPoint(SplineWithDistance.Spline.GetPoint(t));
            var globalRotation = tr.rotation * Quaternion.LookRotation(SplineWithDistance.Spline.GetVelocity(t));

            var tr2 = transform;

            tr2.position = globalPosition;
            tr2.rotation = globalRotation;

            return t;
        }
    }

    public class BezierMover : ABezierMover
    {
        [SerializeField] private BezierSplineCreator initialPath;
        [SerializeField, Min(0.01f)] private float speed = 1f;
        [SerializeField] private bool loop;

        private bool _moving;
        private BezierSpline Spline => initialPath.Spline;

        private void Awake()
        {
            SplineWithDistance = new BezierSplineWithDistance(Spline);
        }

        [ContextMenu("Move")]
        private void Move() => Move(0f);

        public void Move(float startDisplacement)
        {
            _moving = true;
            SetToDisplacement(startDisplacement);
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            _moving = false;
        }

        private void Update()
        {
            var displacement = Displacement;

            if (loop && displacement >= SplineWithDistance.ArcLength)
            {
                displacement -= SplineWithDistance.ArcLength;
            }

            if (!_moving || Spline == null || displacement >= SplineWithDistance.ArcLength) return;

            displacement += Time.deltaTime * speed;

            SetToDisplacement(displacement);
        }
    }
}