using System;
using Common;
using CommonActivities;
using Gameplay;
using SNM;
using SNM.Easings;
using UnityEngine;
using Math = System.Math;

namespace Gameplay
{
    public class PieceScheduler
    {
        public static void MovePiecesOutOfTheBoard(Piece[] pieces, Vector3[] positions, Vector3 centerPoint)
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
                var flocking = new Flocking(pieces[i].Config.flockingConfigData,
                    new Flocking.InputData {target = positions[i], transform = pieces[i].transform}, null);

                pieces[i].PieceActivityQueue.Add(new Delay(delay += 0.2f));
                pieces[i].PieceActivityQueue.Add(flocking);
                pieces[i].PieceActivityQueue.Begin();
            }
        }

        public static void MovePieceToTheBoardOnGameStart(Piece p, Vector3 initialPos, Tile t, Activity triggerActivity,
            float delay)
        {
            p.transform.position = initialPos;
            var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.Pieces.Count - 1));
            p.PieceActivityQueue.Add(triggerActivity);
            CreateAAnimActivity(p, LegHashes.sit_down, null);
            p.PieceActivityQueue.Add(new Delay(delay));
            p.PieceActivityQueue.Add(new Flocking(p.Config.flockingConfigData,
                new Flocking.InputData {target = position, transform = p.transform}, null));
            p.PieceActivityQueue.Begin();
        }

        public static void CreateAAnimActivity(Piece p, int animHash, Action onDone)
            => CreateAAnimActivity(p, () => animHash, onDone);

        public static void CreateAAnimActivity(Piece p, Func<int> animHash, Action onDone)
        {
            var anim = -1;
            Activity animActivity = new Lambda(() =>
            {
                anim = animHash.Invoke();
                p.Animator?.Play(anim, -1, 0f);
            }, () =>
            {
                if (p.Animator == null) return true;

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