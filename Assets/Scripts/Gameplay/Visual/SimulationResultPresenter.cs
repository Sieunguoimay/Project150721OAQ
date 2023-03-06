using System;
using System.Collections.Generic;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Visual.Board;

namespace Gameplay.Visual
{
    public class SimulationResultPresenter : IBoardMoveSimulationResultHandler
    {
        private readonly CoreGameplayVisualPresenter _handler;
        private readonly List<MoveSimulationProgressData> _simulationSteps = new();

        public event Action<SimulationResultPresenter, IReadOnlyList<MovingStep>> MoveStepsAvailableEvent;

        public SimulationResultPresenter(CoreGameplayVisualPresenter handler)
        {
            _handler = handler;
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
            MoveStepsAvailableEvent?.Invoke(this, movingSteps);
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
                        TargetPieceContainer = _handler.BoardVisual.Tiles[s.TileIndex]
                    };
                }

                prevTileIndex = s.TileIndex;
                prevAmount = s.NumCitizens;
            }

            return movingSteps;
        }

        private MovingStep CreateMovingStepForDrop(MoveType moveType, int prevTileIndex, int tileIndex,
            int prevAmount)
        {
            return new()
            {
                MoveType = moveType,
                PieceContainer = prevTileIndex >= 0 ? _handler.BoardVisual.Tiles[prevTileIndex] : null,
                TargetPieceContainer = _handler.BoardVisual.Tiles[tileIndex],
                RemainingPieces = prevAmount
            };
        }
    }
}

public class MovingStep
{
    public MoveType MoveType;
    public IPieceContainer TargetPieceContainer;
    public IPieceContainer PieceContainer;
    public int RemainingPieces;
}

