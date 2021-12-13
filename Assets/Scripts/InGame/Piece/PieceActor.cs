using System;
using DG.Tweening;
using SNM;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PieceActor : SNM.Actor
{
    public event Action<bool> OnJump = delegate(bool last) { };
    public bool IsJumping => CurrentActivity != null;

    [Serializable]
    public struct ConfigData
    {
        public float angularSpeed;
    }

    public ConfigData Config { get; } = new ConfigData() {angularSpeed = 270f};

    protected override void OnNewActivity(Activity activity)
    {
        if (activity is Jump ja)
        {
            OnJump?.Invoke(ja.inputData.flag > 0);
        }
    }

    public class Jump : Activity
    {
        public InputData inputData;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
        public Vector3 initialAcceleration;
        public float time;
        public float duration;
        private Transform transform;

        public Jump(Transform transform, InputData inputData, IEasing easing = null)
        {
            this.transform = transform;
            this.inputData = inputData;
            if (easing != null)
            {
                SetEase(easing);
            }
        }

        public override void Begin()
        {
            float h = inputData.height;
            float t = inputData.duration;
            var pos = transform.position;
            float a = (-8f * h) / (t * t);

            time = 0;
            IsDone = false;
            duration = t;
            initialPosition = pos;
            initialAcceleration = Vector3.up * a;
            initialVelocity = Vector3.up * (-a * 0.5f * duration);
        }

        public override void Update(float deltaTime)
        {
            if (!IsDone)
            {
                time += deltaTime;

                float t = Mathf.Min(time / duration, 1f);

                // var xz = SNM.Math.MotionEquation(
                //     initialPosition, initialVelocity,
                //     initialAcceleration, time);

                var xz = transform.position;
                var y = SNM.Math.MotionEquation(
                    initialPosition, initialVelocity,
                    initialAcceleration, Ease.GetEase(t) * duration);
                transform.position = new Vector3(xz.x, y.y, xz.z);

                if (time >= duration)
                {
                    IsDone = true;
                }
            }
        }

        public override void End()
        {
            inputData.callback?.Invoke(null, inputData.flag);
        }

        public class InputData
        {
            // public Vector3 target;
            public float height = 1f;
            public float duration = 0.4f;
            public int flag;
            public Action<PieceActor, int> callback;
        }
    }
    public class TurnAway : Activity
    {
        private Transform transform;

        public TurnAway(Transform transform)
        {
            this.transform = transform;
        }

        public override void Begin()
        {
            base.Begin();

            var lr = transform.localEulerAngles;
            lr.y += UnityEngine.Random.Range(-60f, 60f);
            transform.DOLocalRotate(lr, 1f).SetId(this)
                .OnComplete(() => { IsDone = true; });
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
            if (!IsDone)
            {
                _time += deltaTime;
                float t = Mathf.Min(_time / _duration, 1f);

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
                    IsDone = true;
                }
            }
        }
    }
}