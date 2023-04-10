using System;
using UnityEngine;

namespace Gameplay.Cards
{
    public class CardSelector : ScriptableObject
    {
        private Card _selectedCard;
        public event EventHandler CardSelected;

        public void SelectCard(Card card)
        {
            _selectedCard = card;
            CardSelected?.Invoke(this, EventArgs.Empty);
        }
    }
}