using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Framework.Entities.Variable;
using Gameplay.Piece;
using Gameplay.Piece.Activities;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IPieceDropper
    {
        void Setup(IReadOnlyList<ITile> tileSpace);
        void Take(IPieceContainer container, int amount);
        void TakeAll(IPieceContainer container);
        void SetMoveStartPoint(int index, bool forward);
        void DropOnce(Action<ITile> done);
        void DropTillDawn(Action<IPieceDropper, ITile> onDone);
        void Cleanup();
    }

    public class TileAdapter
    {
        public TileAdapter(ITile tile)
        {
            Tile = tile;
        }

        public ITile Tile { get; }
        public IVariable<int> VisitorCount { get; } = new Variable<int>();
    }

    public class PieceDropper : IPieceDropper
    {
        private readonly BoardTraveller _boardTraveller = new();
        private readonly List<ICitizen> _citizens = new();

        private bool _forward;
        private IReadOnlyList<TileAdapter> _tileSpace;
        private Action<ITile> _done;

        public void Setup(IReadOnlyList<ITile> tiles)
        {
            _tileSpace = tiles.Select(t => new TileAdapter(t)).ToArray();
        }

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

        public void SetMoveStartPoint(int index, bool forward)
        {
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
                        yield return _tileSpace[_boardTraveller.GetIndexAtStep(j + 1)];
                    }
                }

                var citizen = _citizens[i];

                citizen.CitizenMove.JumpingMove(GetVisitedPoints(i), OnCitizenReachedTheTargetTile, i * 0.15f);
                citizen.TargetTile.SetValue(_tileSpace[_boardTraveller.GetIndexAtStep(i + 1)].Tile);
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
                SetMoveStartPoint(successTile.TileIndex, _forward);
                DropOnce(t => ContinueDropping(done, t.TileIndex));
            }
            else
            {
                done?.Invoke(this, _tileSpace[index].Tile);
            }
        }
    }
}