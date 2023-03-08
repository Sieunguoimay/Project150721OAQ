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
        public PlayerInteractResult(Tile selectedTile, bool direction)
        {
            SelectedTile = selectedTile;
            Direction = direction;
        }

        public Tile SelectedTile { get; }
        public bool Direction { get; }
    }

    public class PlayerInteract : BaseGenericDependencyInversionUnit<PlayerInteract>, IPlayerInteract
    {
        [SerializeField] private TileChooser tileChooser;
        [SerializeField] private ActionChooser actionChooser;
        private IGameplayContainer _container;
        private IActionChooser ActionChooser => actionChooser;
        
        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _container = Resolver.Resolve<IGameplayContainer>();
        }

        public void Initialize()
        {
            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;
            tileChooser.SelectedTileChangedEvent += OnSelectedTileChanged;

            ActionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            ActionChooser.DirectionSelectedEvent += OnDirectionSelected;

            tileChooser.Setup(_container.PlayTurnTeller);
        }

        public void Cleanup()
        {
            ActionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;

            tileChooser.ResetAll();
            ActionChooser.HideAway();
            tileChooser.TearDown();
        }

        public void Show()
        {
            tileChooser.ShowUp();
        }

        private void OnSelectedTileChanged()
        {
            UpdateActionChooserPosition();
            ActionChooser.ShowUp();
        }

        private void UpdateActionChooserPosition()
        {
            var tileTransform = tileChooser.SelectedTile.transform;
            var rot = tileTransform.rotation;
            var pos = tileTransform.position + rot * Vector3.forward;

            ActionChooser.SetPositionAndRotation(pos, rot);
        }

        private void OnDirectionSelected()
        {
            var result = new PlayerInteractResult(tileChooser.SelectedTile, actionChooser.SelectedDirection);
            ResultEvent?.Invoke(result);
        }

        public event Action<PlayerInteractResult> ResultEvent;
    }
}