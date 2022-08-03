using System;
using Common;
using CommonActivities;
using SNM;
using SNM.Easings;
using UnityEngine;

namespace InGame
{
    public class PieceScheduler
    {
        private readonly Piece _piece;

        public PieceScheduler(Piece piece)
        {
            _piece = piece;
        }

        public void CreateNewJump(Vector3 pos, int flag, Action<Piece, int> callback)
        {
            // var parallelAnimation = new ParallelActivity();
            // parallelAnimation.Add(new PieceActivityQueue.Jump(_piece.transform,
            //     new PieceActivityQueue.Jump.InputData
            //     {
            //         flag = flag,
            //         callback = callback,
            //         duration = duration
            //     },
            //     BezierEasing.CreateBezierEasing(0.35f, 0.75f)));
            var jumpForward = new JumpForward(_piece.transform, pos, _piece.Config.flockingConfigData.maxSpeed,
                new LinearEasing(), 1f, BezierEasing.CreateBezierEasing(0.35f, 0.75f));
            // parallelAnimation.Add();

            // var sA = new ActivityQueue();
            // sA.Add(new Delay(0.1f));
            // sA.Add(jumpForward);

            // var parallelAnimation2 = new ParallelActivity();
            // parallelAnimation2.Add(new PieceActor.BounceAnim(_piece.FootTransform, 0.15f));
            // parallelAnimation2.Add(sA);

            _piece.PieceActivityQueue.Add(new Delay(0.1f));
            _piece.PieceActivityQueue.Add(jumpForward);

            jumpForward.Done += () => callback?.Invoke(_piece, flag);
        }

        public void CreateNewLandAnim()
        {
            // _piece.PieceActor.Add(new PieceActor.BounceAnim(_piece.FootTransform, 0.15f));
            _piece.PieceActivityQueue.Add(new PieceActivityQueue.TurnAway(_piece.transform));
        }
    }
}