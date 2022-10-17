using UnityEngine;

namespace Gameplay.Board
{
    public class BoardTraveller
    {
        private int _steps;
        private int _stepCount = -1;
        private int _number;
        public int CurrentIndex { get; private set; } = -1;

        public void Start(int startIndex, int steps, int number)
        {
            CurrentIndex = startIndex;
            _number = number;
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
            CurrentIndex = Mod(forward ? CurrentIndex + 1 : CurrentIndex - 1, _number);
        }

        public void Reset()
        {
            CurrentIndex = -1;
        }

        private static int Mod(int x, int m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}