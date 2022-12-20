﻿using System.Linq;
using Common.Activity;
using Common.DecisionMaking;
using UnityEngine;

namespace Common.Animation.ScriptingAnimation
{
    public abstract class ScriptingAnimation : MonoActivity
    {
        [field: SerializeField] protected Transform Target { get; private set; }
        [SerializeField, Min(0f)] private float duration;
        [field: SerializeField] protected AnimationCurve Curve { get; private set; }

        public float Duration => duration;

        [field: System.NonSerialized] public ActivityQueue ActivityQueue { get; } = new();

        [ContextMenu("Play")]
        public virtual void Play()
        {
            if (!ActivityQueue.Inactive)
            {
                ActivityQueue.End();
            }

            ActivityQueue.Add(new ActivityTimer(duration, Tick, true));
            ActivityQueue.Begin();
        }

        public void Tick(float p)
        {
            OnTick(Curve.Evaluate(p));
        }

        protected abstract void OnTick(float p);

        private void Update()
        {
            ActivityQueue.Update(Time.deltaTime);
        }

        public override Activity.Activity CreateActivity()
        {
            return new ActivityTimer(duration, Tick, true);
        }
    }

    public abstract class MovingAnimation : ScriptingAnimation
    {
        protected abstract IAnimationMover GetMover(Transform target);
        private IAnimationMover _mover;

        public override void Play()
        {
            base.Play();
            _mover = GetMover(Target);
        }

        protected override void OnTick(float p)
        {
            _mover?.Move(Mathf.Max(p, 0f));
        }

        public override Activity.Activity CreateActivity()
        {
            _mover = GetMover(Target);

            return base.CreateActivity();
        }
    }

    public interface IAnimationMover
    {
        void Move(float progress);
    }

    public abstract class BaseAnimationMover : IAnimationMover
    {
        protected readonly Transform Target;

        protected BaseAnimationMover(Transform target)
        {
            Target = target;
        }

        public abstract void Move(float progress);
    }
}