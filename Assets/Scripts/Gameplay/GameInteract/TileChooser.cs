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

        private ButtonChooser.ButtonData[] _buttons;

        private Action<Tile> _onResult;

        public void ChooseTile(Tile[] tiles, Action<Tile> onResult)
        {
            _onResult = onResult;
            var offsetFromTile = tiles[0].Size;
            var offset = Vector3.Cross((tiles[0].transform.position - tiles[1].transform.position).normalized,
                tiles[0].transform.up) * offsetFromTile;
            _buttons = GenerateButtonData(tiles.Where(t => t.Pieces.Count > 0).ToList(), offset);
            buttonChooser.ShowButtons(_buttons, OnTileChooserResult);
        }

        private static ButtonChooser.ButtonData[] GenerateButtonData(IReadOnlyList<Tile> tiles, Vector3 offset)
        {
            var buttons = new ButtonChooser.ButtonData[tiles.Count];
            for (var i = 0; i < tiles.Count; i++)
            {
                buttons[i] = new ButtonChooser.ButtonData
                {
                    position = tiles[i].transform.position + offset,
                    rotation = tiles[i].transform.rotation,
                    displayInfo = tiles[i].Pieces.Count,
                    AttachedData = tiles[i]
                };
            }

            return buttons;
        }

        private void OnTileChooserResult(ButtonChooser.ButtonData buttonData)
        {
            _onResult?.Invoke(buttonData.AttachedData as Tile);
        }
    }
}