using System;
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

        public void CreateNewJump(Vector3 pos, int flag, Action<PieceActor, int> callback)
        {
            var parallelAnimation = new ParallelActivity();
            parallelAnimation.Add(new PieceActor.Jump(_piece.transform,
                new PieceActor.Jump.InputData
                {
                    flag = flag,
                    callback = callback,
                    duration = 0.4f
                },
                BezierEasing.Blueprint1));
            parallelAnimation.Add(new CommonActivities.StraightMove(_piece.transform, pos, 0.4f));

            var sA = new SequentialActivity();
            sA.Add(new CommonActivities.Delay(0.1f));
            sA.Add(parallelAnimation);

            var parallelAnimation2 = new ParallelActivity();
            // parallelAnimation2.Add(new PieceActor.BounceAnim(_piece.FootTransform, 0.15f));
            parallelAnimation2.Add(sA);

            _piece.PieceActor.Add(parallelAnimation2);
        }

        public void CreateNewLandAnim()
        {
            // _piece.PieceActor.Add(new PieceActor.BounceAnim(_piece.FootTransform, 0.15f));
            _piece.PieceActor.Add(new PieceActor.TurnAway(_piece.transform));
        }
    }
}