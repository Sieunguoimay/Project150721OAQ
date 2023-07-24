using System;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.Visual.Views;
using UnityEngine;

namespace Gameplay.Visual.Presenters
{
    public class BoardVisualPresenter : ScriptableEntity, IRefreshResultHandler
    {
        [SerializeField, ObjectBinderSO.Selector(typeof(BoardVisualGenerator))]
        private ObjectBinderSO boardVisualView;
        public event Action<RefreshData> BoardStateChangedEvent;

        public void HandleRefreshData(RefreshData refreshData)
        {
            BoardStateChangedEvent?.Invoke(refreshData);
            boardVisualView.GetRuntimeObject<BoardVisualGenerator>().GenerateBoardVisual(refreshData);
        }
    }
}