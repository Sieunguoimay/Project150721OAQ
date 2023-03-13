using System.Collections.Generic;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Visual.Views;

namespace Gameplay.Visual
{
    public class SimulationResultPresenter : IBoardMoveSimulationResultHandler
    {
        private readonly Dictionary<int, List<MoveSimulationProgressData>> _simulationSteps = new();

        private PiecesMovingRunner _movingRunner;

        public void SetMovingRunner(PiecesMovingRunner value)
        {
            _movingRunner = value;
        }

        public void OnSimulationResult(MoveSimulationResultData result)
        {
            ExtractMovingSteps();
        }

        public void OnSimulationProgress(int simulationId, MoveSimulationProgressData result)
        {
            if (_simulationSteps.ContainsKey(simulationId))
            {
                _simulationSteps[simulationId].Add(result);
            }
            else
            {
                _simulationSteps.Add(simulationId, new List<MoveSimulationProgressData> {result});
            }
        }

        private void ExtractMovingSteps()
        {
            var movingSteps = GenerateMovingSteps(); // GenerateMovingSteps(_simulationSteps[0]);
            _movingRunner?.RunTheMoves(movingSteps);
            _simulationSteps.Clear();
        }

        private MovingStep[] GenerateMovingSteps()
        {
            var combinedMovingSteps = new List<MovingStep>();
            foreach (var simulationId in _simulationSteps.Keys)
            {
                var movingSteps = GenerateMovingSteps(_simulationSteps[simulationId]);
                for (var i = 0; i < movingSteps.Length; i++)
                {
                    var movingStep = movingSteps[i];
                    if (i >= combinedMovingSteps.Count)
                    {
                        combinedMovingSteps.Add(movingStep);
                    }
                    else
                    {
                        combinedMovingSteps[i].StepActionItems.AddRange(movingStep.StepActionItems);
                    }
                }
            }

            return combinedMovingSteps.ToArray();
        }

        private static MovingStep[] GenerateMovingSteps(IReadOnlyList<MoveSimulationProgressData> simulationSteps)
        {
            var movingSteps = new MovingStep[simulationSteps.Count];
            var prevTileIndex = -1;
            var prevAmount = 0;
            for (var i = 0; i < simulationSteps.Count; i++)
            {
                var s = simulationSteps[i];
                movingSteps[i] = new MovingStep
                {
                    StepActionItems = new List<StepActionItem>
                    {
                        CreateStepActionItem(s.MoveType, prevTileIndex, s.TileIndex, prevAmount)
                    }
                };

                prevTileIndex = s.TileIndex;
                prevAmount = s.NumCitizens + s.NumMandarins;
            }

            return movingSteps;
        }

        private static StepActionItem CreateStepActionItem(MoveType moveType, int prevTileIndex, int tileIndex, int prevAmount)
        {
            if (moveType == MoveType.Drop)
            {
                return CreateStepActionItemForDrop(moveType, prevTileIndex, tileIndex, prevAmount);
            }

            return new StepActionItem
            {
                MoveType = moveType,
                TargetPieceContainerIndex = tileIndex
            };
        }

        private static StepActionItem CreateStepActionItemForDrop(MoveType moveType, int prevTileIndex, int tileIndex,
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
    public List<StepActionItem> StepActionItems;
}

public class StepActionItem
{
    public MoveType MoveType;
    public int TargetPieceContainerIndex;
    public int PieceContainerIndex;
    public int RemainingPieces;
}