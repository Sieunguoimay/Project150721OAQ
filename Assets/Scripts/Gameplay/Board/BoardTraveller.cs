namespace Gameplay.Board
{
    public class BoardTraveller
    {
        private int _spaceSize;
        private int _currentIndex = -1;
        private bool _forward;

        public void Init(int startIndex, int spaceSize, bool forward)
        {
            _currentIndex = startIndex;
            _spaceSize = spaceSize;
            _forward = forward;
        }

        public int GetIndexAtStep(int step)
        {
            return MoveNext(_currentIndex, _spaceSize, _forward, step);
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