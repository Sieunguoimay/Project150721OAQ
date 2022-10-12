using System;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ButtonChooserWithText : ButtonChooser
    {
        public override void Setup(ButtonData[] buttons, Action<int> onResult)
        {
            Buttons = buttons;
            Result = onResult;
            var n = Mathf.Min(buttonViews.Length, Buttons.Length);
            for (var i = 0; i < n; i++)
            {
                (buttonViews[i] as OnGroundButtonWithText)?.SetupCallback(Buttons[i].ID, OnButtonClick);
                (buttonViews[i] as OnGroundButtonWithText)?.ExtraSetup(Buttons[i]);
            }
        }
    }
}