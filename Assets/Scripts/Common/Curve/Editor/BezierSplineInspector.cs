﻿using System;
using UnityEditor;
using UnityEngine;

namespace Curve
{
    [CustomEditor(typeof(BezierSplineCreator))]
    public class BezierSplineInspector : Editor
    {
        private BezierSplineCreator _spline;
        private Transform _handleTransform;
        private Quaternion _handleRotation;

        private int _selectedIndex = -1;

        private const int StepsPerCurve = 10;
        private const float DirectionScale = 0.5f;
        private const float HandleSize = 0.04f;
        private const float PickSize = 0.06f;

        private static readonly Color[] ModeColors =
        {
            Color.white,
            Color.yellow,
            Color.cyan
        };

        public override void OnInspectorGUI()
        {
            // DrawDefaultInspector();
            _spline = target as BezierSplineCreator;
            EditorGUI.BeginChangeCheck();
            var closed = EditorGUILayout.Toggle("Loop", _spline.Closed);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Toggle Loop");
                EditorUtility.SetDirty(_spline);
                _spline.SetClosed(closed);
            }

            if (_selectedIndex >= 0 && _selectedIndex < _spline.ControlPointCount)
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
            var point = EditorGUILayout.Vector3Field("Position", _spline.GetLocalControlPoint(_selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Move Point");
                EditorUtility.SetDirty(_spline);
                _spline.SetLocalControlPoint(_selectedIndex, point);
            }

            EditorGUI.BeginChangeCheck();
            var mode = (BezierPointMode) EditorGUILayout.EnumPopup("Mode",
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
            _spline = target as BezierSplineCreator;
            _handleTransform = _spline.transform;
            _handleRotation = Tools.pivotRotation == PivotRotation.Local
                ? _handleTransform.rotation
                : Quaternion.identity;

            var p0 = ShowPoint(0);
            for (var i = 1; i < _spline.ControlPointCount; i += 3)
            {
                var p1 = ShowPoint(i);
                var p2 = ShowPoint(i + 1);
                var p3 = ShowPoint(i + 2);

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
            var point = _spline.transform.TransformPoint(_spline.GetPoint(0f));
            Handles.DrawLine(point, point + _spline.transform.TransformVector(_spline.GetVelocity(0f).normalized * DirectionScale));
            var steps = StepsPerCurve * _spline.SegmentCount;
            for (var i = 1; i <= steps; i++)
            {
                point = _spline.transform.TransformPoint(_spline.GetPoint(i / (float) steps));
                Handles.DrawLine(point, point + _spline.transform.TransformVector(_spline.GetVelocity(i / (float) steps)).normalized * DirectionScale);
                Handles.DrawLine(point + Vector3.left * 0.03f, point + Vector3.right * 0.03f);
                Handles.DrawLine(point + Vector3.up * 0.03f, point + Vector3.down * 0.03f);
                Handles.DrawLine(point + Vector3.forward * 0.03f, point + Vector3.back * 0.03f);
            }
        }

        private Vector3 ShowPoint(int index)
        {
            var point = _handleTransform.TransformPoint(_spline.GetLocalControlPoint(index));
            var size = HandleUtility.GetHandleSize(point);
            if (index == 0)
            {
                size *= 2f;
            }

            Handles.color = ModeColors[(int) _spline.GetControlPointMode(index)];
            if (Handles.Button(point, _handleRotation, size * HandleSize, size * PickSize, Handles.DotHandleCap))
            {
                _selectedIndex = index;
                Repaint();
            }

            if (_selectedIndex == index)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point,
                    _handleRotation); //FreeMoveHandle(point, Quaternion.identity, 0.1f, Vector2.zero, Handles.CylinderHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Point Move");
                    EditorUtility.SetDirty(_spline);
                    _spline.SetLocalControlPoint(index, _handleTransform.InverseTransformPoint(point));
                }
            }

            return point;
        }
    }
}