using System;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Board;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class GameInteractManager : InjectableBehaviour<GameInteractManager>
    {
        [SerializeField] private TileChooser tileChooser;

        private BoardManager _boardManager;
        private Action<(Tile, bool)> _onResult;

        public override void Setup(IResolver resolver)
        {
            base.Setup(resolver);
            _boardManager = resolver.Resolve<BoardManager>();
        }

        public void PerformAction(Board.Board.TileGroup tileGroup, Action<(Tile, bool)> onResult)
        {
            _onResult = onResult;
            var tiles = _boardManager.SpawnedTiles.Where(st => tileGroup.Tiles.Contains(st)).ToArray();
            tileChooser.ChooseTile(tiles, OnTileChooserResult);
        }

        private void OnTileChooserResult(Tile tile)
        {
            _onResult?.Invoke((tile, true));
        }
    }
}