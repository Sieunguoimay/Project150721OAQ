using System;
using UnityEditor;
using UnityEngine;

namespace Curve
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineInspector : Editor
    {
        private BezierSpline _spline;
        private Transform _handleTransform;
        private Quaternion _handleRotation;

        private int _selectedIndex = -1;

        private const int stepsPerCurve = 10;
        private const float directionScale = 0.5f;
        private const float handleSize = 0.04f;
        private const float pickSize = 0.06f;

        private static Color[] modeColors =
        {
            Color.white,
            Color.yellow,
            Color.cyan
        };

        public override void OnInspectorGUI()
        {
            // DrawDefaultInspector();
            _spline = target as BezierSpline;
            if (_selectedIndex >= 0 && _selectedIndex < _spline.ControlPointCount)
            {
                DrawSelectedPointInspector();
            }

            if (GUILayout.Button("Add Curve"))
            {
                Undo.RecordObject(_spline, "Add Curve");
                _spline.AddCurve();
                EditorUtility.SetDirty(_spline);
            }
        }

        private void DrawSelectedPointInspector()
        {
            GUILayout.Label("Selected Point");
            EditorGUI.BeginChangeCheck();
            var point = EditorGUILayout.Vector3Field("Position", _spline.GetControlPoint(_selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Move Point");
                EditorUtility.SetDirty(_spline);
                _spline.SetControlPoint(_selectedIndex, point);
            }

            EditorGUI.BeginChangeCheck();
            var mode = (BezierControlPointMode) EditorGUILayout.EnumPopup("Mode",
                _spline.GetControlPointMode(_selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Change Point Mode");
                _spline.SetControlPointMode(_selectedIndex, mode);
                EditorUtility.SetDirty(_spline);
            }
        }

        private void OnSceneGUI()
        {
            _spline = target as BezierSpline;
            _handleTransform = _spline.transform;
            _handleRotation = Tools.pivotRotation == PivotRotation.Local
                ? _handleTransform.rotation
                : Quaternion.identity;

            Vector3 p0 = ShowPoint(0);
            for (int i = 1; i < _spline.ControlPointCount; i += 3)
            {
                Vector3 p1 = ShowPoint(i);
                Vector3 p2 = ShowPoint(i + 1);
                Vector3 p3 = ShowPoint(i + 2);

                Handles.color = Color.gray;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);

                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
                p0 = p3;
            }

            ShowDirections();
        }

        private void ShowDirections()
        {
            Handles.color = Color.green;
            var point = _spline.GetPoint(0f);
            Handles.DrawLine(point, point + _spline.GetDirection(0f) * directionScale);
            var steps = stepsPerCurve * _spline.CurveCount;
            for (int i = 1; i <= steps; i++)
            {
                point = _spline.GetPoint(i / (float) steps);
                Handles.DrawLine(point, point + _spline.GetDirection(i / (float) steps) * directionScale);
            }
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = _handleTransform.TransformPoint(_spline.GetControlPoint(index));
            float size = HandleUtility.GetHandleSize(point);
            Handles.color = modeColors[(int)_spline.GetControlPointMode(index)];
            if (Handles.Button(point, _handleRotation, size * handleSize, size * pickSize, Handles.DotCap))
            {
                _selectedIndex = index;
                Repaint();
            }

            if (_selectedIndex == index)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, _handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Point Move");
                    EditorUtility.SetDirty(_spline);
                    _spline.SetControlPoint(index, _handleTransform.InverseTransformPoint(point));
                }
            }

            return point;
        }
    }
}