using System;
using System.Linq;
using Framework.Resolver;
using Gameplay.Board;
using Gameplay.GameInteract.Button;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class GameInteractManager : MonoControlUnitBase<GameInteractManager>
    {
        [SerializeField] private TileChooser tileChooser;
        [SerializeField] private ActionChooser actionChooser;

        private ICitizenTile[] _tiles;
        private TileChooser.ButtonCommand[] _choosingTileCommands;
        [field: System.NonSerialized] public ICitizenTile CurrentChosenTile { get; private set; }

        public void SetupInteract(BoardSide boardSide, MoveButtonCommand left, MoveButtonCommand right)
        {
            _tiles = boardSide.CitizenTiles.Where(s => s.HeldPieces.Count > 0).ToArray();
            
            _choosingTileCommands = new TileChooser.ButtonCommand[_tiles.Length];

            for (var i = 0; i < _tiles.Length; i++)
            {
                _choosingTileCommands[i] = new ButtonCommandChoosingTile().Setup(actionChooser, this, _tiles[i]);
            }

            CurrentChosenTile = null;
            actionChooser.SetMoveCommands(left.Setup(this, false), right.Setup(this, true));
            actionChooser.SetupOtherCommands();
        }

        public void ResetAll()
        {
            tileChooser.ResetAll();
            actionChooser.HideAway();
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

        public void ShowTileChooser()
        {
            tileChooser.ChooseTile(_tiles, _choosingTileCommands);
        }

        public void HideTileChooser()
        {
            tileChooser.ButtonContainer2.HideButtons();
        }

        private class ButtonCommandChoosingTile : TileChooser.ButtonCommand
        {
            private GameInteractManager _interact;
            private ActionChooser _actionChooser;
            private ICitizenTile _tile;

            public ButtonCommandChoosingTile Setup(ActionChooser actionChooser, GameInteractManager interact, ICitizenTile tile)
            {
                _actionChooser = actionChooser;
                _interact = interact;
                _tile = tile;
                return this;
            }

            public override void Execute(IButton button)
            {
                base.Execute(button);
                if (_interact.CurrentChosenTile != null)
                {
                    NotifyTileSelectionListeners(_interact.CurrentChosenTile, false);
                }
                
                NotifyTileSelectionListeners(_tile, true);

                var shouldHideOthers = _actionChooser.ButtonContainer.Buttons.Any(bv => bv.Active);
                if (shouldHideOthers)
                {
                    _actionChooser.ButtonContainer.HideButtons();
                    _actionChooser.Delay(.15f, ShowDirectionChooser);
                }
                else
                {
                    ShowDirectionChooser();
                }
            }

            private void ShowDirectionChooser()
            {
                _interact.ShowDirectionChooserForTile(_tile);
                _interact.CurrentChosenTile = _tile;
            }
        }

        public abstract class MoveButtonCommand : ButtonContainer.ButtonCommand
        {
            private bool _forward;
            private GameInteractManager _interact;

            public ButtonContainer.ButtonCommand Setup(GameInteractManager interact, bool forward)
            {
                _forward = forward;
                _interact = interact;
                return this;
            }

            public override void Execute(IButton button)
            {
                base.Execute(button);
                _interact.HideTileChooser();
                if (_interact.CurrentChosenTile != null)
                {
                    Move(_interact.CurrentChosenTile, _forward);
                }
            }

            protected abstract void Move(ITile tile, bool forward);
        }
    }
}