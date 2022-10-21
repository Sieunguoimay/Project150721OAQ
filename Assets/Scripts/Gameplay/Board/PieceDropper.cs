using System;
using System.Collections.Generic;
using Common;
using Gameplay.Piece;

namespace Gameplay.Board
{
    public class PieceDropper
    {
        private readonly List<Piece.Piece> _pieces = new();

        private readonly BoardTraveller _boardTraveller = new();

        private bool _forward;
        private readonly Gameplay.Board.Board _board;

        public PieceDropper(Gameplay.Board.Board board)
        {
            _board = board;
        }

        public void ClearHoldingPieces()
        {
            _pieces.Clear();
        }

        public void Take(List<Piece.Piece> pieces, int num)
        {
            ForwardLastItems(pieces, _pieces, num);
        }

        public void SetMoveStartPoint(int index, bool forward)
        {
            _boardTraveller.Start(index, _pieces.Count, _board.Tiles.Length);
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
            var delay = 0f;
            var n = _pieces.Count;

            for (var i = 0; i < n; i++)
            {
                _boardTraveller.Next(_forward);
                var currentTile = _board.Tiles[_boardTraveller.CurrentIndex];

                for (var j = 0; j < n - i; j++)
                {
                    if (_pieces[i + j] is not Citizen p) continue;

                    var skipSlot = currentTile is MandarinTile;
                    var citizenPos =
                        currentTile.GetPositionInFilledCircle(currentTile.Pieces.Count + j + (skipSlot ? 5 : 0));

                    if (i == 0)
                    {
                        p.PieceActivityQueue.Add(new ActivityRotateToTarget(p.transform, citizenPos, delay));

                        delay += 0.2f;
                    }

                    p.PieceActivityQueue.Add(PieceScheduler.CreateJumpTimelineActivity(p, citizenPos));

                    if (i == n - 1)
                    {
                        p.PieceActivityQueue.Add(new ActivityLastDrop(done, currentTile));
                    }
                }

                currentTile.Pieces.Add(_pieces[i]);
            }

            foreach (var p in _pieces)
            {
                p.PieceActivityQueue.Add(new TurnAway(p.transform));
                p.PieceActivityQueue.Add(PieceScheduler.CreateAnimActivity(p, LegHashes.sit_down));
                p.PieceActivityQueue.Begin();
            }

            _pieces.Clear();
        }

        public void DropNonStop(Action<Tile> onDone)
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

                foreach (var p in _pieces)
                {
                    p.PieceActivityQueue.Add(PieceScheduler.CreateAnimActivity(p, LegHashes.stand_up));
                }

                DropOnce(t => ContinueDropping(done, t));
            }
            else
            {
                done?.Invoke(tile);
            }
        }
    }

    public class ActivityLastDrop : Activity
    {
        private readonly Action<Tile> _callback;
        private readonly Tile _tile;

        public ActivityLastDrop(Action<Tile> callback, Tile tile)
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