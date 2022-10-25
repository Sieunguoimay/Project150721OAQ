using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Gameplay.Piece;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Board
{
    public class PieceDropper : IPieceContainer
    {
        private readonly BoardTraveller _boardTraveller = new();
        private Board _board;

        private bool _forward;

        public List<Piece.Piece> Pieces { get; } = new();

        public void SetBoard(Board board)
        {
            _board = board;
        }

        public void ClearHoldingPieces()
        {
            Pieces.Clear();
        }

        public void Take(List<Piece.Piece> pieces, int num)
        {
            ForwardLastItems(pieces, Pieces, num);
        }

        public void SetMoveStartPoint(int index, bool forward)
        {
            _boardTraveller.Start(index, Pieces.Count, _board.Tiles.Length);
            _forward = forward;
        }

        public static void ForwardLastItems<T>(List<T> source, List<T> target, int num)
        {
            if (num == source.Count)
            {
                target.AddRange(source);
                source.Clear();
                return;
            }

            target.AddRange(source.GetRange(source.Count - num, num));
            source.RemoveRange(source.Count - num, num);
        }

        public void DropOnce(Action<Tile> done)
        {
            var n = Pieces.Count;

            for (var i = 0; i < n; i++)
            {
                _boardTraveller.Next(_forward);
                var currentTile = _board.Tiles[_boardTraveller.CurrentIndex];

                for (var j = 0; j < n - i; j++)
                {
                    if (Pieces[i + j] is not Citizen p) continue;

                    var skipSlot = currentTile is MandarinTile mt && mt.Pieces.Any(pi => pi is Mandarin);
                    var index = currentTile.Pieces.Count + j + (skipSlot ? 9 : 0);
                    var citizenPos = currentTile.GetPositionInFilledCircle(index);

                    p.ActivityQueue.Add(i == 0 && j > 0 ? new ActivityDelay(j * 0.1f) : null);
                    p.ActivityQueue.Add(i == 0 ? new ActivityRotateToTarget(p.transform, citizenPos, 0.2f) : null);
                    p.ActivityQueue.Add(new ActivityJumpTimeline(p, citizenPos));
                    p.ActivityQueue.Add(i == n - 1 ? new ActivityNotifyOnLastDrop(done, currentTile) : null);
                }

                currentTile.Pieces.Add(Pieces[i]);
            }

            foreach (var p in Pieces)
            {
                p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.land));
                p.ActivityQueue.Add(new ActivityTurnAway(p.transform));
                p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.sit_down));
                p.ActivityQueue.Begin();
            }

            Pieces.Clear();
        }

        public void DropTillDawn(Action<Tile> onDone)
        {
            DropOnce(t => ContinueDropping(onDone, t));
        }

        private void ContinueDropping(Action<Tile> done, Tile tile)
        {
            var successTile = _board.GetSuccessTile(tile, _forward);

            _boardTraveller.Reset();

            if (successTile.Pieces.Count > 0 && successTile is not MandarinTile)
            {
                Take(successTile.Pieces, successTile.Pieces.Count);
                SetMoveStartPoint(Array.IndexOf(_board.Tiles, successTile), _forward);

                foreach (var p in Pieces)
                {
                    p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.stand_up));
                }

                DropOnce(t => ContinueDropping(done, t));
            }
            else
            {
                done?.Invoke(tile);
            }
        }

        private class ActivityNotifyOnLastDrop : Activity
        {
            private readonly Action<Tile> _callback;
            private readonly Tile _tile;

            public ActivityNotifyOnLastDrop(Action<Tile> callback, Tile tile)
            {
                _tile = tile;
                _callback = callback;
            }

            public override void Begin()
            {
                base.Begin();
                _callback?.Invoke(_tile);
                NotifyDone();
            }
        }
    }
}