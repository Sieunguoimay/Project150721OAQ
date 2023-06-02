using System;
using Common.Activity;
using Common.Animation;
using Gameplay.Visual.Piece.Activities;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.Visual.Piece
{
    public class CitizenAnimator : MonoBehaviour
    {
        [SerializeField] private AnimatorStateEventListener animatorListener;
        [SerializeField] private PlayableDirector jumpTimeline;

        [SerializeField] private ActivityFlocking.ConfigData flockingConfigData = new()
        {
            maxSpeed = 3f,
            maxAcceleration = 10f,
            arriveDistance = 1f,
            spacing = 0.3f
        };

        private Animator _animator;
        private Animator Animator => _animator ??= animatorListener.GetComponent<Animator>();
        public PlayableDirector JumpTimeline => jumpTimeline;

        private readonly ActivityQueue _activityQueue = new();
        private IActivityQueue ActivityQueue => _activityQueue;
        private ActivityFlocking.ConfigData FlockingConfigData => flockingConfigData;

        private void Update()
        {
            _activityQueue.Update(Time.deltaTime);
        }

        public void PlayAnimStandUp(Action completeHandler = null)
        {
            ActivityQueue.Add(new ActivityAnimation(Animator, LegHashes.stand_up));
            ActivityQueue.Add(new ActivityCallback(() =>
            {
                Animator.Play(LegHashes.idle);
                completeHandler?.Invoke();
            }));
            ActivityQueue.Begin();
        }

        public void StandUpAndRotateTo(Vector3 target, Action completeHandler)
        {
            ActivityQueue.Add(new ActivityAnimation(Animator, LegHashes.stand_up));
            ActivityQueue.Add(new ActivityCallback(() => { Animator.Play(LegHashes.idle); }));
            ActivityQueue.Add(new ActivityRotateToTarget(transform, target, .2f));
            ActivityQueue.Add(new ActivityCallback(() => { completeHandler?.Invoke(); }));
            ActivityQueue.Begin();
        }

        public void PlayAnimSitDown()
        {
            Animator.Play(LegHashes.sit_down);
        }

        public void JumpTo(Vector3 target, Action completeHandler)
        {
            ActivityQueue.Add(new ActivityJumpTimeline(this, () => target));
            ActivityQueue.Add(new ActivityCallback(() => { completeHandler?.Invoke(); }));
            ActivityQueue.Begin();
        }

        public void RotateToward(Vector3 target, Action completeHandler)
        {
            ActivityQueue.Add(new ActivityRotateToTarget(transform, target, .2f));
            ActivityQueue.Add(new ActivityCallback(() => { completeHandler?.Invoke(); }));
            ActivityQueue.Begin();
        }

        public void StraightMove(Vector3 target, Action reachTargetCallback, float delay)
        {
            var activityQueue = ActivityQueue;
            activityQueue.Add(delay > 0f ? new ActivityDelay(delay) : null);
            activityQueue.Add(new ActivityAnimation(Animator, LegHashes.stand_up));
            activityQueue.Add(new ActivityFlocking(FlockingConfigData, target, transform, null));
            activityQueue.Add(new ActivityAnimation(Animator, LegHashes.sit_down));
            activityQueue.Add(new ActivityCallback(() => reachTargetCallback?.Invoke()));
            activityQueue.Begin();
        }
    }
}