using System;
using System.Linq;
using Framework.Resolver;
using Gameplay.Board;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class GameInteractManager : MonoControlUnitBase<GameInteractManager>
    {
        [SerializeField] private TileChooser tileChooser;
        [SerializeField] private ActionChooser actionChooser;

        private Tile[] _tiles;
        private BoardManager _boardManager;
        private TileChooser.ButtonCommand[] _choosingTileCommands;
        [field: System.NonSerialized] public Tile ChosenTile { get; private set; }

        protected override void OnInject(IResolver resolver)
        {
            _boardManager = resolver.Resolve<BoardManager>();
        }

        public void SetupInteract(Board.Board.BoardSide boardSide, MoveButtonCommand left, MoveButtonCommand right)
        {
            _tiles = _boardManager.Board.Tiles.Where(st => boardSide.CitizenTiles.Contains(st) && st.PiecesContainer.Count > 0)
                .ToArray();

            _choosingTileCommands = new TileChooser.ButtonCommand[_tiles.Length];

            for (var i = 0; i < _tiles.Length; i++)
            {
                _choosingTileCommands[i] = new ButtonCommandChoosingTile().Setup(actionChooser, this, _tiles[i]);
            }

            ChosenTile = null;
            actionChooser.SetMoveCommands(left.Setup(this, false), right.Setup(this, true));
            actionChooser.SetupOtherCommands();
        }

        public static void NotifyTilesAdapters(Tile tile, bool selected)
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

        public void ShowDirectionChooserForTile(Tile tile)
        {
            var tileTransform = tile.transform;
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
            private Tile _tile;

            public ButtonCommandChoosingTile Setup(ActionChooser actionChooser, GameInteractManager interact, Tile tile)
            {
                _actionChooser = actionChooser;
                _interact = interact;
                _tile = tile;
                return this;
            }

            public override void Execute()
            {
                base.Execute();
                if (_interact.ChosenTile != null)
                {
                    NotifyTilesAdapters(_interact.ChosenTile, false);
                }

                var shouldHide = _actionChooser.ButtonContainer.ButtonViews.Any(bv => bv.Active);
                if (shouldHide)
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
                NotifyTilesAdapters(_tile, true);
                _interact.ShowDirectionChooserForTile(_tile);
                _interact.ChosenTile = _tile;
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

            public override void Execute()
            {
                base.Execute();
                _interact.HideTileChooser();
                if (_interact.ChosenTile)
                {
                    Move(_interact.ChosenTile, _forward);
                }
            }

            protected abstract void Move(Tile tile, bool forward);
        }
    }
}