using System;
using TMPro;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class OnGroundButtonWithText : OnGroundButton
    {
        [SerializeField] private TextMeshPro text;

        public void ExtraSetup(ButtonChooser.ButtonData buttonData)
        {
            text.text = buttonData.DisplayInfo;
        }
    }
}