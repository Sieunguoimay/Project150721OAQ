using System.Collections.Generic;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Visual.Views;

namespace Gameplay.Visual
{
    public class SimulationResultPresenter :
        SelfBindingGenericDependencyInversionUnit<IBoardMoveSimulationResultHandler>,
        IBoardMoveSimulationResultHandler
    {
        private readonly List<MoveSimulationProgressData> _simulationSteps = new();
        private TurnDataExtractor _turnDataExtractor;
        private SingleThreadPiecesMovingRunner _movingRunner;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _movingRunner = Resolver.Resolve<SingleThreadPiecesMovingRunner>();
            _turnDataExtractor = Resolver.Resolve<TurnDataExtractor>();
        }

        public void OnSimulationResult(MoveSimulationResultData result)
        {
            ExtractMovingSteps();
        }

        public void OnSimulationProgress(MoveSimulationProgressData result)
        {
            _simulationSteps.Add(result);
        }

        private void ExtractMovingSteps()
        {
            var movingSteps = GenerateMovingSteps();
            _movingRunner?.RunTheMoves(movingSteps);
            _simulationSteps.Clear();
        }

        private SingleMovingStep[] GenerateMovingSteps()
        {
            var movingSteps = new SingleMovingStep[_simulationSteps.Count];
            for (var i = 0; i < _simulationSteps.Count; i++)
            {
                var s = _simulationSteps[i];
                if (s.MoveType == MoveType.DoubleGrasp)
                {
                    movingSteps[i] = CreateDoubleGraspMovingStep(s);
                }
                else
                {
                    movingSteps[i] = CreateMovingStep(s);
                }
            }

            return movingSteps;
        }

        private SingleMovingStep CreateMovingStep(MoveSimulationProgressData progressData)
        {
            return new SingleMovingStep
            {
                MoveType = progressData.MoveType,
                TargetPieceContainerIndex = progressData.TileIndex,
                TurnIndex = _turnDataExtractor.ExtractedTurnData.CurrentTurnIndex,
            };
        }

        private DoubleGraspMovingStep CreateDoubleGraspMovingStep(MoveSimulationProgressData progressData)
        {
            return new DoubleGraspMovingStep
            {
                MoveType = progressData.MoveType,
                TargetPieceContainerIndex = progressData.TileIndex,
                TargetPieceContainerIndex2 = progressData.NextTileIndex,
                TurnIndex = _turnDataExtractor.ExtractedTurnData.CurrentTurnIndex,
            };
        }
    }
}

public class SingleMovingStep
{
    public MoveType MoveType;
    public int TargetPieceContainerIndex;
    public int TurnIndex;
}

public class DoubleGraspMovingStep : SingleMovingStep
{
    public int TargetPieceContainerIndex2;
}