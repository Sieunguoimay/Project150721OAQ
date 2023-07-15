using System;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public interface ISimulatorManager
    {
        public void CreateAllBoardSimulators();
        public IBoardMoveSimulator GetSimulator(SimulationType actionType);
    }
    
    public class SimulatorManager : ScriptableEntity, ISimulatorManager
    {
        private BoardMoveSimulator _boardMoveSimulator;
        private BoardMoveSimulator _goneWithTheWindSimulator;
        private ConcurrentBoardMoveSimulator _concurrentBoardMoveSimulator;
        private IBoardMoveSimulationResultHandler _simulationResultHandler;
        private IConcurrentMoveSimulationResultHandler _concurrentSimulationResultHandler;
        private BoardEntityAccess _boardEntityAccess;

        protected override Type GetBindingType()
        {
            return typeof(ISimulatorManager);
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _simulationResultHandler = Resolver.Resolve<IBoardMoveSimulationResultHandler>();
            _concurrentSimulationResultHandler = Resolver.Resolve<IConcurrentMoveSimulationResultHandler>();
            _boardEntityAccess = Resolver.Resolve<BoardEntityAccess>();
        }

        public void CreateAllBoardSimulators()
        {
            var moveMakerFactory = new MoveMakerFactory(_boardEntityAccess);
            _boardMoveSimulator =
                new BoardMoveSimulator(_simulationResultHandler, moveMakerFactory.CreateMoveMaker());
            _goneWithTheWindSimulator =
                new BoardMoveSimulator(_simulationResultHandler, moveMakerFactory.CreateMoveMaker());
            _concurrentBoardMoveSimulator =
                new ConcurrentBoardMoveSimulator(_concurrentSimulationResultHandler, moveMakerFactory);
        }

        public IBoardMoveSimulator GetSimulator(SimulationType simulationType)
        {
            return simulationType switch
            {
                SimulationType.Basic => _boardMoveSimulator,
                SimulationType.GoneWithTheWind => _goneWithTheWindSimulator,
                SimulationType.Concurrent => _concurrentBoardMoveSimulator,
                _ => throw new ArgumentOutOfRangeException(nameof(simulationType), simulationType, null)
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