using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;

namespace Gameplay.CoreGameplay.Controllers
{
    public interface ICoreGameplayController
    {
        void Install();
        void Uninstall();
        void RunGameplay();
        void NotifyGameplayOnSimulationPresentationEnded();
        void RequestRefresh(IRefreshResultHandler resultPresenter);
        void MovePieceToNewTile(PieceInteractData.PieceMoveData moveData);
        void MovePiecesToPocket(PieceInteractData.PieceMoveToPocketData moveData);
        void RunSimulation(MoveSimulationInputData inputData);
    }

    public class CoreGameplayController : ICoreGameplayController
    {
        private readonly CoreGameplayContainer _container = new();

        private CoreGameplayInstaller _installer;

        public void SetupDependencies(CoreGameplayInstaller installer)
        {
            _installer = installer;
        }

        public void Install()
        {
            _installer.InstallEntities();
            _installer.InstallRefreshRequest(_container);
            _installer.InstallPiecesInteract(_container);
            _installer.InstallBoardMoveSimulation(_container);
            _installer.InstallTurnDataExtractor(_container);
            _installer.InstallGameplayDriver(_container);
        }

        public void Uninstall()
        {
            _installer.Uninstall(_container);
        }

        public void RunGameplay()
        {
            _container.GameplayDriver.MakeDecisionOfCurrentTurn();
        }

        public void NotifyGameplayOnSimulationPresentationEnded()
        {
            _container.GameplayDriver.OnSimulationPresentationEnded();
        }

        public void RequestRefresh(IRefreshResultHandler resultPresenter)
        {
            _container.RefreshRequester.Refresh(resultPresenter);
        }

        public void MovePieceToNewTile(PieceInteractData.PieceMoveData moveData)
        {
            _container.PiecesInteractor.MovePieceToNewTile(moveData);
        }

        public void MovePiecesToPocket(PieceInteractData.PieceMoveToPocketData moveData)
        {
            _container.PiecesInteractor.MovePiecesToPocket(moveData);
        }

        public void RunSimulation(MoveSimulationInputData inputData)
        {
            _container.BoardMoveSimulator.RunSimulation(inputData);
        }
    }


    public class CoreGameplayContainer
    {
        public IRefreshRequester RefreshRequester;
        public IPiecesInteractor PiecesInteractor;
        public IBoardMoveSimulator BoardMoveSimulator;
        public TurnDataExtractor TurnDataExtractor;
        public GameplayDriver GameplayDriver;
    }
}