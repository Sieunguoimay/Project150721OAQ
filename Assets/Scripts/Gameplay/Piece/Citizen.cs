using System;
using System.Collections.Generic;
using Common.Activity;
using Common.Animation;
using Framework.Entities.Variable;
using Gameplay.Board;
using Gameplay.Piece.Activities;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.Piece
{
    public interface ICitizen : IPiece
    {
        CitizenMove CitizenMove { get; }
        Animator Animator { get; }
        IActivityQueue ActivityQueue { get; }
        IVariable<ITile> TargetTile { get; } 
    }

    [SelectionBase]
    public class Citizen : Piece, ICitizen
    {
        [SerializeField] private AnimatorListener animatorListener;
        [SerializeField] private PlayableDirector jumpTimeline;

        [SerializeField] private ActivityFlocking.ConfigData flockingConfigData = new()
        {
            maxSpeed = 3f,
            maxAcceleration = 10f,
            arriveDistance = 1f,
            spacing = 0.3f
        };

        private Transform _cameraTransform;
        private Animator _animator;
        public Animator Animator => _animator ??= animatorListener.GetComponent<Animator>();
        public PlayableDirector JumpTimeline => jumpTimeline;

        private Activity[] _standUpActivities;
        public CitizenMove CitizenMove { get; private set; }

        private readonly ActivityQueue _activityQueue = new();
        public IActivityQueue ActivityQueue => _activityQueue;
        public ActivityFlocking.ConfigData FlockingConfigData => flockingConfigData;

        public IVariable<ITile> TargetTile { get; } = new Variable<ITile>();
        
        private void Awake()
        {
            _standUpActivities = new Activity[]
            {
                new ActivityAnimation(Animator, LegHashes.stand_up),
                new ActivityCallback(() => Animator.Play(LegHashes.idle))
            };
            CitizenMove = new CitizenMove(this);
        }

        private void Update()
        {
            _activityQueue.Update(Time.deltaTime);
        }

        public void PlayAnimStandUp()
        {
            foreach (var a in _standUpActivities)
            {
                ActivityQueue.Add(a);
            }

            ActivityQueue.Begin();
        }

        public void PlayAnimSitDown()
        {
            Animator.Play(LegHashes.sit_down);
        }
        
    }

    public class CitizenMove
    {
        private readonly Citizen _citizen;

        public CitizenMove(Citizen citizen)
        {
            _citizen = citizen;
        }

        public event Action<Citizen> ReachedTargetEvent;
        public event Action<Citizen> MoveDoneEvent;

        public void JumpingMove(IEnumerable<Vector3> targetSequence, float delay = 0f)
        {
            _citizen.ActivityQueue.Add(delay > 0 ? new ActivityDelay(delay) : null);

            var firstTarget = true;
            foreach (var target in targetSequence)
            {
                if (firstTarget)
                {
                    _citizen.ActivityQueue.Add(new ActivityRotateToTarget(_citizen.transform, target, .2f));
                    firstTarget = false;
                }

                _citizen.ActivityQueue.Add(new ActivityJumpTimeline(_citizen, target));
            }

            _citizen.ActivityQueue.Add(new ActivityCallback(() => ReachedTargetEvent?.Invoke(_citizen)));
            _citizen.ActivityQueue.Add(new ActivityAnimation(_citizen.Animator, LegHashes.land));
            _citizen.ActivityQueue.Add(new ActivityTurnAway(_citizen.transform));
            _citizen.ActivityQueue.Add(new ActivityAnimation(_citizen.Animator, LegHashes.sit_down));
            _citizen.ActivityQueue.Add(new ActivityCallback(() => MoveDoneEvent?.Invoke(_citizen)));
            _citizen.ActivityQueue.Begin();
        }

        public void StraightMove(Vector3 target, float delay)
        {
            _citizen.ActivityQueue.Add(delay > 0f ? new ActivityDelay(delay) : null);
            _citizen.ActivityQueue.Add(new ActivityAnimation(_citizen.Animator, LegHashes.stand_up));
            _citizen.ActivityQueue.Add(new ActivityFlocking(_citizen.FlockingConfigData, target, _citizen.transform,null));
            _citizen.ActivityQueue.Add(new ActivityAnimation(_citizen.Animator, LegHashes.sit_down));
            _citizen.ActivityQueue.Add(new ActivityCallback(() => ReachedTargetEvent?.Invoke(_citizen)));
            _citizen.ActivityQueue.Begin();
        }
    }
}