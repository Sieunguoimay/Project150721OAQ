namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public interface IBoardMoveSimulator
    {
        void RunSimulation(MoveSimulationInputData inputData);
    }

    public class BoardMoveSimulator : IBoardMoveSimulator
    {
        private readonly IBoardMoveSimulationResultHandler _simulationResultHandler;
        private readonly BoardStateMachine _boardStateMachine;
        private readonly MoveMaker _moveMaker;

        public BoardMoveSimulator(IBoardMoveSimulationResultHandler resultHandler, MoveMaker moveMaker)
        {
            _simulationResultHandler = resultHandler;
            _moveMaker = moveMaker;
            _moveMaker.SetProgressHandler(OnSimulationProgress);
            _boardStateMachine = new BoardStateMachine(_moveMaker);
            _boardStateMachine.SetEndHandler(OnBoardStateMachineEnd);
        }

        public void RunSimulation(MoveSimulationInputData inputData)
        {
            _moveMaker.SetStartingCondition(inputData.SideIndex, inputData.StartingTileIndex, inputData.Direction);
            _boardStateMachine.NextAction();
        }
        private void OnSimulationProgress(MoveMaker arg1, MoveSimulationProgressData arg2)
        {
            _simulationResultHandler?.OnSimulationProgress(arg2);
        }

        private void OnBoardStateMachineEnd()
        {
            _simulationResultHandler.OnSimulationResult(new MoveSimulationResultData());
        }
    }

    public class MoveSimulationResultData
    {
    }

    public class MoveSimulationProgressData
    {
        public MoveType MoveType;
        public int TileIndex;
        public int NextTileIndex;
    }

    public enum MoveType
    {
        Grasp,
        Drop,
        Slam,
        Eat,
        DoubleGrasp,
    }

    public interface IBoardMoveSimulationResultHandler
    {
        void OnSimulationProgress(MoveSimulationProgressData result);
        void OnSimulationResult(MoveSimulationResultData result);
    }
}