using System;
using System.Collections.Generic;
using Common.Animation.ScriptingAnimation;
using UnityEngine;

namespace Gameplay.MagicFlower
{
    public class WavingStickAnimation : ScriptingAnimation
    {
        [SerializeField, Min(0)] private int cycle = 1;
        [SerializeField, Range(0f, 180f)] private float amplitude = 20f;

        private readonly List<Transform> _hierarchy = new();
        private readonly List<float> _defaultEuler = new();

        private void Start()
        {
            var current = Target;
            while (current.childCount > 0)
            {
                var child = current.GetChild(0);
                _hierarchy.Add(child);
                _defaultEuler.Add(child.localEulerAngles.z);
                current = child;
            }
        }

        protected override void OnTick(float p)
        {
            if (_hierarchy.Count == 0) return;
            var amp = Mathf.Lerp(amplitude, 0f, p);
            for (var i = 0; i < _hierarchy.Count; i++)
            {
                var sin = Mathf.Sin(p * Mathf.PI * 2f * cycle);
                var euler = _hierarchy[i].localEulerAngles;
                euler.z = _defaultEuler[i] + sin * amp;
                _hierarchy[i].localEulerAngles = euler;
            }
        }
    }
}