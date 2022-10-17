using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Board;
using Gameplay.GameInteract.Button;
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
            buttonContainer.Setup(commands.Select(c => new ButtonData(c, new ButtonDisplayInfoSpecialAction()))
                .ToArray());
            buttonContainer.ShowButtons();
        }

        private void SetButtonsPositionAndRotation(IReadOnlyList<Tile> optionTiles)
        {
            for (var i = 0; i < optionTiles.Count; i++)
            {
                buttonContainer.ButtonViews[i].transform.position = optionTiles[i].transform.position + optionTiles[i].transform.forward*optionTiles[i].Size;
                buttonContainer.ButtonViews[i].transform.rotation = optionTiles[i].transform.rotation;
                buttonContainer.ButtonViews[i].Display
                    .SetDisplayInfo(new ButtonDisplayInfoText($"{optionTiles[i].Pieces.Count}"));
            }
        }
    }
}