using System.Collections.Generic;
using System.Linq;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.Visual.Views;
using UnityEngine;

namespace Gameplay.Visual.Presenters
{
    public class ConcurrentSimulationResultPresenter :
        SelfBindingGenericDependencyInversionUnit<IConcurrentMoveSimulationResultHandler>,
        IConcurrentMoveSimulationResultHandler,
        IRefreshResultHandler
    {
        private readonly Dictionary<int, List<ConcurrentItem>> _dictionary = new();

        private int _threadId;
        private TurnDataExtractor _turnDataExtractor;
        private MultiThreadPiecesMovingRunner _movingRunner;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _turnDataExtractor = Resolver.Resolve<TurnDataExtractor>();
            _movingRunner = Resolver.Resolve<MultiThreadPiecesMovingRunner>();
        }

        public void OnSimulationProgress(int threadId, MoveSimulationProgressData result)
        {
            _threadId = threadId;
            if (!_dictionary.ContainsKey(threadId))
            {
                _dictionary.Add(threadId, new List<ConcurrentItem>());
            }

            _dictionary[threadId].Add(CreateConcurrentItem(threadId, result));
        }

        private ConcurrentItem CreateConcurrentItem(int threadId, MoveSimulationProgressData result)
        {
            return new ConcurrentItem
            {
                MoveType = result.MoveType,
                TargetPieceContainerIndex = result.TileIndex,
                TurnIndex = _turnDataExtractor.ExtractedTurnData.CurrentTurnIndex,
                ThreadId = threadId,
            };
        }

        public void OnSimulationResult(MoveSimulationResultData result)
        {
            // _controller.CheckBranching();
            _movingRunner.RunTheMoves(CreateMovingSteps(), _dictionary.Keys.Count);
            _dictionary.Clear();
        }

        private IReadOnlyList<ConcurrentMovingStep> CreateMovingSteps()
        {
            var concurrentMovingSteps = new List<ConcurrentMovingStep>();
            var stepIndex = 0;
            while (true)
            {
                var concurrentItems = new List<ConcurrentItem>();
                var added = false;

                foreach (var threadId in _dictionary.Keys.Where(threadId => stepIndex < _dictionary[threadId].Count))
                {
                    concurrentItems.Add(_dictionary[threadId][stepIndex]);
                    added = true;
                }

                if (!added) break;

                stepIndex++;
                concurrentMovingSteps.Add(new ConcurrentMovingStep
                {
                    ConcurrentItems = concurrentItems.ToArray()
                });
            }

            return concurrentMovingSteps;
        }

        public void HandleRefreshData(RefreshData refreshData)
        {
            var str = $"{_threadId}: ";
            str = refreshData.PiecesInTiles.Aggregate(str,
                (current, pieces) => current + $"{pieces.CitizenPiecesCount + pieces.MandarinPiecesCount} ");
            Debug.Log(str);
        }
    }

    public class ConcurrentMovingStep
    {
        public ConcurrentItem[] ConcurrentItems;
    }

    public class ConcurrentItem : SingleMovingStep
    {
        public int ThreadId;
    }
}