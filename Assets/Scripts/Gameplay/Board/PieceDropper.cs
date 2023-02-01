using System;
using System.Collections.Generic;
using System.Linq;
using Common.Activity;
using Gameplay.Piece;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IPieceDropper
    {
        void SetBoard(Board board);
        event Action<IPieceDropper> BoardChanged;
        Board Board { get; }
        void Take(IPieceContainer container, int amount);
        void SetMoveStartPoint(int index, bool forward);
        void DropOnce(Action<ITile> done);
        void DropTillDawn(Action<ITile> onDone);
    }

    public class PieceDropper : IPieceContainer, IPieceDropper
    {
        private readonly BoardTraveller _boardTraveller = new();

        private bool _forward;

        private readonly List<Piece.Piece> _heldPieces = new();
        public IReadOnlyList<Piece.Piece> HeldPieces => _heldPieces;

        public void AddPiece(Piece.Piece piece)
        {
            _heldPieces.Add(piece);
        }

        public void RemoveLast()
        {
            if (_heldPieces.Count > 0)
            {
                _heldPieces.RemoveAt(_heldPieces.Count - 1);
            }
        }

        public void Sort(Comparison<Piece.Piece> comparison)
        {
            _heldPieces.Sort(comparison);
        }

        public void Clear()
        {
            _heldPieces.Clear();
        }

        public Board Board { get; private set; }
        public event Action<IPieceDropper> BoardChanged;

        public void SetBoard(Board board)
        {
            Board = board;
            BoardChanged?.Invoke(this);
        }

        public void Take(IPieceContainer container, int num)
        {
            var available = container.HeldPieces.Count;
            if (num == available)
            {
                _heldPieces.AddRange(container.HeldPieces);
                container.Clear();
                return;
            }

            for (var i = 0; i < Mathf.Min(num, available); i++)
            {
                container.RemoveLast();
                _heldPieces.Add(container.HeldPieces[^1]);
            }
        }

        public void SetMoveStartPoint(int index, bool forward)
        {
            _boardTraveller.Start(index, HeldPieces.Count, Board.Tiles.Length);
            _forward = forward;
        }

        public void DropOnce(Action<ITile> done)
        {
            var n = HeldPieces.Count;

            for (var i = 0; i < n; i++)
            {
                _boardTraveller.Next(_forward);
                var currentTile = Board.Tiles[_boardTraveller.CurrentIndex];

                for (var j = 0; j < n - i; j++)
                {
                    if (HeldPieces[i + j] is not Citizen p) continue;

                    var skipSlot = currentTile.TargetPieceType == PieceType.Mandarin && currentTile.HeldPieces.Any(pi => pi.PieceType == PieceType.Mandarin);
                    var index = currentTile.HeldPieces.Count + j + (skipSlot ? 9 : 0);
                    var citizenPos = currentTile.GetPositionInFilledCircle(index);

                    p.ActivityQueue.Add(i == 0 && j > 0 ? new ActivityDelay(j * 0.1f) : null);
                    p.ActivityQueue.Add(i == 0 ? new ActivityRotateToTarget(p.transform, citizenPos, 0.2f) : null);
                    p.ActivityQueue.Add(new ActivityJumpTimeline(p, citizenPos));
                    p.ActivityQueue.Add(i == n - 1 ? new ActivityNotifyOnLastDrop(done, currentTile) : null);
                }

                currentTile.AddPiece(HeldPieces[i]);
            }

            foreach (var p in HeldPieces)
            {
                p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.land));
                p.ActivityQueue.Add(new ActivityTurnAway(p.transform));
                p.ActivityQueue.Add(new ActivityAnimation(p.Animator, LegHashes.sit_down));
                p.ActivityQueue.Begin();
            }

            _heldPieces.Clear();
        }

        public void DropTillDawn(Action<ITile> onDone)
        {
            DropOnce(t => ContinueDropping(onDone, t));
        }

        private void ContinueDropping(Action<ITile> done, ITile tile)
        {
            var successTile = Board.GetSuccessTile(tile, _forward);

            _boardTraveller.Reset();

            if (successTile.HeldPieces.Count > 0 && successTile.TargetPieceType == PieceType.Citizen)
            {
                Take(successTile, successTile.HeldPieces.Count);
                SetMoveStartPoint(Array.IndexOf(Board.Tiles, successTile), _forward);

                foreach (var p in HeldPieces)
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
            private readonly Action<ITile> _callback;
            private readonly ITile _tile;

            public ActivityNotifyOnLastDrop(Action<ITile> callback, ITile tile)
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