using System;
using Gameplay.Board;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class GameInteractManager : MonoControlUnitBase<GameInteractManager>
    {
        [SerializeField] private TileChooser tileChooser;
        [SerializeField] private ActionChooser actionChooser;

        private bool _waitingForAlLTileChoosingButtonHiddenAway;
        private GameplayControlUnit _gameplayControl;

        public event Action<ITile, bool> MoveEvent;

        protected override void OnSetup()
        {
            base.OnSetup();
            
            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;
            tileChooser.SelectedTileChangedEvent += OnSelectedTileChanged;
            tileChooser.ButtonContainer.AllButtonHiddenEvent -= OnAllTileChoosingButtonsHidden;
            tileChooser.ButtonContainer.AllButtonHiddenEvent += OnAllTileChoosingButtonsHidden;

            actionChooser.Setup();
            actionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            actionChooser.DirectionSelectedEvent += OnDirectionSelected;
            _gameplayControl = Resolver.Resolve<GameplayControlUnit>();
            
            _gameplayControl.GameplayBeginEvent -= OnGameBegin;
            _gameplayControl.GameplayBeginEvent += OnGameBegin;
            _gameplayControl.GameplayEndEvent -= OnGameEnd;
            _gameplayControl.GameplayEndEvent += OnGameEnd;
        }


        protected override void OnTearDown()
        {
            base.OnTearDown();
            _gameplayControl.GameplayBeginEvent -= OnGameBegin;
            _gameplayControl.GameplayEndEvent -= OnGameEnd;
            
            actionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            tileChooser.ButtonContainer.AllButtonHiddenEvent -= OnAllTileChoosingButtonsHidden;
            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;
            actionChooser.TearDown();
        }

        private void OnGameBegin()
        {
            tileChooser.Setup(Resolver.Resolve<BoardManager>().Board.Metadata.NumCitizenTilesPerSide);
        }

        private void OnGameEnd()
        {
            tileChooser.TearDown();
        }
        
        private void OnDirectionSelected(ActionChooser arg1, ActionChooser.DirectionSelectArgs arg2)
        {
            MoveEvent?.Invoke(tileChooser.SelectedTile, arg2.Direction);
        }

        public void ResetAll()
        {
            tileChooser.ResetAll();
            actionChooser.HideAway();
        }

        private void OnSelectedTileChanged(TileChooser obj, TileChooser.SelectedTileEventArgs selectedTileEventArgs)
        {
            _waitingForAlLTileChoosingButtonHiddenAway = true;

            if (selectedTileEventArgs.PrevSelectedTile != null)
            {
                NotifyTileSelectionListeners(selectedTileEventArgs.PrevSelectedTile as ICitizenTile, false);
            }

            if (tileChooser.SelectedTile != null)
            {
                NotifyTileSelectionListeners(tileChooser.SelectedTile as ICitizenTile, true);
            }
        }

        private void OnAllTileChoosingButtonsHidden(IButtonContainer obj)
        {
            if (_waitingForAlLTileChoosingButtonHiddenAway && tileChooser.SelectedTile != null)
            {
                _waitingForAlLTileChoosingButtonHiddenAway = false;
                ShowDirectionChooserForTile(tileChooser.SelectedTile);
            }
        }

        public static void NotifyTileSelectionListeners(ICitizenTile tile, bool selected)
        {
            var selectionAdaptors = tile.GetSelectionAdaptors();
            foreach (var sa in selectionAdaptors)
            {
                if (selected)
                    sa.OnTileSelected();
                else
                    sa.OnTileDeselected(false);
            }
        }

        public void ShowDirectionChooserForTile(ITile tile)
        {
            var tileTransform = tile.Transform;
            var tileRotation = tileTransform.rotation;
            var pos = tileTransform.position + tileRotation * Vector3.forward * tile.Size;
            var t = actionChooser.transform;
            t.position = pos;
            t.rotation = tileRotation;

            actionChooser.ShowUp();
        }

        public void ShowTileChooser(ICitizenTile[] tiles)
        {
            tileChooser.ChooseTile(tiles);
        }
    }
}