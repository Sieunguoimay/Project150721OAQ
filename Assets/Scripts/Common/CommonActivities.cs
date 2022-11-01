using System;
using UnityEngine;

namespace Common
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
                MarkAsDone();
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
                MarkAsDone();
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
                MarkAsDone();
            }
        }
    }
}