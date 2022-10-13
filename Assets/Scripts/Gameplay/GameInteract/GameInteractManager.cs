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
        [SerializeField] private DirectionChooser directionChooser;

        private BoardManager _boardManager;
        private Action<(Tile, bool)> _onResult;
        private Action<Tile> _onSelected;
        private Tile _chosenTile;
        private Tile[] _tiles;

        public override void Setup(IResolver resolver)
        {
            base.Setup(resolver);
            _boardManager = resolver.Resolve<BoardManager>();
        }

        public void PerformAction(Board.Board.TileGroup tileGroup, Action<Tile> onSelected,
            Action<(Tile, bool)> onResult)
        {
            _onResult = onResult;
            _onSelected = onSelected;
            _tiles = _boardManager.SpawnedTiles.Where(st => tileGroup.Tiles.Contains(st)).ToArray();
            tileChooser.ChooseTile(_tiles, OnTileChooserResult);
        }

        private void OnTileChooserResult(Tile tile)
        {
            _chosenTile = tile;
            _onSelected?.Invoke(tile);

            var selectionAdaptors = tile.GetSelectionAdaptors();
            foreach (var sa in selectionAdaptors)
            {
                sa.OnTileSelected();
            }

            var pos = tile.transform.position + tile.transform.rotation * Vector3.forward * tile.Size;
            directionChooser.ChooseDirection(pos, tile.transform.rotation, OnDirectionChooserResult);
        }

        private void OnDirectionChooserResult(int result)
        {
            if (result == 2)
            {
                var selectionAdaptors = _chosenTile.GetSelectionAdaptors();
                foreach (var sa in selectionAdaptors)
                {
                    sa.OnTileDeselected(false);
                }

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