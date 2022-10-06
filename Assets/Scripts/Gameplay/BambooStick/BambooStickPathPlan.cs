using System.Linq;
using Common.Curve;
using UnityEngine;

namespace Gameplay.BambooStick
{
    public class BambooStickPathPlan : MonoBehaviour
    {
        [SerializeField] private BezierSplineModifiableMono splineMono;

        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        
        [SerializeField, Min(0f)] private float endPointFactor;

        [ContextMenu("Plan")]
        public void PlanPath()
        {
            var controlPoints = splineMono.SplineModifiable.ControlPoints;
            
            if (controlPoints.Count < 7)
            {
                Debug.LogError("Expect 7 control points , i.e 3 bezier curve segments");
                return;
            }

            var startPos = startPoint.position;
            var endPos = endPoint.position;

            SetPoint(0, startPos);
            SetPoint(1, startPos + startPoint.forward * endPointFactor);
            SetPoint(6, endPos);
            SetPoint(5, endPos + endPoint.forward * endPointFactor);
            SetPoint(3, (controlPoints[1] + controlPoints[5]) / 2f);
            SetPoint(2, controlPoints[3] + (controlPoints[1] - controlPoints[5]) / 4f);
            SetPoint(4, controlPoints[3] - (controlPoints[1] - controlPoints[5]) / 4f);
        }

        private void SetPoint(int index, Vector3 point)
        {
            splineMono.SplineModifiable.SetControlPoint(index, splineMono.transform.InverseTransformPoint(point));
        }
    }
}