using System.Collections.Generic;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Visual.Views;

namespace Gameplay.Visual
{
    public class SimulationResultPresenter : IBoardMoveSimulationResultHandler
    {
        private readonly List<MoveSimulationProgressData> _simulationSteps = new();

        public PiecesMovingRunner MovingRunner { get; set; }

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

        private MovingStep[] GenerateMovingSteps()
        {
            var movingSteps = new MovingStep[_simulationSteps.Count];
            var prevTileIndex = -1;
            var prevAmount = 0;
            for (var i = 0; i < _simulationSteps.Count; i++)
            {
                var s = _simulationSteps[i];
                if (s.MoveType == MoveType.Drop)
                {
                    movingSteps[i] = CreateMovingStepForDrop(s.MoveType, prevTileIndex, s.TileIndex, prevAmount);
                }
                else
                {
                    movingSteps[i] = new MovingStep
                    {
                        MoveType = s.MoveType,
                        TargetPieceContainerIndex = s.TileIndex
                    };
                }

                prevTileIndex = s.TileIndex;
                prevAmount = s.NumCitizens + s.NumMandarins;
            }

            return movingSteps;
        }

        private static MovingStep CreateMovingStepForDrop(MoveType moveType, int prevTileIndex, int tileIndex,
            int prevAmount)
        {
            return new()
            {
                MoveType = moveType,
                RemainingPieces = prevAmount,
                PieceContainerIndex = prevTileIndex,
                TargetPieceContainerIndex = tileIndex
            };
        }
    }
}

public class MovingStep
{
    public MoveType MoveType;
    public int TargetPieceContainerIndex;
    public int PieceContainerIndex;
    public int RemainingPieces;
}