using System;
using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;
using UnityEngine;

namespace Gameplay.CoreGameplay.Presenters
{
    public interface IInteractResultPresenter : IRefreshResultHandler, IPieceInteractResultHandler,
        IBoardMoveSimulationResultHandler
    {
    }
    public class SimpleInteractResultPresenter : IInteractResultPresenter
    {
        private readonly CoreGameplayContainer _container;
        public event Action<SimpleInteractResultPresenter> RefreshDataAvailableEvent;
        public RefreshData RefreshData { get; private set; }

        public SimpleInteractResultPresenter(CoreGameplayContainer container)
        {
            _container = container;
        }

        public void HandleRefreshData(RefreshData refreshData)
        {
            RefreshData = refreshData;
            RefreshDataAvailableEvent?.Invoke(this);
        }

        public void OnMovePieceToNewTileDone(PieceInteractResultData resultData)
        {
            if (resultData.Success)
            {
                Debug.Log("Move Succeeded");
                _container.RefreshRequester.Refresh();
            }
            else
            {
                Debug.LogError("Move failed");
            }
        }

        public void OnMoveAllPiecesToPocketDone(PieceInteractResultData resultData)
        {
            if (resultData.Success)
            {
                Debug.Log("Eat Succeeded");
                _container.RefreshRequester.Refresh();
            }
            else
            {
                Debug.LogError("Eat failed");
            }
        }

        public void OnSimulationResult(MoveSimulationOutputData result)
        {
            Debug.Log("Simulation Done");
            _container.RefreshRequester.Refresh();
        }

        public void OnSimulationProgress(MoveSimulationOutputData result)
        {
            Debug.Log("Simulation Progress");
            _container.RefreshRequester.Refresh();
        }
    }
}