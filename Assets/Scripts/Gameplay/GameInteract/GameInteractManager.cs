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
        private Tile _chosenTile;
        private Tile[] _tiles;

        public override void Setup(IResolver resolver)
        {
            base.Setup(resolver);
            _boardManager = resolver.Resolve<BoardManager>();
        }

        public void PerformAction(Board.Board.TileGroup tileGroup, Action<(Tile, bool)> onResult)
        {
            _onResult = onResult;
            _tiles = _boardManager.SpawnedTiles.Where(st => tileGroup.Tiles.Contains(st)).ToArray();
            tileChooser.ChooseTile(_tiles, OnTileChooserResult);
        }

        private void OnTileChooserResult(Tile tile)
        {
            _chosenTile = tile;
            NotifyTilesAdapters(tile, true);
            ShowDirectionChooserForTile(tile);
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
            actionChooser.ShowUp(pos, tile.transform.rotation, OnActionChooserResult);
        }


        private void OnActionChooserResult(int result)
        {
            if (result == 2)
            {
                NotifyTilesAdapters(_chosenTile, false);

                tileChooser.ChooseTile(_tiles, OnTileChooserResult);
            }
            else
            {
                _onResult?.Invoke((_chosenTile, result == 1));
            }

            _chosenTile = null;
        }
    }
}