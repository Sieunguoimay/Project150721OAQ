using System;
using Common;
using CommonActivities;
using Gameplay.Board;
using UnityEngine;

namespace Gameplay.Piece
{
    public static class PieceScheduler
    {
        public static void MovePiecesOutOfTheBoard(Gameplay.Piece.Piece[] pieces, Vector3[] positions, Vector3 centerPoint)
        {
            Array.Sort(pieces, (a, b) =>
            {
                var da = Vector3.SqrMagnitude(centerPoint - a.transform.position);
                var db = Vector3.SqrMagnitude(centerPoint - b.transform.position);
                return da < db ? -1 : 1;
            });
            var delay = 0f;
            for (var i = 0; i < pieces.Length; i++)
            {
                var flocking = new Flocking(pieces[i].Config.flockingConfigData, positions[i], pieces[i].transform, null);

                pieces[i].PieceActivityQueue.Add(new Delay(delay += 0.2f));
                CreateAnimActivity(pieces[i], LegHashes.stand_up, null);
                pieces[i].PieceActivityQueue.Add(flocking);
                CreateAnimActivity(pieces[i], LegHashes.sit_down, null);
                pieces[i].PieceActivityQueue.Begin();
            }
        }

        public static void MovePieceToTheBoardOnGameStart(Citizen p, Tile t, Activity triggerActivity,
            float delay)
        {
            var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.Pieces.Count - 1));
            if (triggerActivity != null)
            {
                p.PieceActivityQueue.Add(triggerActivity);
            }

            CreateAnimActivity(p, LegHashes.sit_down, null);
            p.PieceActivityQueue.Add(new Delay(delay));
            p.PieceActivityQueue.Add(new Flocking(p.Config.flockingConfigData, position, p.transform, null));
            p.PieceActivityQueue.Begin();
        }

        public static void CreateAnimActivity(Piece p, int animHash, Action onDone)
            => CreateAnimActivity(p, () => animHash, onDone);

        public static void CreateAnimActivity(Piece p, Func<int> animHash, Action onDone)
        {
            if (p.Animator == null) return;

            var anim = -1;
            Activity animActivity = new Lambda(() =>
            {
                anim = animHash.Invoke();
                p.Animator.Play(anim, -1, 0f);
            }, () =>
            {
                var info = p.Animator.GetCurrentAnimatorStateInfo(0);
                if (info.shortNameHash != anim) return false;

                if (info.loop)
                    Debug.LogError("Loop... :(");

                return info.normalizedTime >= 1f;
            });
            if (onDone != null)
            {
                animActivity.Done += onDone;
            }

            p.PieceActivityQueue.Add(animActivity);
        }
    }
}