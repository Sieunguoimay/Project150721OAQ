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
        private ICommand[] _choosingTileCommand;

        public override void Setup(IResolver resolver)
        {
            base.Setup(resolver);
            _boardManager = resolver.Resolve<BoardManager>();
        }

        public void PerformAction(Board.Board.TileGroup tileGroup, Action<(Tile, bool)> onResult)
        {

            _onResult = onResult;
            _tiles = _boardManager.SpawnedTiles.Where(st => tileGroup.Tiles.Contains(st)).ToArray();
            var options = _tiles.Where(t => t.Pieces.Count > 0).ToArray();

            _choosingTileCommand = new ICommand[options.Length];
            for (var i = 0; i < options.Length; i++)
            {
                _choosingTileCommand[i] = new ChoosingTileCommand(tileChooser.ButtonChooser, this, options[i]);
            }
            
            ChosenTile = null;

            ShowTileChooser();
        }

        public void NotifyTilesAdapters(Tile tile, bool selected)
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
            var pos = tile.transform.position + tile.transform.rotation * Vector3.forward * tile.Size;
            actionChooser.ShowUp(pos, tile.transform.rotation, this);
        }

        public void ShowTileChooser()
        {
            tileChooser.ChooseTile(_tiles, _choosingTileCommand);
        }

        // private void OnActionChooserResult(int result)
        // {
        //     if (result == 2)
        //     {
        //         NotifyTilesAdapters(ChosenTile, false);
        //
        //         ShowTileChooser();
        //     }
        //     else
        //     {
        //     }
        //
        // }

        public void MoveLeftRight(bool right)
        {
            if (ChosenTile != null)
            {
                _onResult?.Invoke((ChosenTile, right));
            }
        }
    }

    public interface ICommand
    {
        void Execute();
    }

    public class ChoosingTileCommand : ButtonChooser.ButtonCommand
    {
        private readonly GameInteractManager _interact;
        private readonly Tile _tile;

        public ChoosingTileCommand(ButtonChooser buttonChooser, GameInteractManager interact, Tile tile) : base(buttonChooser)
        {
            _interact = interact;
            _tile = tile;
        }

        public override void Execute()
        {
            base.Execute();
            _interact.NotifyTilesAdapters(_tile, true);
            _interact.ShowDirectionChooserForTile(_tile);
            _interact.ChosenTile = _tile;
        }
    }
}