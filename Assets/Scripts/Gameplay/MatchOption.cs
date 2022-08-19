using System;

namespace Gameplay
{
    public interface IMatchOption
    {
        int PlayerNum { get; }
        int TilesPerGroup { get; }
    }

    public class MatchOption : IMatchOption
    {
        public int PlayerNum { get; private set; } = 3;
        public int TilesPerGroup { get; private set; } = 5;
    }
}