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
                // _gameplay.UpdateTurn();
                // CheckPiecesOnCurrentSide();
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
            
        }
    }
}