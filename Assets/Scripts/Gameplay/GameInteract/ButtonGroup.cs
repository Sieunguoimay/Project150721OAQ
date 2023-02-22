using System.Collections.Generic;
using Gameplay.GameInteract.Button;

namespace Gameplay.GameInteract
{
    public class ButtonGroup
    {
        public IReadOnlyList<IButton> Buttons { get; }

        public ButtonGroup(IReadOnlyList<IButton> buttons)
        {
            Buttons = buttons;
        }

        public void ShowButtons()
        {
            foreach (var t in Buttons)
            {
                t.ShowUp();
            }
        }

        public void HideButtons()
        {
            foreach (var t in Buttons)
            {
                t.HideAway();
            }
        }
    }
}