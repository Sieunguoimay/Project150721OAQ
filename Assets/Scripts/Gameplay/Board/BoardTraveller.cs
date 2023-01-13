using UnityEngine;

namespace Gameplay.Board
{
    public class BoardTraveller
    {
        private int _steps;
        private int _stepCount = -1;
        private int _spaceSize;
        public int CurrentIndex { get; private set; } = -1;

        public void Start(int startIndex, int steps, int spaceSize)
        {
            CurrentIndex = startIndex;
            _spaceSize = spaceSize;
            _steps = steps;
            _stepCount = 0;
        }

        public void Next(bool forward)
        {
            var isTravelling = _stepCount >= 0 && _stepCount < _steps;

            if (!isTravelling)
            {
                Debug.Log("Bug... " + _stepCount + " " + _steps);
                return;
            }

            _stepCount++;

            CurrentIndex = MoveNext(CurrentIndex, _spaceSize, forward); 
        }

        public void Reset()
        {
            CurrentIndex = -1;
        }

        public static int MoveNext(int indexInSpace, int spaceSize, bool forward)
        {
            return Mod(forward ? indexInSpace + 1 : indexInSpace - 1, spaceSize);
        }

        private static int Mod(int x, int m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}