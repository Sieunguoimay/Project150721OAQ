using System;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public interface ISimulatorFactory
    {
        public void CreateAllBoardSimulators(BoardEntityAccess boardEntityAccess);
        public IBoardMoveSimulator GetSimulator(BoardActionType actionType);
    }

    public class SimulatorFactory : DependencyInversionScriptableObjectNode, ISimulatorFactory
    {
        private BoardMoveSimulator _boardMoveSimulator;
        private BoardMoveSimulator _goneWithTheWindSimulator;
        private ConcurrentBoardMoveSimulator _concurrentBoardMoveSimulator;
        private IBoardMoveSimulationResultHandler _simulationResultHandler;
        private IConcurrentMoveSimulationResultHandler _concurrentSimulationResultHandler;

        protected override Type GetBindingType()
        {
            return typeof(ISimulatorFactory);
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _simulationResultHandler = Resolver.Resolve<IBoardMoveSimulationResultHandler>();
            _concurrentSimulationResultHandler = Resolver.Resolve<IConcurrentMoveSimulationResultHandler>();
        }

        public void CreateAllBoardSimulators(BoardEntityAccess boardEntityAccess)
        {
            var moveMakerFactory = new MoveMakerFactory(boardEntityAccess);
            _boardMoveSimulator = new BoardMoveSimulator(_simulationResultHandler, moveMakerFactory.CreateMoveMaker());
            _goneWithTheWindSimulator =
                new BoardMoveSimulator(_simulationResultHandler, moveMakerFactory.CreateMoveMaker());
            _concurrentBoardMoveSimulator =
                new ConcurrentBoardMoveSimulator(_concurrentSimulationResultHandler, moveMakerFactory);
        }

        public IBoardMoveSimulator GetSimulator(BoardActionType actionType)
        {
            return actionType switch
            {
                BoardActionType.Basic => _boardMoveSimulator,
                BoardActionType.GoneWithTheWind => _goneWithTheWindSimulator,
                BoardActionType.Concurrent => _concurrentBoardMoveSimulator,
                _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
            };
        }
    }

    public class MoveMakerFactory
    {
        private readonly BoardEntityAccess _boardEntityAccess;

        public MoveMakerFactory(BoardEntityAccess boardEntityAccess)
        {
            _boardEntityAccess = boardEntityAccess;
        }

        public MoveMaker CreateMoveMaker()
        {
            var moveMaker = new MoveMaker("_");
            moveMaker.SetBoardEntityAccess(_boardEntityAccess);
            return moveMaker;
        }
    }
}