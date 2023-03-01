namespace Gameplay.Helpers
{
    public static class BoardTraveller
    {
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