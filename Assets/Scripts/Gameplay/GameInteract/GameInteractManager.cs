﻿using System;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Board;
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
        private ICommand[] _choosingTileCommands;

        public override void Setup(IResolver resolver)
        {
            base.Setup(resolver);
            _boardManager = resolver.Resolve<BoardManager>();
        }

        public void PerformAction(Board.Board.TileGroup tileGroup, Action<(Tile, bool)> onResult)
        {
            _onResult = onResult;
            
            _tiles = _boardManager.SpawnedTiles.Where(st => tileGroup.Tiles.Contains(st)&&st.Pieces.Count > 0).ToArray();

            _choosingTileCommands = new ICommand[_tiles.Length];
            
            for (var i = 0; i < _tiles.Length; i++)
            {
                _choosingTileCommands[i] = new ChoosingTileCommand(tileChooser.ButtonContainer, this, _tiles[i]);
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
            tileChooser.ChooseTile(_tiles, _choosingTileCommands);
        }

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

    public class ChoosingTileCommand : ButtonContainer.ButtonCommand
    {
        private readonly GameInteractManager _interact;
        private readonly Tile _tile;

        public ChoosingTileCommand(ButtonContainer buttonContainer, GameInteractManager interact, Tile tile) : base(buttonContainer)
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