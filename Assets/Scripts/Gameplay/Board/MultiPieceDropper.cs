using System;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Board
{
    public interface IMultiPieceDropper
    {
        void DropConcurrently(IReadOnlyList<ITile> tileSpace, PieceBench pieceBench, IReadOnlyList<Drop> drops, Action doneCallback);
    }

    public class MultiPieceDropper : IMultiPieceDropper
    {
        private IReadOnlyList<Drop> _drops;

        private IPieceDropper[] _pieceDroppers;

        private PieceBench _pieceBench;
        private IReadOnlyList<ITile> _tileSpace;
        private int _doneCount;
        private Action _doneCallback;

        public void DropConcurrently(IReadOnlyList<ITile> tileSpace, PieceBench pieceBench, IReadOnlyList<Drop> drops, Action doneCallback)
        {
            _doneCount = 0;
            _tileSpace = tileSpace;
            _pieceBench = pieceBench;
            _drops = drops.Where(d => _tileSpace[d.TileIndex].HeldPieces.Count > 0).ToArray();
            _doneCallback = doneCallback;
            _pieceDroppers = new IPieceDropper[_drops.Count];
            for (var i = 0; i < _drops.Count; i++)
            {
                _drops[i].DropIndex = i;
                _pieceDroppers[i] = new PieceDropper();
                _pieceDroppers[i].Setup(tileSpace);
                _pieceDroppers[i].TakeAll(tileSpace[_drops[i].TileIndex]);
                _pieceDroppers[i].SetMoveStartPoint(_drops[i].TileIndex, _drops[i].DropDirection);
                _pieceDroppers[i].DropTillDawn(OnDropDone);
            }
        }

        private void OnDropDone(IPieceDropper dropper, ITile tile)
        {
            var i = Array.IndexOf(_pieceDroppers, dropper);
            if (!new PieceEater().TryEat(_tileSpace, _pieceBench, tile.TileIndex, _drops[i].DropDirection, OnEatDone))
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

    public class Drop
    {
        public int TileIndex;
        public bool DropDirection;
        public int DropIndex;
    }
}