using System.Linq;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;

namespace Gameplay.CoreGameplay.Interactors
{
    public class CoreGameplayInteractDriver
    {
        private readonly TurnDataExtractor _turnDataExtractor;
        private readonly BoardActionBoardActionDecisionMakingDriver _boardActionBoardActionDecisionMakingDriver;
        private readonly BoardEntityAccess _boardEntityAccess;

        public CoreGameplayInteractDriver(
            TurnDataExtractor turnDataExtractor,
            BoardActionBoardActionDecisionMakingDriver boardActionBoardActionDecisionMakingDriver, 
            BoardEntityAccess boardEntityAccess)
        {
            _turnDataExtractor = turnDataExtractor;
            _boardActionBoardActionDecisionMakingDriver = boardActionBoardActionDecisionMakingDriver;
            _boardEntityAccess = boardEntityAccess;
        }

        public void CheckBranching()
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
            return _turnDataExtractor.ExtractedTurnData.CitizenTileEntitiesOfCurrentTurn
                .Sum(t => t.PieceEntities.Count) > 0;
        }

        private bool AnyPiecesAvailableOnBenchOfCurrentSide()
        {
            return _turnDataExtractor.ExtractedTurnData.PocketEntity.PieceEntities
                .Any(p => p.PieceType == PieceType.Citizen);
        }

        private void RunGameOver()
        {
        }

        private void RunTakingPiecesBackToBoard()
        {
        }

        private void RunMoveDecisionMaking()
        {
            _boardActionBoardActionDecisionMakingDriver.MakeDecisionOfCurrentTurn();
        }
    }
}