using System;
using System.Linq;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Entities;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;

namespace Gameplay.CoreGameplay.Interactors
{
    public class CoreGameplayBranchingDriver : SelfBindingDependencyInversionUnit
    {
        private TurnDataExtractor _turnDataExtractor;
        //private DecisionMakingController _boardActionDecisionMakingDriver;
        private BoardEntityAccess _boardEntityAccess;
        private SimulationArgumentSelectionController _simulationArgumentSelectionController;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _turnDataExtractor = Resolver.Resolve<TurnDataExtractor>();
            //_boardActionDecisionMakingDriver = Resolver.Resolve<DecisionMakingController>();
            _boardEntityAccess = Resolver.Resolve<BoardEntityAccess>();
            _simulationArgumentSelectionController = Resolver.Resolve<SimulationArgumentSelectionController>();
        }

        public void RunBranching()
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
                //RunMoveDecisionMaking();
                _simulationArgumentSelectionController.StartSelectionSequence();
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

        //private void RunMoveDecisionMaking()
        //{
        //    _boardActionDecisionMakingDriver.MakeDecisionOfCurrentTurn();
        //}
    }
}