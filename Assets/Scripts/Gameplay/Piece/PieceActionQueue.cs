using System;
using Common;
using CommonActivities;
using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Gameplay.Piece
{
    public class PieceActivityQueue : ActivityQueue
    {
        [Serializable]
        public struct ConfigData
        {
            public float angularSpeed;
        }

        public ConfigData Config { get; } = new() {angularSpeed = 270f};

        public class Jump : EasingActivity
        {
            private readonly InputData _inputData;
            private Vector3 _initialPosition;
            private Vector3 _initialVelocity;
            private Vector3 _initialAcceleration;
            private float _time;
            private float _duration;
            private readonly Transform _transform;

            public Jump(Transform transform, InputData inputData, IEasing easing) : base(easing)
            {
                _transform = transform;
                _inputData = inputData;
            }

            public override void Begin()
            {
                var h = _inputData.height;
                var t = _inputData.duration;
                var pos = _transform.position;
                var a = (-8f * h) / (t * t);

                _time = 0;
                Inactive = false;
                _duration = t;
                _initialPosition = pos;
                _initialAcceleration = Vector3.up * a;
                _initialVelocity = Vector3.up * (-a * 0.5f * _duration);
            }

            public override void Update(float deltaTime)
            {
                if (Inactive) return;

                _time += deltaTime;

                var t = Mathf.Min(_time / _duration, 1f);

                var xz = _transform.position;
                var y = SNM.Math.MotionEquation(
                    _initialPosition, _initialVelocity,
                    _initialAcceleration, Ease.GetEase(t) * _duration);
                _transform.position = new Vector3(xz.x, y.y, xz.z);

                if (_time >= _duration)
                {
                    Inactive = true;
                }
            }

            public override void End()
            {
            }

            public float GetJumpDistance(float movingSpeed) => _duration * movingSpeed;

            public class InputData
            {
                public float height = 1f;
                public float duration = 0.4f;
            }
        }

        public class TurnAway : Activity
        {
            private readonly Transform _transform;

            public TurnAway(Transform transform)
            {
                _transform = transform;
            }

            public override void Begin()
            {
                base.Begin();

                var lr = _transform.localEulerAngles;
                lr.y += UnityEngine.Random.Range(-60f, 60f);
                _transform.DOLocalRotate(lr, 1f).SetLink(_transform.gameObject)
                    .OnComplete(() => { Inactive = true; });
            }

            public override void Update(float deltaTime)
            {
            }

            public override void End()
            {
                base.End();
                DOTween.Kill(this);
            }
        }

        public class BounceAnim : Activity
        {
            private readonly Transform _transform;
            private readonly float _duration;
            private float _time;
            private readonly float _offset;
            private readonly bool _fullPhase;

            public BounceAnim(Transform transform, float duration, bool fullPhase = false)
            {
                _transform = transform;
                _duration = duration;
                _offset = 0.3f;
                _fullPhase = fullPhase;
            }

            public override void Update(float deltaTime)
            {
                if (Inactive) return;

                _time += deltaTime;
                var t = Mathf.Min(_time / _duration, 1f);

                var scale = _transform.localScale;
                if (_fullPhase)
                {
                    var s = Mathf.Sin(Mathf.Lerp(0, Mathf.PI * 2f, t));
                    scale.y = 1 + (-s) * _offset;
                    scale.x = 1 + (s) * _offset * 0.35f;
                    scale.z = 1 + (s) * _offset * 0.35f;
                }
                else
                {
                    var c = Mathf.Cos(Mathf.Lerp(0, Mathf.PI * 2f, t));
                    scale.y = 1 + (c) * _offset * 0.5f;
                    scale.x = 1 + (-c) * _offset * 0.25f;
                    scale.z = 1 + (-c) * _offset * 0.25f;
                }

                _transform.localScale = scale;

                if (_time >= _duration)
                {
                    Inactive = true;
                }
            }
        }
    }
}