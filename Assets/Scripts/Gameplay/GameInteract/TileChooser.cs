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
        [SerializeField] private ButtonContainer buttonContainer2;

        public ButtonContainer ButtonContainer => buttonContainer;
        public ButtonContainer ButtonContainer2 => buttonContainer2;

        public void ChooseTile(Tile[] tiles, ButtonCommand[] commands)
        {
            var bd = commands.Select(c => new ButtonData(c, new ButtonDisplayInfoSpecialAction())).ToArray();

            SetButtonsPositionAndRotation(tiles);

            buttonContainer.Setup(bd);
            buttonContainer.ShowButtons();
            buttonContainer2.Setup(bd);
            buttonContainer2.ShowButtons();
        }

        private void SetButtonsPositionAndRotation(IReadOnlyList<Tile> optionTiles)
        {
            for (var i = 0; i < optionTiles.Count; i++)
            {
                var tilePos = optionTiles[i].transform.position;
                var tileRot = optionTiles[i].transform.rotation;
                var pos = tilePos + optionTiles[i].transform.forward * optionTiles[i].Size;

                buttonContainer.ButtonViews[i].transform.position = pos;
                buttonContainer.ButtonViews[i].transform.rotation = tileRot;
                buttonContainer.ButtonViews[i].Display
                    .SetDisplayInfo(new ButtonDisplayInfoText($"{optionTiles[i].Pieces.Count}"));

                buttonContainer2.ButtonViews[i].transform.position = tilePos;
                buttonContainer2.ButtonViews[i].transform.rotation = tileRot;
            }
        }

        public class ButtonCommand : ButtonContainer.ButtonCommand
        {
            private readonly TileChooser _tileChooser;

            protected ButtonCommand(TileChooser tileChooser, ButtonContainer container) : base(
                container)
            {
                _tileChooser = tileChooser;
            }

            public override void Execute()
            {
                base.Execute();

                foreach (var bv in _tileChooser.ButtonContainer2.ButtonViews)
                {
                    if (bv.Command == this)
                    {
                        bv.HideAway();
                    }
                    else
                    {
                        if (!bv.Active && bv.Command != null)
                        {
                            bv.ShowUp();
                        }
                    }
                }
            }
        }
    }
}