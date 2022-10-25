using System;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public class MatchOptionItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private string format = "{0}x{1}";
        private IItemHandler _handler;

        private IMatchOption _item;

        public void Setup(IMatchOption item, IItemHandler handler)
        {
            _item = item;
            _handler = handler;
            text.text = string.Format(format, item.PlayerNum, item.TilesPerGroup);
        }

        public void TearDown()
        {
            _item = null;
        }

        public void OnClick()
        {
            _handler?.ItemSelected(_item);
        }

        public interface IItemHandler
        {
            void ItemSelected(IMatchOption item);
        }
    }
}