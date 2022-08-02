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

        public void CreateNewJump(Vector3 pos, int flag, Action<PieceActivityQueue, int> callback)
        {
            var parallelAnimation = new ParallelActivity();
            parallelAnimation.Add(new PieceActivityQueue.Jump(_piece.transform,
                new PieceActivityQueue.Jump.InputData
                {
                    flag = flag,
                    callback = callback,
                    duration = 0.4f
                },
                BezierEasing.CreateBezierEasing(0.35f, 0.75f)));
            parallelAnimation.Add(new CommonActivities.StraightMove(_piece.transform, pos, 0.4f, new LinearEasing()));

            var sA = new ActivityQueue();
            sA.Add(new CommonActivities.Delay(0.1f));
            sA.Add(parallelAnimation);

            var parallelAnimation2 = new ParallelActivity();
            // parallelAnimation2.Add(new PieceActor.BounceAnim(_piece.FootTransform, 0.15f));
            parallelAnimation2.Add(sA);

            _piece.PieceActivityQueue.Add(parallelAnimation2);
        }

        public void CreateNewLandAnim()
        {
            // _piece.PieceActor.Add(new PieceActor.BounceAnim(_piece.FootTransform, 0.15f));
            _piece.PieceActivityQueue.Add(new PieceActivityQueue.TurnAway(_piece.transform));
        }
    }
}