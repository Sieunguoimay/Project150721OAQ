using System.Collections.Generic;
using Gameplay.Visual.Board;
using Gameplay.Visual.Presenters;

namespace Gameplay.Visual.Views
{
    public class MultiThreadPiecesMovingRunner : PiecesMovingRunner
    {
        private IReadOnlyList<ConcurrentMovingStep> _movingSteps;
        private IPieceContainer[] _pieceContainers;
        private int _currentThreadIndex;
        
        private int _itemsPerStep;
        private int _itemDoneCount;

        public void RunTheMoves(IReadOnlyList<ConcurrentMovingStep> movingSteps, int maxThreads)
        {
            _movingSteps = movingSteps;
            StepIterator = 0;
            CreateTempPieceContainers(maxThreads);
            NextStep();
        }

        private void CreateTempPieceContainers(int maxThreads)
        {
            _pieceContainers = new IPieceContainer[maxThreads];
            for (var i = 0; i < _pieceContainers.Length; i++)
            {
                _pieceContainers[i] = new SimplePieceContainer();
            }
        }

        public override void ResetMovingSteps()
        {
            _movingSteps = null;
            _pieceContainers = null;
        }

        protected override void NextStep()
        {
            if (_movingSteps == null || _movingSteps.Count == 0) return;

            if (StepIterator < _movingSteps.Count)
            {
                var step = _movingSteps[StepIterator++];

                _itemsPerStep = step.ConcurrentItems.Length;
                _itemDoneCount = 0;
                
                foreach (var item in step.ConcurrentItems)
                {
                    var executor = CreateStepExecutor(item.MoveType, item.TargetPieceContainerIndex);
                    _currentThreadIndex = item.ThreadId;
                    executor.Execute();
                }
            }
            else
            {
                _movingSteps = null;
                OnAllMovingStepsExecutedEvent();
            }
        }

        protected override IPieceContainer GetTempPieceContainer()
        {
            return _pieceContainers[_currentThreadIndex];
        }

        protected override void OnStepExecutionDone()
        {
            _itemDoneCount++;
            if (_itemDoneCount >= _itemsPerStep)
            {
                NextStep();
            }
        }
    }
}