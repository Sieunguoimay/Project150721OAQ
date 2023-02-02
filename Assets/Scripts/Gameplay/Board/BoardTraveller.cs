using System.Collections;
using System.Collections.Generic;

namespace Gameplay.Board
{
    public class BoardTraveller : IEnumerator<ITile>
    {
        private int _stepCount = -1;
        private int _spaceSize;
        private int _currentIndex = -1;
        private bool _forward;

        private Board _board;

        public ITile Current => _currentIndex < 0 ? null : _board.Tiles[_currentIndex];
        object IEnumerator.Current => Current;


        public void Init(Board board, int startIndex, int spaceSize, bool forward)
        {
            _board = board;
            _currentIndex = startIndex;
            _spaceSize = spaceSize;
            _stepCount = 0;
            _forward = forward;
        }

        public bool MoveNext()
        {
            _stepCount++;
            _currentIndex = MoveNext(_currentIndex, _spaceSize, _forward);
            return true;
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            _currentIndex = -1;
        }

        public int GetIndexAtStep(int step, bool forward)
        {
            return MoveNext(_currentIndex, _spaceSize, forward, step);
        }

        public static int MoveNext(int indexInSpace, int spaceSize, bool forward, int step = 1)
        {
            return Mod(forward ? indexInSpace + step : indexInSpace - step, spaceSize);
        }

        private static int Mod(int x, int m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}