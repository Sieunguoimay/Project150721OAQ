using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public class MatchOptionItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private string format = "{0} {1}x{2}";
        private MatchOptionItem _item;

        public void Setup(MatchOptionItem item)
        {
            _item = item;
            text.text = string.Format(format, item.Index, item.PlayerNum, item.TilesPerGroup);
        }

        public void TearDown()
        {
            _item = null;
        }

        public void OnClick()
        {
            _item.OnSelect();
        }
    }
}