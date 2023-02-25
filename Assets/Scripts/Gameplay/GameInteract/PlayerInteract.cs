using System;
using Gameplay.Board;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public interface IPlayerInteract
    {
        void Initialize();
        void Cleanup();
        void Show();
        event Action<PlayerInteractResult> ResultEvent;
    }

    public class PlayerInteractResult
    {
        public PlayerInteractResult(ITile selectedTile, bool direction)
        {
            SelectedTile = selectedTile;
            Direction = direction;
        }

        public ITile SelectedTile { get; }
        public bool Direction { get; }
    }

    public class PlayerInteract : BaseGenericDependencyInversionUnit<PlayerInteract>, IPlayerInteract
    {
        [SerializeField] private TileChooser tileChooser;
        [SerializeField] private ActionChooser actionChooser;

        private PlayerController _playerController;
        private IActionChooser ActionChooser => actionChooser;

        public void Initialize()
        {
            var boardManager = Resolver.Resolve<BoardManager>();
            _playerController = Resolver.Resolve<PlayerController>();

            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;
            tileChooser.SelectedTileChangedEvent += OnSelectedTileChanged;

            ActionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            ActionChooser.DirectionSelectedEvent += OnDirectionSelected;
            
            tileChooser.Setup(boardManager.Board, _playerController);
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
            var tileTransform = tileChooser.SelectedTile.Transform;
            var rot = tileTransform.rotation;
            var pos = tileTransform.position + rot * Vector3.forward * tileChooser.SelectedTile.Size;

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