using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Gameplay.Board;
using Gameplay.GameInteract.Button;
using TMPro;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class TileChooser : MonoBehaviour
    {
        [SerializeField, TypeConstraint(typeof(IButtonContainer))]
        private UnityEngine.Object buttonContainer;

        [SerializeField, TypeConstraint(typeof(IButtonContainer))]
        private UnityEngine.Object buttonContainer2;

        public IButtonContainer ButtonContainer => buttonContainer as IButtonContainer;
        public IButtonContainer ButtonContainer2 => buttonContainer2 as IButtonContainer;

        public void ChooseTile(IReadOnlyList<ICitizenTile> tiles, IEnumerable<ButtonCommand> commands)
        {
            var bd = commands.Select(c => new ButtonData(c.SetButtonContainer2(ButtonContainer2).SetContainer(ButtonContainer),
                new ButtonDisplayInfoSpecialAction())).ToArray();
            foreach (var buttonCommand in commands)
            {
                
            }

            SetButtonsPositionAndRotation(tiles);

            ButtonContainer.Setup(bd);
            ButtonContainer.ShowButtons();
            ButtonContainer2.Setup(bd);
            ButtonContainer2.ShowButtons();
        }

        public void ResetAll()
        {
            ButtonContainer.HideButtons();
            ButtonContainer2.HideButtons();
        }
        
        private void SetButtonsPositionAndRotation(IReadOnlyList<ICitizenTile> optionTiles)
        {
            for (var i = 0; i < optionTiles.Count; i++)
            {
                var tilePos = optionTiles[i].Transform.position;
                var tileRot = optionTiles[i].Transform.rotation;
                var pos = tilePos + optionTiles[i].Transform.forward * optionTiles[i].Size;

                ButtonContainer.Buttons[i].transform.position = pos;
                ButtonContainer.Buttons[i].transform.rotation = tileRot;
                ButtonContainer.Buttons[i].Display
                    .SetDisplayInfo(new ButtonDisplayInfoText($"{optionTiles[i].HeldPieces.Count}"));

                ButtonContainer2.Buttons[i].transform.position = tilePos;
                ButtonContainer2.Buttons[i].transform.rotation = tileRot;
            }
        }
 
        public class ButtonCommand : ButtonContainer.ButtonCommand
        {
            private IButtonContainer _buttonContainer2;

            public ButtonCommand SetButtonContainer2(IButtonContainer buttonContainer)
            {
                _buttonContainer2 = buttonContainer;
                return this;
            }

            public override void Execute(IButton button)
            {
                base.Execute(button);

                foreach (var bv in _buttonContainer2.Buttons)
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