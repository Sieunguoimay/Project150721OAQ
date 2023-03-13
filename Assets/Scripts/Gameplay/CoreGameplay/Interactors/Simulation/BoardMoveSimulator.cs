namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public class BoardMoveSimulator 
    {
        private readonly IBoardMoveSimulationResultHandler _simulationResultHandler;
        private readonly BoardStateMachine _boardStateMachine;
        private readonly MoveMaker _moveMaker;

        public BoardMoveSimulator(IBoardMoveSimulationResultHandler resultHandler,
            BoardEntityAccess boardEntityAccess)
        {
            _simulationResultHandler = resultHandler;
            _moveMaker = new MoveMaker(OnSimulationProgress, boardEntityAccess);
            _boardStateMachine = new BoardStateMachine(_moveMaker);
            _boardStateMachine.SetEndHandler(OnBoardStateMachineEnd);
        }

        public void RunSimulation(MoveSimulationInputData inputData)
        {
            _moveMaker.Initialize(inputData.SideIndex, inputData.StartingTileIndex, inputData.Direction);
            _boardStateMachine.NextAction();
        }

        private void OnSimulationProgress(MoveMaker arg1, MoveSimulationProgressData arg2)
        {
            _simulationResultHandler?.OnSimulationProgress(0, arg2);
        }

        private void OnBoardStateMachineEnd()
        {
            _simulationResultHandler.OnSimulationResult(new MoveSimulationResultData());
        }
    }

    public class MoveSimulationInputData
    {
        public int StartingTileIndex;
        public bool Direction;
        public int SideIndex;
    }

    public class MoveSimulationResultData
    {
    }

    public class MoveSimulationProgressData
    {
        public MoveType MoveType;
        public int TileIndex;
        public int NumCitizens;
        public int NumMandarins;
    }

    public enum MoveType
    {
        Grasp,
        Drop,
        Slam,
        Eat
    }

    public interface IBoardMoveSimulationResultHandler
    {
        void OnSimulationProgress(int simulationId, MoveSimulationProgressData result);
        void OnSimulationResult(MoveSimulationResultData result);
    }
}