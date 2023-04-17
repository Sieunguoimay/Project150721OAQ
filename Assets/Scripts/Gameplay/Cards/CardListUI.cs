using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Cards
{
    public class CardListUI : MonoBehaviour
    {
        [SerializeField] private CardRepository cardRepository;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardListParent;
        [SerializeField] private CardSelector cardSelector;

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
            var cards = cardRepository.GetAll();
            foreach (var card in cards)
            {
                var cardObject = Instantiate(cardPrefab, cardListParent);
                var cardUI = cardObject.GetComponent<CardUI>();
                cardUI.SetCard(card);
                cardUI.Setup(cardSelector);
                _cardUis.Add(cardUI);
            }
        }

        public void TearDownCardList()
        {
            foreach (var cardUi in _cardUis)
            {
                cardUi.TearDown();
                Destroy(cardUi.gameObject);
            }
        }
    }
}