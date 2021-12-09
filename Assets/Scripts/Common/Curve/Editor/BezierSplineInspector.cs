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
            EditorGUI.BeginChangeCheck();
            var closed = EditorGUILayout.Toggle("Loop", _spline.Closed);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Toggle Loop");
                EditorUtility.SetDirty(_spline);
                _spline.Closed = closed;
            }

            if (_selectedIndex >= 0 && _selectedIndex < _spline.PointCount)
            {
                DrawSelectedPointInspector();
            }

            if (GUILayout.Button("Add Curve"))
            {
                Undo.RecordObject(_spline, "Add Curve");
                _spline.AddSegment();
                EditorUtility.SetDirty(_spline);
            }
        }

        private void DrawSelectedPointInspector()
        {
            GUILayout.Label("Selected Point");
            EditorGUI.BeginChangeCheck();
            var point = EditorGUILayout.Vector3Field("Position", _spline.GetPoint(_selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Move Point");
                EditorUtility.SetDirty(_spline);
                _spline.SetPoint(_selectedIndex, point);
            }

            EditorGUI.BeginChangeCheck();
            var mode = (BezierPointMode) EditorGUILayout.EnumPopup("Mode",
                _spline.GetPointMode(_selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Change Point Mode");
                _spline.SetPointMode(_selectedIndex, mode);
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
            for (int i = 1; i < _spline.PointCount; i += 3)
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
            var point = _spline.GetPosition(0f);
            Handles.DrawLine(point, point + _spline.GetDirection(0f) * directionScale);
            var steps = stepsPerCurve * _spline.SegmentCount;
            for (int i = 1; i <= steps; i++)
            {
                point = _spline.GetPosition(i / (float) steps);
                Handles.DrawLine(point, point + _spline.GetDirection(i / (float) steps) * directionScale);
            }
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = _handleTransform.TransformPoint(_spline.GetPoint(index));
            float size = HandleUtility.GetHandleSize(point);
            if (index == 0)
            {
                size *= 2f;
            }

            Handles.color = modeColors[(int) _spline.GetPointMode(index)];
            if (Handles.Button(point, _handleRotation, size * handleSize, size * pickSize, Handles.DotCap))
            {
                _selectedIndex = index;
                Repaint();
            }

            if (_selectedIndex == index)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, _handleRotation); //FreeMoveHandle(point, Quaternion.identity, 0.1f, Vector2.zero, Handles.CylinderHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Point Move");
                    EditorUtility.SetDirty(_spline);
                    _spline.SetPoint(index, _handleTransform.InverseTransformPoint(point));
                }
            }

            return point;
        }
    }
}