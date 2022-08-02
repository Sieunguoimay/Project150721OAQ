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
        private readonly Vector3 _target;
        private Vector3 _origin;
        private readonly float _duration;
        private readonly Transform _transform;

        private float _time;

        public StraightMove(Transform transform, Vector3 target, float duration, IEasing ease) : base(ease)
        {
            _transform = transform;
            _target = target;
            _duration = duration;
        }

        public override void Begin()
        {
            base.Begin();
            _time = 0;
            _origin = _transform.position;
            _transform.rotation = Quaternion.LookRotation(SNM.Math.Projection(_target - _origin, Vector3.up));
        }

        public override void Update(float deltaTime)
        {
            if (IsDone) return;

            _time += deltaTime;
            var t = Mathf.Min(_time / _duration, 1f);
            var pos = Vector3.Lerp(_origin, _target, Ease.GetEase(t));
            pos.y = _transform.position.y;
            _transform.position = pos;
            if (_time >= _duration)
            {
                IsDone = true;
            }
        }
    }

    public class Delay : Activity
    {
        private readonly float _duration;
        private float _time = 0;

        public Delay(float duration)
        {
            _duration = duration;
        }

        public override void Update(float deltaTime)
        {
            _time += deltaTime;
            if (_time >= _duration)
            {
                IsDone = true;
            }
        }
    }
}