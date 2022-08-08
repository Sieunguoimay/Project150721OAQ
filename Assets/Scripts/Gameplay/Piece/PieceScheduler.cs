using System;
using Common;
using CommonActivities;
using Gameplay;
using SNM;
using SNM.Easings;
using UnityEngine;

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

        public static void MovePieceToTheBoardOnGameStart(Piece p, Vector3 initialPos, Tile t, WaitForEnd triggerActivity,
            float delay)
        {
            p.transform.position = initialPos;
            var position = t.GetPositionInFilledCircle(Mathf.Max(0, t.Pieces.Count - 1));
            p.PieceActivityQueue.Add(triggerActivity);
            p.PieceActivityQueue.Add(new Delay(delay));
            p.PieceActivityQueue.Add(new Flocking(p.Config.flockingConfigData,
                new Flocking.InputData {target = position, transform = p.transform}, null));
            p.PieceActivityQueue.Begin();
        }
    }
}