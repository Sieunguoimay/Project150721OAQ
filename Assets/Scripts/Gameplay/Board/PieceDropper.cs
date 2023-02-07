using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Piece;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IPieceDropper
    {
        void Take(IPieceContainer container, int amount);
        void SetMoveStartPoint(Board board, ITile tile, bool forward);
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
        private Action<ITile> _done;

        public void Take(IPieceContainer container, int num)
        {
            var available = container.HeldPieces.Count;
            if (num == available)
            {
                foreach (var p in container.HeldPieces)
                {
                    if (p is ICitizen c) _citizens.Add(c);
                }

                container.Clear();
            }
            else
            {
                for (var i = 0; i < Mathf.Min(num, available); i++)
                {
                    if (container.HeldPieces[^1] is not ICitizen c) continue;
                    _citizens.Add(c);
                    container.RemovePiece(c);
                }
            }
        }

        public void SetMoveStartPoint(Board board, ITile tile, bool forward)
        {
            _board = board;
            _boardTraveller.Init(Array.IndexOf(_board.Tiles, tile), _board.Tiles.Length, forward);
            _forward = forward;
        }

        public void DropOnce(Action<ITile> done)
        {
            for (var i = 0; i < _citizens.Count; i++)
            {
                IEnumerable<Vector3> GetVisitedPoints(int citizenIndex)
                {
                    for (var j = 0; j < citizenIndex + 1; j++)
                    {
                        var visitedTileIndex = _boardTraveller.GetIndexAtStep(j + 1);
                        var visitedTile = _board.Tiles[visitedTileIndex];
                        yield return visitedTile.GetGridPosition(visitedTile.HeldPieces.Count + citizenIndex - j);
                    }
                }

                var citizen = _citizens[i];
                
                citizen.CitizenMove.JumpingMove(GetVisitedPoints(i), i * 0.15f);
                citizen.CitizenMove.ReachedTargetEvent -= OnCitizenReachedTheTargetTile;
                citizen.CitizenMove.ReachedTargetEvent += OnCitizenReachedTheTargetTile;
                
                var tile = _board.Tiles[_boardTraveller.GetIndexAtStep(i + 1)];

                citizen.TargetTile.SetValue(tile);
            }

            _done = done;
        }

        private void OnCitizenReachedTheTargetTile(ICitizen citizen)
        {
            citizen.CitizenMove.ReachedTargetEvent -= OnCitizenReachedTheTargetTile;
            citizen.TargetTile.Value.AddPiece(citizen);
            _citizens.Remove(citizen);
            
            if (_citizens.Count == 0)
            {
                _done?.Invoke(citizen.TargetTile.Value);
            }
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
            var shouldContinue = successTile.HeldPieces.Count > 0 && successTile is ICitizenTile;

            if (shouldContinue)
            {
                foreach (var p in successTile.HeldPieces)
                {
                    if (p is ICitizen ci)
                    {
                        ci.ActivityQueue.Add(new ActivityAnimation(ci.Animator, LegHashes.stand_up));
                    }
                }

                Take(successTile, successTile.HeldPieces.Count);
                SetMoveStartPoint(_board, successTile, _forward);
                DropOnce(t => ContinueDropping(done, t));
            }
            else
            {
                done?.Invoke(tile);
            }
        }
    }
}