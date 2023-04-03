using Framework.DependencyInversion;
using Gameplay.Entities.Stage;
using Gameplay.Entities.Stage.StageSelector;

namespace Gameplay.CoreGameplay.Gateway
{
    public class CoreGameplayDataAccess : 
        SelfBindingGenericDependencyInversionUnit<ICoreGameplayDataAccess>,
        ICoreGameplayDataAccess
    {
        private IStageSelector _stageSelector;
        private MatchData _matchData;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _stageSelector = Resolver.Resolve<IStageSelector>("stage_selector");
        }

        public void RefreshData()
        {
            _matchData = _stageSelector.SelectedStage.Data.MatchData;
        }
        
        public BoardData GetBoardData()
        {
            return new()
            {
                NumSides = _matchData.playerNum,
                TilesPerSide = _matchData.tilesPerGroup,
                PiecesPerTile = _matchData.numCitizensInTile
            };
        }

        public TurnData GetTurnData()
        {
            return new()
            {
                InitialTurnIndex = 0,
                NumTurns = _matchData.playerNum
            };
        }
    }
}