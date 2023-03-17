using UnityEngine;

namespace Gameplay.Visual.GameInteract.Button
{
    public class ButtonDisplaySpecialAction : AButtonDisplay
    {
        public override void SetDisplayInfo(IButtonDisplayInfo displayInfo)
        {
            Debug.Log("Display: special action information");
        }
    }

    public class ButtonDisplayInfoSpecialAction : IButtonDisplayInfo
    {
    }
}