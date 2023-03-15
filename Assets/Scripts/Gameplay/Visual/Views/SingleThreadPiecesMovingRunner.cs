using System.Collections.Generic;
using Gameplay.Visual.Board;

namespace Gameplay.Visual.Views
{
    public class SingleThreadPiecesMovingRunner : PiecesMovingRunner
    {
        private IReadOnlyList<MovingStep> _movingSteps;
        private readonly IPieceContainer _pieceContainer = new SimplePieceContainer();

        public void RunTheMoves(IReadOnlyList<MovingStep> movingSteps)
        {
            _movingSteps = movingSteps;
            StepIterator = 0;
            NextStep();
        }

        public override void ResetMovingSteps()
        {
            _movingSteps = null;
        }

        protected override void NextStep()
        {
            if (_movingSteps == null || _movingSteps.Count == 0) return;

            if (StepIterator < _movingSteps.Count)
            {
                var step = _movingSteps[StepIterator++];
                var executor = CreateStepExecutor(step.MoveType, step.TargetPieceContainerIndex);
                executor.Execute();
            }
            else
            {
                _movingSteps = null;
                OnAllMovingStepsExecutedEvent();
            }
        }

        protected override IPieceContainer GetTempPieceContainer()
        {
            return _pieceContainer;
        }

        protected override void OnStepExecutionDone()
        {
            NextStep();
        }
    }
}