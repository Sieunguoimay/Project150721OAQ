using System;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Board;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class GameInteractManager : InjectableBehaviour<GameInteractManager>
    {
        [SerializeField] private TileChooser tileChooser;
        [SerializeField] private ActionChooser actionChooser;

        private BoardManager _boardManager;
        private Action<(Tile, bool)> _onResult;
        [field: System.NonSerialized] public Tile ChosenTile { get; set; }
        private Tile[] _tiles;
        private TileChooser.ButtonCommand[] _choosingTileCommands;

        public override void Setup(IResolver resolver)
        {
            base.Setup(resolver);
            _boardManager = resolver.Resolve<BoardManager>();
        }

        public void PerformAction(Board.Board.TileGroup tileGroup, Action<(Tile, bool)> onResult)
        {
            _onResult = onResult;

            _tiles = _boardManager.SpawnedTiles.Where(st => tileGroup.Tiles.Contains(st) && st.Pieces.Count > 0)
                .ToArray();

            _choosingTileCommands = new TileChooser.ButtonCommand[_tiles.Length];

            for (var i = 0; i < _tiles.Length; i++)
            {
                _choosingTileCommands[i] = new ButtonCommandChoosingTile(tileChooser, actionChooser, this, _tiles[i]);
            }

            ChosenTile = null;

            ShowTileChooser();
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

            actionChooser.ShowUp(this);
        }

        public void ShowTileChooser()
        {
            tileChooser.ChooseTile(_tiles, _choosingTileCommands);
        }

        public void HideTileChooser()
        {
            tileChooser.ButtonContainer2.HideButtons();
        }

        public void MoveLeftRight(bool right)
        {
            if (ChosenTile != null)
            {
                _onResult?.Invoke((ChosenTile, right));
            }
        }
    }


    public class ButtonCommandChoosingTile : TileChooser.ButtonCommand
    {
        private readonly GameInteractManager _interact;
        private readonly ActionChooser _actionChooser;
        private readonly Tile _tile;

        public ButtonCommandChoosingTile(TileChooser tileChooser, ActionChooser actionChooser,
            GameInteractManager interact, Tile tile) : base(
            tileChooser, tileChooser.ButtonContainer)
        {
            _actionChooser = actionChooser;
            _interact = interact;
            _tile = tile;
        }

        public override void Execute()
        {
            base.Execute();
            if (_interact.ChosenTile != null)
            {
                GameInteractManager.NotifyTilesAdapters(_interact.ChosenTile, false);
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
            GameInteractManager.NotifyTilesAdapters(_tile, true);
            _interact.ShowDirectionChooserForTile(_tile);
            _interact.ChosenTile = _tile;
        }
    }
}