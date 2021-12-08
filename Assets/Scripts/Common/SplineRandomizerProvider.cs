using System;
using Curve;
using UnityEditor;
using UnityEngine;

namespace Common
{
    public class SplineRandomizerProvider : MonoBehaviour
    {
        [SerializeField] private SplineRandomizer.Config config;
        [SerializeField] private BezierSpline spline;
        [SerializeField] public Vector3 startPoint;
        [SerializeField] public Vector3 endPoint;

        private SplineRandomizer _randomizer;

        public SplineRandomizer CreateRandomizer(BezierSpline spline)
        {
            return new SplineRandomizer(config, spline);
        }

        [ContextMenu("Randomize")]
        public void Randomize()
        {
            if (_randomizer == null)
            {
                _randomizer = CreateRandomizer(spline);
            }

            _randomizer.Randomize(startPoint, endPoint);
        }
    }

    [CustomEditor(typeof(SplineRandomizerProvider))]
    public class SplineRandomizerProviderEditor : Editor
    {
        private SplineRandomizerProvider _provider;

        private void OnSceneGUI()
        {
            _provider = target as SplineRandomizerProvider;

            var handleRotation = Tools.pivotRotation == PivotRotation.Local ? _provider.transform.rotation : Quaternion.identity;
            Handles.DrawDottedLine(_provider.startPoint, _provider.endPoint, 5f);
            EditorGUI.BeginChangeCheck();
            var point = Handles.DoPositionHandle(_provider.startPoint, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_provider, "Move Point");
                _provider.startPoint = point;
                EditorUtility.SetDirty(_provider);
            }

            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(_provider.endPoint, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_provider, "Move Point");
                _provider.endPoint = point;
                EditorUtility.SetDirty(_provider);
            }
        }
    }
}