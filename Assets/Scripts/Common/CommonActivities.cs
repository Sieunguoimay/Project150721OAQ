using SNM;
using UnityEngine;

namespace CommonActivities
{
    public class StraightMove : Activity
    {
        private readonly Vector3 _target;
        private Vector3 _origin;
        private readonly float _duration;
        private readonly Transform _transform;

        private float _time;

        public StraightMove(Transform transform, Vector3 target, float duration, IEasing ease = null)
        {
            _transform = transform;
            _target = target;
            _duration = duration;
            if (ease != null)
            {
                SetEase(ease);
            }
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
            if (!IsDone)
            {
                _time += deltaTime;
                float t = Mathf.Min(_time / _duration, 1f);
                var pos = Vector3.Lerp(_origin, _target, Ease.GetEase(t));
                pos.y = _transform.position.y;
                _transform.position = pos;
                if (_time >= _duration)
                {
                    IsDone = true;
                }
            }
        }
    }

    public class Delay : SNM.Activity
    {
        private float duration;
        private float time = 0;

        public Delay(float duration)
        {
            this.duration = duration;
        }

        public override void Update(float deltaTime)
        {
            time += deltaTime;
            if (time >= duration)
            {
                IsDone = true;
            }
        }
    }
}