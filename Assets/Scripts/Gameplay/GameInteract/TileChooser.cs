using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Board;
using TMPro;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class TileChooser : MonoBehaviour
    {
        [SerializeField] private ButtonChooser buttonChooser;

        private Tile[] _options;
        private TextMeshPro[] _texts;

        public ButtonChooser ButtonChooser => buttonChooser;

        private void Start()
        {
            _texts = new TextMeshPro[buttonChooser.ButtonViews.Length];
            for (var i = 0; i < _texts.Length; i++)
            {
                _texts[i] = buttonChooser.ButtonViews[i].GetComponentInChildren<TextMeshPro>();
            }
        }

        public void ChooseTile(Tile[] tiles, ICommand[] commands)
        {
            _options = tiles;//tiles.Where(t => t.Pieces.Count > 0).ToArray();
            SetButtonsPositionAndRotation(_options);
            buttonChooser.Setup(_options.Length, commands);
            buttonChooser.ShowButtons();
        }

        private void SetButtonsPositionAndRotation(IReadOnlyList<Tile> optionTiles)
        {
            var offset = Vector3.Cross((optionTiles[0].transform.position - optionTiles[1].transform.position).normalized,
                optionTiles[0].transform.up) * optionTiles[0].Size;

            for (var i = 0; i < optionTiles.Count; i++)
            {
                buttonChooser.ButtonViews[i].SetPositionAndRotation(optionTiles[i].transform.position + offset, optionTiles[i].transform.rotation);
                _texts[i].text = $"{optionTiles[i].Pieces.Count}";
            }
        }
        //
        // private void OnTileChooserResult(int index)
        // {
        //     _onResult?.Invoke(_options[index]);
        // }
    }
}