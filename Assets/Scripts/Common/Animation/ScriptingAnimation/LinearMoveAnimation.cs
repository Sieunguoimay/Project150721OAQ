using UnityEngine;

namespace Common.Animation.ScriptingAnimation
{
    public class LinearMoveAnimation : ScriptingAnimation, IAnimationMover
    {
        [SerializeField] private Vector3[] points;

        protected override IAnimationMover GetMover(Transform target)
        {
            return this;
        }

        public void Move(float progress)
        {
            if (points.Length < 2) return;
            if (points.Length == 2)
            {
                Target.position = transform.TransformPoint(Vector3.Lerp(points[0], points[1], progress));
            }

            else
            {
                var index = Mathf.FloorToInt(progress * (points.Length - 1));
                Target.position = transform.TransformPoint(Vector3.Lerp(points[index], points[index + 1], progress));
            }
        }
    }
}