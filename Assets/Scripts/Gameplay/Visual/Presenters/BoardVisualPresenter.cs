using System;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Interactors;
using UnityEngine;

namespace Gameplay.Visual.Presenters
{
    public class BoardVisualPresenter : ScriptableEntity, IRefreshResultHandler
    {
        [SerializeField] private BoardVisualGeneratorRepresenter boardVisualGeneratorRepresenter;
        public event Action<RefreshData> BoardStateChangedEvent;

        public void HandleRefreshData(RefreshData refreshData)
        {
            BoardStateChangedEvent?.Invoke(refreshData);
            boardVisualGeneratorRepresenter.Author.GenerateBoardVisual(refreshData);
        }
    }
}