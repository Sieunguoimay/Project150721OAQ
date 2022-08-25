using System;
using Common;
using SNM;
using UnityEngine;

namespace CommonActivities
{
    public interface IEasing
    {
        float GetEase(float x);
    }

    public sealed class LinearEasing : IEasing
    {
        public float GetEase(float x)
        {
            return x;
        }
    }

    public abstract class EasingActivity : Activity
    {
        protected readonly IEasing Ease;

        protected EasingActivity(IEasing ease)
        {
            Ease = ease;
        }
    }

    public class StraightMove : EasingActivity
    {
        protected Vector3 Target { get; }
        private Vector3 _origin;
        protected Transform Transform { get; }

        private float _time;
        protected float Duration { get; set; }

        public StraightMove(Transform transform, Vector3 target, float duration, IEasing ease) : base(ease)
        {
            Transform = transform;
            Target = target;
            Duration = duration;
        }


        public override void Begin()
        {
            base.Begin();
            _time = 0;
            _origin = Transform.position;
            Transform.rotation = Quaternion.LookRotation(SNM.Math.Projection(Target - _origin, Vector3.up));
        }

        public override void Update(float deltaTime)
        {
            if (Inactive) return;

            _time += deltaTime;
            var t = Mathf.Min(_time / Duration, 1f);

            UpdatePosition(t);

            if (_time >= Duration)
            {
                NotifyDone();
            }
        }

        protected virtual void UpdatePosition(float t)
        {
            var pos = Vector3.Lerp(_origin, Target, Ease.GetEase(t));
            pos.y = Transform.position.y;
            Transform.position = pos;
        }
    }

    public class StraightMoveBySpeed : StraightMove
    {
        private readonly float _speed;

        protected StraightMoveBySpeed(Transform transform, Vector3 target, float speed, IEasing ease) : base(transform,
            target, 1f, ease)
        {
            _speed = speed;
        }

        public override void Begin()
        {
            Duration = Vector3.Distance(Target, Transform.position) / _speed;
            base.Begin();
            Debug.Log(Duration);
        }
    }


    public class JumpForward : StraightMove
    {
        private Vector3 _initialPosition;
        private Vector3 _initialVelocity;
        private Vector3 _initialAcceleration;
        private readonly float _height;
        private readonly IEasing _jumpEase;

        public JumpForward(Transform transform, Vector3 target, float duration, IEasing moveEase, float height,
            IEasing jumpEase) : base(transform, target, duration, moveEase)
        {
            _height = height;
            _jumpEase = jumpEase;
        }

        public override void Begin()
        {
            base.Begin();
            var pos = Transform.position;
            var distance = Vector3.Distance(Target, pos);
            var h = Mathf.Clamp(distance, _height / 2f, _height);
            var t = Duration;
            var a = (-8f * h) / (t * t);

            _initialPosition = pos;
            _initialAcceleration = Vector3.up * a;
            _initialVelocity = Vector3.up * (-a * 0.5f * Duration);
        }

        protected override void UpdatePosition(float t)
        {
            var xz = Transform.position;
            var y = SNM.Math.MotionEquation(
                _initialPosition, _initialVelocity,
                _initialAcceleration, _jumpEase.GetEase(t) * Duration);
            Transform.position = new Vector3(xz.x, y.y, xz.z);
            base.UpdatePosition(t);
        }
    }

    public class Delay : Activity
    {
        private float _duration;
        private readonly Func<float> _onBegin;
        protected float Time { get; private set; } = 0;

        public Delay(float duration)
        {
            _duration = duration;
        }

        public Delay(Func<float> onBegin)
        {
            _onBegin = onBegin;
        }

        public override void Begin()
        {
            base.Begin();
            _duration = _onBegin?.Invoke() ?? _duration;
        }

        public override void Update(float deltaTime)
        {
            Time += deltaTime;
            if (Time >= _duration)
            {
                NotifyDone();
            }
        }
    }

    public class Timer : Delay
    {
        private readonly Action<float> _onTick;

        public Timer(float duration, Action<float> onTick) : base(duration)
        {
            _onTick = onTick;
        }

        public override void Update(float deltaTime)
        {
            _onTick?.Invoke(Time);
            base.Update(deltaTime);
        }
    }

    public class Lambda : Activity
    {
        private readonly Action _onBegin;
        private readonly Func<bool> _onUpdate;

        public Lambda(Action onBegin)
        {
            _onBegin = onBegin;
        }

        public Lambda(Action onBegin, Func<bool> onUpdate)
        {
            _onBegin = onBegin;
            _onUpdate = onUpdate;
        }

        public override void Begin()
        {
            base.Begin();
            _onBegin?.Invoke();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            if (!Inactive && (_onUpdate?.Invoke() ?? false))
            {
                NotifyDone();
            }
        }
    }
}