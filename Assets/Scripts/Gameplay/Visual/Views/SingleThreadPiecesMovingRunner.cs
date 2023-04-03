using System;
using System.Collections.Generic;
using Gameplay.Visual.Board;

namespace Gameplay.Visual.Views
{
    public class SingleThreadPiecesMovingRunner : PiecesMovingRunner
    {
        private IReadOnlyList<SingleMovingStep> _movingSteps;
        private readonly IPieceContainer _pieceContainer = new SimplePieceContainer();
        private MovingStepExecutor _executor;

        protected override Type GetBindingType()
        {
            return typeof(SingleThreadPiecesMovingRunner);
        }

        public void RunTheMoves(IReadOnlyList<SingleMovingStep> movingSteps)
        {
            _movingSteps = movingSteps;
            StepIterator = 0;
            NextStep();
        }

        public override void ResetMovingSteps()
        {
            _pieceContainer.Clear();
            _movingSteps = null;
            if (_executor != null)
            {
                _executor.Cleanup();
                _executor = null;
            }
        }

        protected override void NextStep()
        {
            if (_movingSteps == null || _movingSteps.Count == 0) return;

            if (StepIterator < _movingSteps.Count)
            {
                var step = _movingSteps[StepIterator++];
                _executor = CreateStepExecutor(step);
                _executor.Execute();
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
            _executor = null;
            NextStep();
        }
    }
}