using System;
using UnityEngine;

namespace Gameplay.Cards
{
    public class CardSelector : ScriptableObject
    {
        public event EventHandler CardSelected;

        [field: System.NonSerialized] public Card SelectedCard { get; private set; }

        public void SelectCard(Card card)
        {
            SelectedCard = card;
            CardSelected?.Invoke(this, EventArgs.Empty);
        }
    }
}