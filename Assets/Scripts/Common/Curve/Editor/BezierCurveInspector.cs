using System;
using UnityEditor;
using UnityEngine;

namespace Curve
{
    [CustomEditor(typeof(BezierCurve))]
    public class BezierCurveInspector : Editor
    {
        private BezierCurve _curve;
        private Transform _handleTransform;
        private Quaternion _handleRotation;

        private const int lineSteps = 10;

        private void OnSceneGUI()
        {
            _curve = target as BezierCurve;
            _handleTransform = _curve.transform;
            _handleRotation = Tools.pivotRotation == PivotRotation.Local ? _handleTransform.rotation : Quaternion.identity;

            Vector3 p0 = ShowPoint(0);
            Vector3 p1 = ShowPoint(1);
            Vector3 p2 = ShowPoint(2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p1, p2);

            Handles.color = Color.white;
            var lineStart = _curve.GetPoint(0f);
            for (int i = 1; i <= lineSteps; i++)
            {
                var lineEnd = _curve.GetPoint(i / (float) lineSteps);
                Handles.DrawLine(lineStart, lineEnd);
                lineStart = lineEnd;
            }
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = _handleTransform.TransformPoint(_curve.points[index]);
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, _handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_curve, "Point Move");
                EditorUtility.SetDirty(_curve);
                _curve.points[index] = _handleTransform.InverseTransformPoint(point);
            }

            return point;
        }
    }
}