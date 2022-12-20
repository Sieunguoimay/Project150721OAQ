using System;
using UnityEngine;

namespace Common.Activity
{
    public class ActivityDelay : Activity
    {
        protected float Time { get; set; } = 0;
        protected float Duration { get; }

        public ActivityDelay(float duration)
        {
            Duration = duration;
        }

        public override void Begin()
        {
            base.Begin();
            Time = 0f;
        }

        public override void Update(float deltaTime)
        {
            Time += deltaTime;
            if (Time >= Duration)
            {
                End();
            }
        }
    }

    public class ActivityTimer : ActivityDelay
    {
        private readonly Action<float> _onTick;
        private readonly bool _progress;

        public ActivityTimer(float duration, Action<float> onTick, bool progress = false) : base(duration)
        {
            _onTick = onTick;
            _progress = progress;
        }

        public override void Begin()
        {
            base.Begin();
            _onTick?.Invoke(0f);
        }

        public override void Update(float deltaTime)
        {
            Time += deltaTime;
            if (Time >= Duration)
            {
                _onTick?.Invoke(_progress ? 1f : Duration);
                End();
            }
            else
            {
                _onTick?.Invoke(_progress ? Mathf.Min(1f, Time / Duration) : Mathf.Min(Time, Duration));
            }
        }
    }

    public class Lambda : Activity
    {
        private readonly Action _onBegin;
        private readonly Func<bool> _onUpdate;

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
                End();
            }
        }
    }

    public class FloatRunnerActivity : Activity
    {
        private readonly float _beginValue;
        private readonly float _endValue;
        private readonly float _speed;
        private readonly LoopType _loopType;
        private readonly Action<float> _onValueProgress;

        private float _value;
        private float _sign;

        public FloatRunnerActivity(float beginValue, float endValue, float speed, LoopType loopType, Action<float> onValueProgress)
        {
            _beginValue = beginValue;
            _endValue = endValue;
            _speed = speed;
            _loopType = loopType;
            _onValueProgress = onValueProgress;

            _sign = Mathf.Sign(_endValue - _beginValue);
        }

        public override void Begin()
        {
            base.Begin();
            _onValueProgress?.Invoke(_value = _beginValue);
        }

        public override void Update(float deltaTime)
        {
            _value += deltaTime * _speed * _sign;
            switch (_loopType)
            {
                case LoopType.None:
                    if (_sign > 0 && _value >= _endValue)
                    {
                        _onValueProgress?.Invoke(_value);
                        End();
                        return;
                    }

                    if (_sign < 0 && _value <= _endValue)
                    {
                        _onValueProgress?.Invoke(_value);
                        End();
                        return;
                    }

                    break;
                case LoopType.PingPong:
                {
                    if (_value >= _endValue)
                    {
                        _sign *= -1;
                        _value = _endValue;
                    }
                    else if (_value <= _beginValue)
                    {
                        _sign *= -1;
                        _value = _beginValue;
                    }

                    break;
                }    
                case LoopType.Restart:
                {
                    if (_value >= _endValue) _value = _beginValue;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _onValueProgress?.Invoke(_value);
        }

        [Serializable]
        public enum LoopType
        {
            None,
            Restart,
            PingPong
        }
    }
}