using System;
using System.Collections.Generic;
using Gameplay.Piece;
using Gameplay.Piece.Activities;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IPieceDropper
    {
        void Take(IPieceContainer container, int amount);
        void TakeAll(IPieceContainer container);
        void SetMoveStartPoint(IReadOnlyList<TileAdapter> tiles, int index, bool forward);
        void DropOnce(Action<ITile> done);
        void DropTillDawn(Action<IPieceDropper,ITile> onDone);
        void Cleanup();
    }

    public class TileAdapter
    {
        public TileAdapter(ITile tile)
        {
            Tile = tile;
        }

        public ITile Tile { get; }
    }
    public class PieceDropper : IPieceDropper
    {
        private readonly BoardTraveller _boardTraveller = new();
        private readonly List<ICitizen> _citizens = new();

        private bool _forward;
        private IReadOnlyList<TileAdapter> _tileSpace;
        private Action<ITile> _done;

        public void Take(IPieceContainer container, int num)
        {
            var available = container.HeldPieces.Count;
            if (num == available)
            {
                TakeAll(container);
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

        public void TakeAll(IPieceContainer container)
        {
            foreach (var p in container.HeldPieces)
            {
                if (p is ICitizen c) _citizens.Add(c);
            }

            container.Clear();
        }

        public void SetMoveStartPoint(IReadOnlyList<TileAdapter> tiles, int index, bool forward)
        {
            _tileSpace = tiles;
            _boardTraveller.Init(index, _tileSpace.Count, forward);
            _forward = forward;
        }

        public void DropOnce(Action<ITile> done)
        {
            if (_citizens.Count == 0)
            {
                Debug.LogError("Citizen List is empty. Please make sure you Take() the correct container");
                return;
            }
            for (var i = 0; i < _citizens.Count; i++)
            {
                IEnumerable<TileAdapter> GetVisitedPoints(int citizenIndex)
                {
                    for (var j = 0; j < citizenIndex + 1; j++)
                    {
                        var visitedTileIndex = _boardTraveller.GetIndexAtStep(j + 1);
                        var visitedTile = _tileSpace[visitedTileIndex];
                        yield return visitedTile;//visitedTile.GetGridPosition(visitedTile.HeldPieces.Count + citizenIndex - j);
                    }
                }

                var citizen = _citizens[i];

                citizen.CitizenMove.JumpingMove(GetVisitedPoints(i),OnCitizenReachedTheTargetTile, i * 0.15f);

                var tile = _tileSpace[_boardTraveller.GetIndexAtStep(i + 1)].Tile;

                citizen.TargetTile.SetValue(tile);
            }

            _done = done;
        }

        private void OnCitizenReachedTheTargetTile(ICitizen citizen)
        {
            citizen.TargetTile.Value.AddPiece(citizen);
            _citizens.Remove(citizen);

            if (_citizens.Count == 0)
            {
                _done?.Invoke(citizen.TargetTile.Value);
            }
        }

        public void DropTillDawn(Action<IPieceDropper, ITile> onDone)
        {
            DropOnce(t => ContinueDropping(onDone, t.TileIndex));
        }

        public void Cleanup()
        {
            if (_citizens.Count > 0)
            {
                _citizens.Clear();
            }

            _tileSpace = null;
            _done = null;
        }

        private void ContinueDropping(Action<IPieceDropper, ITile> done, int index)
        {
            var successTile = _tileSpace[BoardTraveller.MoveNext(index, _tileSpace.Count, _forward)].Tile;
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

                TakeAll(successTile);
                SetMoveStartPoint(_tileSpace, successTile.TileIndex, _forward);
                DropOnce(t => ContinueDropping(done, t.TileIndex));
            }
            else
            {
                done?.Invoke(this, _tileSpace[index].Tile);
            }
        }
    }
}