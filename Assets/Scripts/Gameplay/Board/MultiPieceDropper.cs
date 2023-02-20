using System;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Board
{
    [Obsolete]
    public interface IMultiPieceDropper
    {
        void DropConcurrently(IReadOnlyList<ITile> tileSpace, PieceBench pieceBench, IReadOnlyList<int> drops, Action doneCallback, bool direction);
    }

    [Obsolete]
    public class MultiPieceDropper : IMultiPieceDropper
    {
        private IReadOnlyList<int> _drops;

        private IPieceDropper[] _pieceDroppers;

        private PieceBench _pieceBench;
        private IReadOnlyList<ITile> _tileSpace;
        private int _doneCount;
        private Action _doneCallback;

        public void DropConcurrently(IReadOnlyList<ITile> tileSpace, PieceBench pieceBench, IReadOnlyList<int> drops, Action doneCallback, bool direction)
        {
            _doneCount = 0;
            _tileSpace = tileSpace;
            _pieceBench = pieceBench;
            _drops = drops.Where(d => _tileSpace[d].HeldPieces.Count > 0).ToArray();
            _doneCallback = doneCallback;
            _pieceDroppers = new IPieceDropper[_drops.Count];
            for (var i = 0; i < _drops.Count; i++)
            {
                _pieceDroppers[i] = new PieceDropper();
                _pieceDroppers[i].Setup(tileSpace);
                _pieceDroppers[i].TakeAll(tileSpace[_drops[i]]);
                _pieceDroppers[i].SetMoveStartPoint(_drops[i], direction);
                _pieceDroppers[i].DropTillDawn(OnDropDone);
            }
        }

        private void OnDropDone(IPieceDropper dropper, ITile tile)
        {
            var i = Array.IndexOf(_pieceDroppers, dropper);
            if (!new PieceEater().TryEat(_tileSpace, _pieceBench, tile.TileIndex, dropper.Direction, OnEatDone))
            {
                OnEatDone();
            }

            void OnEatDone()
            {
                _doneCount++;

                if (_doneCount == _drops.Count)
                {
                    _doneCallback?.Invoke();
                }
            }
        }
    }
}