﻿using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors
{
    public class CoreGameplayInstaller
    {
        private BoardEntity _board;
        private readonly CoreGameplayContainer _container;
        private PiecesInteractor.InnerPiecesInteractor _innerPiecesInteractor;

        public CoreGameplayInstaller(CoreGameplayContainer container)
        {
            _container = container;
        }

        public void InstallEntities(IEntitiesDataAccess dataAccess)
        {
            var boardData = dataAccess.GetBoardData();
            _board = CoreEntitiesFactory.CreateBoardEntity(boardData);
            _innerPiecesInteractor = new PiecesInteractor.InnerPiecesInteractor(_board);

        }

        public void InstallRefreshRequest(IRefreshResultHandler resultHandler)
        {
            var refreshRequester = new RefreshRequester(_board, resultHandler);
            _container.RefreshRequester = refreshRequester;
        }

        public void InstallPiecesInteract(IPieceInteractResultHandler interactResultHandler)
        {
            _container.PiecesInteractor = new PiecesInteractor(_innerPiecesInteractor,interactResultHandler);
        }

        public void InstallBoardMoveSimulation(IBoardMoveSimulationResultHandler simulationResultHandler)
        {
            var simulator = new BoardMoveSimulator(_board, simulationResultHandler, _innerPiecesInteractor);
            _container.BoardMoveSimulator = simulator;
        }
    }
}