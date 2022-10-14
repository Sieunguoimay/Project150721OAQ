using System;
using System.Linq;
using Common.Animation;
using DG.Tweening;
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

        public void PlayAnimStandUp()
        {
            PieceScheduler.CreateAnimActivity(this, LegHashes.stand_up, () => { Animator.Play(LegHashes.idle); });
            PieceActivityQueue.Begin();
        }

        public void PlayAnimSitDown()
        {
            Animator.Play(LegHashes.sit_down);
        }
    }
}