using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Cards
{
    public class CardListUI : MonoBehaviour
    {
        [SerializeField] private CardContainer cardContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardListParent;

        private readonly List<CardUI> _cardUis = new();
        private void Start()
        {
            PopulateCardList();
        }

        private void OnDestroy()
        {
            TearDownCardList();
        }

        public void PopulateCardList()
        {
            var cards = cardContainer.Cards;
            foreach (var card in cards)
            {
                var cardObject = Instantiate(cardPrefab, cardListParent);
                var cardUI = cardObject.GetComponent<CardUI>();
                cardUI.SetCard(card);
                _cardUis.Add(cardUI);
            }
        }

        public void TearDownCardList()
        {
            foreach (var cardUi in _cardUis)
            {
                Destroy(cardUi.gameObject);
            }
        }

        public void SetSelectable(bool selectable)
        {
            cardListParent.gameObject.SetActive(selectable);
            foreach(var card in cardContainer.Cards)
            {
                card.SetSelected(false);
            }
        }
    }
}