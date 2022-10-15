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
        [SerializeField] private ButtonContainer buttonContainer;

        private TextMeshPro[] _texts;

        public ButtonContainer ButtonContainer => buttonContainer;

        private void Start()
        {
            _texts = new TextMeshPro[buttonContainer.ButtonViews.Length];
            for (var i = 0; i < _texts.Length; i++)
            {
                _texts[i] = buttonContainer.ButtonViews[i].GetComponentInChildren<TextMeshPro>();
            }
        }

        public void ChooseTile(Tile[] tiles, ICommand[] commands)
        {
            SetButtonsPositionAndRotation(tiles);
            buttonContainer.Setup(commands.Select(c => new ButtonData(c, new ButtonDisplaySpecialActionData()))
                .ToArray());
            buttonContainer.ShowButtons();
        }

        private void SetButtonsPositionAndRotation(IReadOnlyList<Tile> optionTiles)
        {
            var offset = Vector3.Cross(
                (optionTiles[0].transform.position - optionTiles[1].transform.position).normalized,
                optionTiles[0].transform.up) * optionTiles[0].Size;

            for (var i = 0; i < optionTiles.Count; i++)
            {
                buttonContainer.ButtonViews[i].transform.position = optionTiles[i].transform.position + offset;
                buttonContainer.ButtonViews[i].transform.rotation = optionTiles[i].transform.rotation;
                buttonContainer.ButtonViews[i].Display
                    .SetDisplayInfo(new ButtonDisplayInfoText($"{optionTiles[i].Pieces.Count}"));
            }
        }
    }
}