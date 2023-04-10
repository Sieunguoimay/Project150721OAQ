using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Cards
{
    public class CardUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        private Card _card;
        public event Action<Card> OnCardSelected; 

        public void SetCard(Card card)
        {
            _card = card;
            iconImage.sprite = _card.icon;
        }

        public void OnClicked()
        {
            OnCardSelected?.Invoke(_card);
        }
    }
}