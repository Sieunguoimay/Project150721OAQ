using System;
using UnityEditor;
using UnityEngine;

namespace InGame.Common.Editor
{
    [CustomEditor(typeof(Path))]
    public class PathInspector : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var path = target as Path;
            var handleTransform = path.transform;
            var handleRotation = Tools.pivotRotation == PivotRotation.Local
                ? handleTransform.rotation
                : Quaternion.identity;

            var n = path.points.Count;
            if (n > 1)
            {
                for (int i = 0; i < n - 1; i++)
                {
                    var p0 = handleTransform.TransformPoint(path.points[i]);
                    var p1 = handleTransform.TransformPoint(path.points[i + 1]);
                    Handles.color = Color.white;
                    Handles.DrawLine(p0, p1);

                    path.points[i] = DrawHandle(path.points[i], handleRotation, handleTransform);
                    path.points[i + 1] = DrawHandle(path.points[i + 1], handleRotation, handleTransform);
                }
            }
            else if (n > 0)
            {
                path.points[0] = DrawHandle(path.points[0], handleRotation, handleTransform);
            }
        }

        private Vector3 DrawHandle(Vector3 p, Quaternion handleRotation, Transform handleTransform)
        {
            Vector3 p0 = handleTransform.TransformPoint(p);

            EditorGUI.BeginChangeCheck();
            p0 = Handles.DoPositionHandle(p0, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move Point");
                EditorUtility.SetDirty(target);
                return handleTransform.InverseTransformPoint(p0);
            }

            return p;
        }
    }
}