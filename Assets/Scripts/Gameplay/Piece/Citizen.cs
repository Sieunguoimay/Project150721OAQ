using System;
using System.Linq;
using Common;
using Common.Animation;
using DG.Tweening;
using Gameplay.Piece.Activities;
using Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.Piece
{
    [SelectionBase]
    public class Citizen : Piece
    {
        [SerializeField] private AnimatorListener animatorListener;
        [SerializeField] private PlayableDirector jumpTimeline;

        private Transform _cameraTransform;
        private Animator _animator;
        public override Animator Animator => _animator ??= animatorListener.GetComponent<Animator>();
        public override PlayableDirector JumpTimeline => jumpTimeline;

        private Activity[] _standUpActivities;

        private void Awake()
        {
            _standUpActivities = new Activity[]
            {
                new ActivityAnimation(Animator, LegHashes.stand_up),
                new ActivityCallback(() => Animator.Play(LegHashes.idle))
            };
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
}