using System.Linq;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;

namespace Gameplay.CoreGameplay.Interactors.Driver
{
    public class GameplayTaskDistributor : IRefreshResultHandler
    {
        private readonly TurnDataExtractor _turnDataExtractor;
        private readonly MoveMoveDecisionMakingDriver _moveMoveDecisionMakingDriver;
        private readonly BoardEntityAccess _boardEntityAccess;

        public GameplayTaskDistributor(TurnDataExtractor turnDataExtractor, MoveMoveDecisionMakingDriver moveMoveDecisionMakingDriver, BoardEntityAccess boardEntityAccess)
        {
            _turnDataExtractor = turnDataExtractor;
            _moveMoveDecisionMakingDriver = moveMoveDecisionMakingDriver;
            _boardEntityAccess = boardEntityAccess;
        }

        public void Distribute()
        {
            CheckPiecesInMandarinTiles();
        }

        private void CheckPiecesInMandarinTiles()
        {
            if (AnyMandarinTileHasPieces())
            {
                _turnDataExtractor.NextTurn();
                
                CheckPiecesOnCurrentSide();
            }
            else
            {
                RunGameOver();
            }
        }

        private void CheckPiecesOnCurrentSide()
        {
            if (AnyTileOnCurrentSideHasPieces())
            {
                RunMoveDecisionMaking();
            }
            else
            {
                CheckBenchOnCurrentSideForPieces();
            }
        }

        private void CheckBenchOnCurrentSideForPieces()
        {
            if (AnyPiecesAvailableOnBenchOfCurrentSide())
            {
                RunTakingPiecesBackToBoard();
            }
            else
            {
                RunGameOver();
            }
        }
        
        private bool AnyMandarinTileHasPieces()
        {
            return _boardEntityAccess.Board.MandarinTiles.Any(m => m.PieceEntities.Count > 0);
        }
        private bool AnyTileOnCurrentSideHasPieces()
        {
            return false; //_boardStateView.CheckAnyCitizenTileOnSideHasPieces(_turnTeller.CurrentTurn.SideIndex);
        }

        private bool AnyPiecesAvailableOnBenchOfCurrentSide()
        {
            return false;//_boardStateView.CheckBenchOnSideHasPieces(_turnTeller.CurrentTurn.SideIndex);
        }

        public void HandleRefreshData(RefreshData refreshData)
        {
        }

        private void RunGameOver()
        {
        }

        private void RunTakingPiecesBackToBoard()
        {
        }

        private void RunMoveDecisionMaking()
        {
            _moveMoveDecisionMakingDriver.MakeDecisionOfCurrentTurn();
        }
    }
}