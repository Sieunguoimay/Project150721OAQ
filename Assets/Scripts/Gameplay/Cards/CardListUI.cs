using UnityEngine;

namespace Gameplay.Cards
{
    public class CardListUI : MonoBehaviour
    {
        [SerializeField] private CardRepository cardRepository;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardListParent;
        [SerializeField] private CardSelector cardSelector;

        private void Start()
        {
            PopulateCardList();
        }

        public void PopulateCardList()
        {
            var cards = cardRepository.GetAll();
            foreach (var card in cards)
            {
                var cardObject = Instantiate(cardPrefab, cardListParent);
                var cardUI = cardObject.GetComponent<CardUI>();
                cardUI.SetCard(card);
                cardUI.OnCardSelected += OnCardSelected;
            }
        }

        private void OnCardSelected(Card card)
        {
            Debug.Log(card.cardName);
            cardSelector.SelectCard(card);
        }
    }
}