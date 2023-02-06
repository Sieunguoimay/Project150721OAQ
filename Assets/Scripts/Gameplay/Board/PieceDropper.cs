using System;
using System.Collections.Generic;
using Gameplay.Piece;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IPieceDropper
    {
        void Take(Board board, IPieceContainer container, int amount);
        void SetMoveStartPoint(int index, bool forward);
        void DropOnce(Action<ITile> done);
        void DropTillDawn(Action<ITile> onDone);
        void Cleanup();
    }

    public class PieceDropper : IPieceDropper
    {
        private readonly BoardTraveller _boardTraveller = new();
        private readonly List<ICitizen> _citizens = new();

        private bool _forward;
        private Board _board;

        public void Take(Board board, IPieceContainer container, int num)
        {
            _board = board;
            var available = container.HeldPieces.Count;
            if (num == available)
            {
                foreach (var p in container.HeldPieces)
                {
                    _citizens.Add(p as ICitizen);
                }

                container.Clear();
            }
            else
            {
                for (var i = 0; i < Mathf.Min(num, available); i++)
                {
                    _citizens.Add(container.HeldPieces[^1] as ICitizen);
                    container.RemoveLast();
                }
            }
        }

        public void SetMoveStartPoint(int index, bool forward)
        {
            _boardTraveller.Init(_board, index, _board.Tiles.Length, forward);
            _forward = forward;
        }

        public void DropOnce(Action<ITile> done)
        {
            var n = _citizens.Count;

            for (var i = 0; i < n; i++)
            {
                IEnumerable<Vector3> GetVisitedPoints(int citizenIndex)
                {
                    for (var j = 0; j < citizenIndex + 1; j++)
                    {
                        var visitedTileIndex = _boardTraveller.GetIndexAtStep(j + 1, _forward);
                        var visitedTile = _board.Tiles[visitedTileIndex];
                        yield return visitedTile.GetGridPosition(visitedTile.HeldPieces.Count + citizenIndex - j);
                    }
                }

                var citizen = _citizens[i];

                citizen.CitizenMove.JumpingMove(GetVisitedPoints(i), i * 0.15f);

                var stayedTileIndex = _boardTraveller.GetIndexAtStep(i + 1, _forward);
                var tile = _board.Tiles[stayedTileIndex];
                tile.AddPiece(citizen);
            }

            var lastTile = _board.Tiles[_boardTraveller.GetIndexAtStep(n, _forward)];
            var lastCitizen = _citizens[^1];
            lastCitizen.CitizenMove.ReachedTargetEvent += OnCitizenReachedTheTargetTile;

            void OnCitizenReachedTheTargetTile(ICitizen citizen)
            {
                citizen.CitizenMove.ReachedTargetEvent -= OnCitizenReachedTheTargetTile;
                done?.Invoke(lastTile);
            }

            _citizens.Clear();
        }

        public void DropTillDawn(Action<ITile> onDone)
        {
            DropOnce(t => ContinueDropping(onDone, t));
        }

        public void Cleanup()
        {
            if (_citizens.Count > 0)
            {
                _citizens.Clear();
            }
        }

        private void ContinueDropping(Action<ITile> done, ITile tile)
        {
            var successTile = Board.GetSuccessTile(_board.Tiles, tile, _forward);

            _boardTraveller.Reset();

            if (successTile.HeldPieces.Count > 0 && successTile is ICitizenTile)
            {
                Take(_board, successTile, successTile.HeldPieces.Count);
                SetMoveStartPoint(Array.IndexOf(_board.Tiles, successTile), _forward);

                foreach (var ci in _citizens)
                {
                    ci.ActivityQueue.Add(new ActivityAnimation(ci.Animator, LegHashes.stand_up));
                }

                DropOnce(t => ContinueDropping(done, t));
            }
            else
            {
                done?.Invoke(tile);
            }
        }
    }
}