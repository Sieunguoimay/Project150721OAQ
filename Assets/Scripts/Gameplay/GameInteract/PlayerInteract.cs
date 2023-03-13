using System;
using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public interface IPlayerInteract
    {
        void Show();
        event Action<PlayerInteractResult> ResultEvent;
    }

    public class PlayerInteractResult
    {
        public PlayerInteractResult(TileVisual selectedTileVisual, bool direction)
        {
            selectedTileVisual = selectedTileVisual;
            Direction = direction;
        }

        public TileVisual selectedTileVisual { get; }
        public bool Direction { get; }
    }

    [Obsolete]
    public class PlayerInteract : BaseGenericDependencyInversionUnit<PlayerInteract>, IPlayerInteract
    {
        [SerializeField] private TileChooser tileChooser;
        [SerializeField] private ActionChooser actionChooser;

        private IGameplayContainer _container;
        // private IActionChooser ActionChooser => actionChooser;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _container = Resolver.Resolve<IGameplayContainer>();
        }

        public void Initialize()
        {
            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;
            tileChooser.SelectedTileChangedEvent += OnSelectedTileChanged;

            // ActionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            // ActionChooser.DirectionSelectedEvent += OnDirectionSelected;

            tileChooser.Setup(_container.PlayTurnTeller);
        }

        public void Cleanup()
        {
            // ActionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;

            tileChooser.ResetAll();
            actionChooser.HideAway();
            tileChooser.TearDown();
        }

        public void Show()
        {
            tileChooser.ShowUp();
        }

        private void OnSelectedTileChanged()
        {
            UpdateActionChooserPosition();
            actionChooser.ShowUp(OnDirectionSelected);
        }

        private void UpdateActionChooserPosition()
        {
            var tileTransform = tileChooser.selectedTileVisual.transform;
            var rot = tileTransform.rotation;
            var pos = tileTransform.position + rot * Vector3.forward;

            actionChooser.transform.SetPositionAndRotation(pos, rot);
        }

        private void OnDirectionSelected(bool direction)
        {
            var result = new PlayerInteractResult(tileChooser.selectedTileVisual, direction);
            ResultEvent?.Invoke(result);
        }

        public event Action<PlayerInteractResult> ResultEvent;
    }
}