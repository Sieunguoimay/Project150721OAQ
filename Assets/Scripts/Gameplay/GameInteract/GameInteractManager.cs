using System;
using Gameplay.Board;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public interface IGameInteractManager
    {
        void ShowUp();
        event Action<GameInteractResult> ResultEvent;
    }

    public class GameInteractResult
    {
        public GameInteractResult(ITile selectedTile, bool direction)
        {
            SelectedTile = selectedTile;
            Direction = direction;
        }

        public ITile SelectedTile { get; }
        public bool Direction { get; }
    }

    public class GameInteractManager : MonoControlUnitBase<GameInteractManager>, IGameInteractManager
    {
        [SerializeField] private TileChooser tileChooser;
        [SerializeField] private ActionChooser actionChooser;

        private BoardManager _boardManager;
        private PlayersManager _playersManager;

        protected override void OnSetup()
        {
            base.OnSetup();

            _boardManager = Resolver.Resolve<BoardManager>();
            _playersManager = Resolver.Resolve<PlayersManager>();

            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;
            tileChooser.SelectedTileChangedEvent += OnSelectedTileChanged;

            actionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            actionChooser.DirectionSelectedEvent += OnDirectionSelected;
        }

        protected override void OnTearDown()
        {
            base.OnTearDown();

            actionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;
        }

        public void SetupOnGameStart()
        {
            tileChooser.Setup(_boardManager.Board, _playersManager);
            actionChooser.Setup();
        }

        public void TearDownOnGameClear()
        {
            tileChooser.ResetAll();
            actionChooser.HideAway();
            actionChooser.TearDown();
            tileChooser.TearDown();
        }

        public void ShowUp()
        {
            tileChooser.ShowUp();
        }

        private void OnSelectedTileChanged()
        {
            actionChooser.ShowUp(tileChooser.SelectedTile.Transform, tileChooser.SelectedTile.Size);
        }

        private void OnDirectionSelected()
        {
            ResultEvent?.Invoke(new GameInteractResult(tileChooser.SelectedTile, actionChooser.SelectedDirection));
        }

        public event Action<GameInteractResult> ResultEvent;
    }
}