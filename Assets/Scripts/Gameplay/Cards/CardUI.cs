using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Cards
{
    public class CardUI : MonoBehaviour
    {
        private CardSelector _selector;
        public event Action<Card> OnCardClicked;
        public event Action OnSelectedChanged;
        public event Action OnCardDataSet;
        [field: System.NonSerialized] public bool IsSelected { get; private set; }
        [field: System.NonSerialized] public Card CardData { get; private set; }

        public void Setup(CardSelector selector)
        {
            _selector = selector;
            _selector.CardSelected -= OnCardSelectorChanged;
            _selector.CardSelected += OnCardSelectorChanged;
        }

        public void TearDown()
        {
            _selector.CardSelected -= OnCardSelectorChanged;
        }

        private void OnCardSelectorChanged(object sender, EventArgs e)
        {
            IsSelected = _selector.SelectedCard == CardData;
            OnSelectedChanged?.Invoke();
        }

        public void SetCard(Card card)
        {
            CardData = card;
            OnCardDataSet?.Invoke();
        }

        public void OnClicked()
        {
            OnCardClicked?.Invoke(CardData);
            _selector.SelectCard(CardData);
        }
    }
}