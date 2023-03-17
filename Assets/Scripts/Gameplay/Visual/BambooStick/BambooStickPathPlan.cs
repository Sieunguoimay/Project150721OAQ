using Common.Curve;
using UnityEngine;

namespace Gameplay.Visual.BambooStick
{
    public class BambooStickPathPlan : MonoBehaviour
    {
        [SerializeField] private BezierSplineModifiableMono splineMono;

        [SerializeField, Min(0f)] private float endPointFactor;

        public void PlanPath(Vector3 startPos, Vector3 startForward, Vector3 endPos, Vector3 endForward)
        {
            var controlPoints = splineMono.SplineModifiable.ControlPoints;

            if (controlPoints.Count < 7)
            {
                Debug.LogError("Expect 7 control points , i.e 3 bezier curve segments");
                return;
            }

            var point1 = startPos + startForward * endPointFactor;
            var point5 = endPos + endForward * endPointFactor;
            var point3 = (point1 + point5) / 2f;

            SetPoint(0, startPos);
            SetPoint(1, point1);
            SetPoint(6, endPos);
            SetPoint(5, point5);
            SetPoint(3, point3);
            SetPoint(2, point3 + (point1 - point5) / 4f);
            SetPoint(4, point3 - (point1 - point5) / 4f);
        }

        private void SetPoint(int index, Vector3 point)
        {
            splineMono.SplineModifiable.SetControlPoint(index, splineMono.transform.InverseTransformPoint(point));
        }
    }
}