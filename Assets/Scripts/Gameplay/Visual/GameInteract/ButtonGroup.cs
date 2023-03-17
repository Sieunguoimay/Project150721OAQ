using System.Collections.Generic;
using Gameplay.Visual.GameInteract.Button;

namespace Gameplay.Visual.GameInteract
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