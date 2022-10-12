using System;
using System.Collections.Generic;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Board;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class TileChooser : MonoBehaviour
    {
        [SerializeField] private ButtonChooser buttonChooser;

        private Tile[] _tiles;
        private Tile[] _options;
        private Action<int> _onResult;

        public void ChooseTile(Tile[] tiles, Action<int> onResult)
        {
            _tiles = tiles;
            _onResult = onResult;
            var offsetFromTile = tiles[0].Size;
            var offset = Vector3.Cross((tiles[0].transform.position - tiles[1].transform.position).normalized,
                tiles[0].transform.up) * offsetFromTile;
            _options = tiles.Where(t => t.Pieces.Count > 0).ToArray();
            var buttons = GenerateButtonData(_options, offset);
            buttonChooser.Setup(buttons, OnTileChooserResult);
            buttonChooser.SetButtonsPositionAndRotation(buttons.Select(t => (t.Position, t.Rotation)).ToArray());
            buttonChooser.ShowButtons();
        }

        private void OnTileChooserResult(int id)
        {
            _onResult?.Invoke(Array.IndexOf(_tiles, _options[id]));
        }

        private static ButtonChooser.ButtonData[] GenerateButtonData(IReadOnlyList<Tile> tiles, Vector3 offset)
        {
            var buttons = new ButtonChooser.ButtonData[tiles.Count];
            for (var i = 0; i < tiles.Count; i++)
            {
                buttons[i] = new ButtonChooser.ButtonData
                {
                    Position = tiles[i].transform.position + offset,
                    Rotation = tiles[i].transform.rotation,
                    DisplayInfo = $"{tiles[i].Pieces.Count}",
                    ID = i
                };
            }

            return buttons;
        }
    }
}