using Common.Curve;
using Common.Curve.Editor;
using UnityEditor;
using UnityEngine;

namespace Common.Animation.ScriptingAnimation.Editor
{
    [CustomEditor(typeof(BezierAnimation))]
    public class BezierAnimationEditor : BezierSplineInspector
    {
        private BezierSplineWithDistance _splineWidthDistance;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            base.OnInspectorGUI();
            
            if (_splineWidthDistance == null)
            {
                _splineWidthDistance = new BezierSplineWithDistance(_spline.SplineModifiable,( (BezierAnimation) _spline).VertexDistance);
            }
            GUILayout.Label($"Vertex Num: {_splineWidthDistance.Vertices.Count}");
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (_splineWidthDistance == null)
            {
                _splineWidthDistance = new BezierSplineWithDistance(_spline.SplineModifiable,( (BezierAnimation) _spline).VertexDistance);
            }

            Handles.color = Color.red;

            foreach (var v in _splineWidthDistance.Vertices)
            {
                var point = _spline.Transform.TransformPoint(v.Vertex);
                Handles.DrawWireCube(point,Vector3.one*.01f);
            }
        }
    }
}