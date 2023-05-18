using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Cards
{
    public class CardUI : MonoBehaviour
    {
        public event Action OnCardDataSet;
        [field: System.NonSerialized] public Card CardData { get; private set; }

        public void SetCard(Card card)
        {
            CardData = card;
            OnCardDataSet?.Invoke();
        }

        public void OnClicked()
        {
            CardData.SetSelected(true);
        }
    }
}