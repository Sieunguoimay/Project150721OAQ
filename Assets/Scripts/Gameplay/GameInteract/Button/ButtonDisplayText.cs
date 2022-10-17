using TMPro;
using UnityEngine;

namespace Gameplay.GameInteract.Button
{
    public class ButtonDisplayText : AButtonDisplay
    {
        [SerializeField] private TextMeshPro tmp;

        public override void SetDisplayInfo(IButtonDisplayInfo displayInfo)
        {
            if (displayInfo is ButtonDisplayInfoText di)
            {
                tmp.text = di.Text;
            }
        }
    }

    public class ButtonDisplayInfoText : IButtonDisplayInfo
    {
        public ButtonDisplayInfoText(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}