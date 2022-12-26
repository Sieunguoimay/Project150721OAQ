﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common.Animation.ScriptingAnimation;
using Common.Animation.ScriptingAnimation.ShortAnimations;
using Common.Curve;
using Common.Curve.PathCreator.Core.Runtime.Utility;
using Common.Misc;
using Common.UnityExtend;
using Common.UnityExtend.Attribute;
using SNM;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Gameplay.MagicFlower
{
    public class MagicFlowerSpace : MonoBehaviour
    {
        [SerializeField, Min(.1f)] private float spacing = .1f;
        [SerializeField] private Vector2Int tableSize = Vector2Int.one;
        [SerializeField] private GameObject prefab;

        [SerializeField, TypeConstraint(typeof(ISplineCreator), typeof(ScriptingAnimation))]
        private Object bezierSpline;

        [SerializeField] private AngleAnimation angleAnimation;
        [field: SerializeField] public UnityEvent OnCollectBlossom { get; private set; }

        private int _slotIndex;

        private readonly List<Vector2Int> _availableSlots = new();

        private readonly Dictionary<GameObject, Vector2Int> _blossomMap = new();

        [field: NonSerialized] public Vector3 LastCollectedBlossomPosition { get; private set; }

        private void Start()
        {
            CreateSlots();
        }

        private void CreateSlots()
        {
            for (var i = 0; i < tableSize.x; i++)
            {
                for (var j = 0; j < tableSize.y; j++)
                {
                    _availableSlots.Add(new Vector2Int(i, j));
                }
            }
        }

        public void SpawnRandom(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                _slotIndex = Random.Range(0, _availableSlots.Count);
                Spawn();
            }
        }

        public void PrepareLandingSpot(Transform startSpot)
        {
            TransformUtility.CopyWorldScale(prefab.transform, startSpot);
            prefab.transform.rotation = startSpot.rotation;

            //Handle overflow
            if (_availableSlots.Count == 0)
            {
                _availableSlots.Add(new Vector2Int(0, 0));
                Debug.LogError("HU HU overflow, please increase space size");
            }

            _slotIndex = Random.Range(0, _availableSlots.Count);

            var endPos = GetCell(_availableSlots[_slotIndex].x, _availableSlots[_slotIndex].y);
            var startPos = startSpot.position;
            var splineTransform = (bezierSpline as ISplineCreator)?.Transform;
            var spline = (bezierSpline as ISplineCreator)?.SplineModifiable;
            if (spline == null || splineTransform == null) return;

            spline.SetControlPoint(spline.ControlPoints.Count - 1, splineTransform.InverseTransformPoint(endPos));
            spline.SetControlPoint(0, splineTransform.InverseTransformPoint(startPos));

            var localControlMidPoint =
                transform.InverseTransformPoint(
                    splineTransform.TransformPoint(spline.ControlPoints[spline.ControlPoints.Count / 2]));
            var localControlFirstPoint =
                transform.InverseTransformPoint(splineTransform.TransformPoint(spline.ControlPoints[0]));
            var localStartPoint = transform.InverseTransformPoint(startPos);
            var localMidPos = transform.InverseTransformPoint((endPos + startPos) / 2);
            var newLocalControlMidPoint = new Vector3(localMidPos.x,
                localStartPoint.y + (localControlMidPoint.y - localControlFirstPoint.y), localMidPos.z);

            spline.SetControlPoint(spline.ControlPoints.Count / 2,
                splineTransform.InverseTransformPoint(transform.TransformPoint(newLocalControlMidPoint)));

            var keys = angleAnimation.GetCurve(0).keys;
            keys[^1].value = Random.Range(-20f, 20f);
            keys[^1].inTangent = 0f;
            angleAnimation.GetCurve(0).keys = keys;

            keys = angleAnimation.GetCurve(1).keys;
            keys[^1].value += Random.Range(0f, 180f);
            angleAnimation.GetCurve(1).keys = keys;
        }

        public void Spawn()
        {
            var blossom = Instantiate(prefab, transform);
            blossom.transform.position = GetCell(_availableSlots[_slotIndex].x, _availableSlots[_slotIndex].y);
            _blossomMap.Add(blossom, _availableSlots[_slotIndex]);
            _availableSlots.RemoveAt(_slotIndex);
            blossom.GetComponent<ABoundsClicker>().Clicked += OnBlossomClicked;
        }

        public void Collect(GameObject blossom)
        {
            if (_blossomMap.TryGetValue(blossom, out var slot))
            {
                _availableSlots.Add(slot);
                LastCollectedBlossomPosition = blossom.transform.position;
                Destroy(blossom);
                OnCollectBlossom?.Invoke();
            }
        }


        private void OnBlossomClicked(EventArgs eventArgs)
        {
            Collect((eventArgs as ABoundsClicker.ClickedEventArgs)?.Sender.gameObject);
        }

        private Vector3 GetCell(int x, int y)
        {
            var left = -tableSize.x / 2f * spacing;
            var bottom = -tableSize.y / 2f * spacing;
            return transform.TransformPoint(new Vector3(left + x * spacing, 0, bottom + y * spacing));
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            var left = -tableSize.x / 2f * spacing;
            var bottom = -tableSize.y / 2f * spacing;
            // for (var i = 0; i < tableSize.x; i++)
            // {
            //     for (var j = 0; j < tableSize.y; j++)
            //     {
            //         Gizmos.DrawWireCube(Vector3.zero + new Vector3(left + i * spacing, 0, bottom + j * spacing),
            //             Vector3.one * spacing);
            //     }
            // }

            foreach (var t in _availableSlots)
            {
                var i = t.x;
                var j = t.y;
                Gizmos.DrawWireCube(Vector3.zero + new Vector3(left + i * spacing, 0, bottom + j * spacing),
                    Vector3.one * spacing);
            }
        }
    }
}