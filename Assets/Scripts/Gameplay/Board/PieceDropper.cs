using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Activity;
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

        public List<Piece.Piece> PiecesContainer { get; } = new();

        public void SetBoard(Board board)
        {
            _board = board;
        }

        public void ClearHoldingPieces()
        {
            PiecesContainer.Clear();
        }

        public void Take(List<Piece.Piece> pieces, int num)
        {
            ForwardLastItems(pieces, PiecesContainer, num);
        }

        public void SetMoveStartPoint(int index, bool forward)
        {
            _boardTraveller.Start(index, PiecesContainer.Count, _board.Tiles.Length);
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
            var n = PiecesContainer.Count;

            for (var i = 0; i < n; i++)
            {
                _boardTraveller.Next(_forward);
                var currentTile = _board.Tiles[_boardTraveller.CurrentIndex];

                for (var j = 0; j < n - i; j++)
                {
                    if (PiecesContainer[i + j] is not Citizen p) continue;

                    var skipSlot = currentTile.TargetPieceType == Piece.Piece.PieceType.Mandarin && currentTile.PiecesContainer.Any(pi => pi.Type == Piece.Piece.PieceType.Mandarin);
                    var index = currentTile.PiecesContainer.Count + j + (skipSlot ? 9 : 0);
                    var citizenPos = currentTile.GetPositionInFilledCircle(index);

                    p.ActivityQueue.Add(i == 0 && j > 0 ? new ActivityDelay(j * 0.1f) : null);
                    p.ActivityQueue.Add(i == 0 ? new ActivityRotateToTarget(p.transform, citizenPos, 0.2f) : null);
                    p.ActivityQueue.Add(new ActivityJumpTimeline(p, citizenPos));
                    p.ActivityQueue.Add(i == n - 1 ? new ActivityNotifyOnLastDrop(done, currentTile) : null);
                }

                currentTile.PiecesContainer.Add(PiecesContainer[i]);
            }

            foreach (var p in PiecesContainer)
            {
                p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.land));
                p.ActivityQueue.Add(new ActivityTurnAway(p.transform));
                p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.sit_down));
                p.ActivityQueue.Begin();
            }

            PiecesContainer.Clear();
        }

        public void DropTillDawn(Action<Tile> onDone)
        {
            DropOnce(t => ContinueDropping(onDone, t));
        }

        private void ContinueDropping(Action<Tile> done, Tile tile)
        {
            var successTile = _board.GetSuccessTile(tile, _forward);

            _boardTraveller.Reset();

            if (successTile.PiecesContainer.Count > 0 && successTile.TargetPieceType == Piece.Piece.PieceType.Citizen)
            {
                Take(successTile.PiecesContainer, successTile.PiecesContainer.Count);
                SetMoveStartPoint(Array.IndexOf(_board.Tiles, successTile), _forward);

                foreach (var p in PiecesContainer)
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
                End();
            }
        }
    }
}