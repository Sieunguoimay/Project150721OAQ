using System.Collections.Generic;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Visual.Views;

namespace Gameplay.Visual
{
    public class SimulationResultPresenter : IBoardMoveSimulationResultHandler
    {
        private readonly List<MoveSimulationProgressData> _simulationSteps = new();

        public SingleThreadPiecesMovingRunner MovingRunner { get; set; }

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
            MovingRunner?.RunTheMoves(movingSteps);
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

        private static SingleMovingStep CreateMovingStep(MoveSimulationProgressData progressData)
        {
            return new SingleMovingStep
            {
                MoveType = progressData.MoveType,
                TargetPieceContainerIndex = progressData.TileIndex
            };
        }

        private static DoubleGraspMovingStep CreateDoubleGraspMovingStep(MoveSimulationProgressData progressData)
        {
            return new DoubleGraspMovingStep
            {
                MoveType = progressData.MoveType,
                TargetPieceContainerIndex = progressData.TileIndex,
                TargetPieceContainerIndex2 = progressData.NextTileIndex,
            };
        }
    }
}

public class SingleMovingStep
{
    public MoveType MoveType;
    public int TargetPieceContainerIndex;
}

public class DoubleGraspMovingStep : SingleMovingStep
{
    public int TargetPieceContainerIndex2;
}