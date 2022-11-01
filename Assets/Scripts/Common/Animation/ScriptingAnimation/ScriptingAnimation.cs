using Common.DecisionMaking;
using UnityEngine;

namespace Common.Animation.ScriptingAnimation
{
    public abstract class ScriptingAnimation : MonoActivity
    {
        [field: SerializeField] protected Transform Target { get; private set; }
        [SerializeField, Min(0f)] private float duration;
        [SerializeField] private AnimationCurve curve;

        private readonly ActivityQueue _activityQueue = new();
        private IAnimationMover _mover;

        [ContextMenu("Play")]
        public virtual void Play()
        {
            _activityQueue.End();
            var activity = new ActivityTimer(duration, OnTick, true);
            _mover = GetMover(Target);
            _activityQueue.Add(activity);
            _activityQueue.Begin();
        }

        protected abstract IAnimationMover GetMover(Transform target);

        private void OnTick(float p)
        {
            var e = Mathf.Max(curve.Evaluate(p), 0f);
            _mover?.Move(e);
        }

        private void Update()
        {
            _activityQueue.Update(Time.deltaTime);
        }

        public override Activity CreateActivity()
        {
            _mover = GetMover(Target);
            return new ActivityTimer(duration, OnTick, true);
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