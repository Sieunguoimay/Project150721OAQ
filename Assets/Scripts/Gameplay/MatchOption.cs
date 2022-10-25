using System;

namespace Gameplay
{
    public interface IMatchOption
    {
        int PlayerNum { get; }
        int TilesPerGroup { get; }
    }
    public interface IMatchChooser
    {
        public IMatchOption MatchOption { get; }
        event Action OnMatchOptionChanged;
    }

    public class MatchChooser : IMatchChooser
    {
        public IMatchOption MatchOption { get; private set; }
        public event Action OnMatchOptionChanged;

        public void SetMatchOption(IMatchOption matchOption)
        {
            MatchOption = matchOption;
            OnMatchOptionChanged?.Invoke();
        }
    }
}