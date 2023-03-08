using System;
using Gameplay.CoreGameplay.Gateway;

namespace Gameplay.CoreGameplay
{
    public partial class CoreGameplayLauncher
    {
        private class CoreGameplayDataAccess : ICoreGameplayDataAccess
        {
            private IGameplayContainer _gameplayContainer;

            public void SetContainer(IGameplayContainer container)
            {
                _gameplayContainer = container;
            }
            
            public BoardData GetBoardData()
            {
                var matchData = _gameplayContainer.MatchData;
                return new BoardData
                {
                    NumSides = matchData.playerNum,
                    TilesPerSide = matchData.tilesPerGroup,
                    PiecesPerTile = matchData.numCitizensInTile
                };
            }
        }
    }
}