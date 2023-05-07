using UnityEngine;

namespace Gameplay.Cards
{
    public class Card : ScriptableObject
    {
        public CardType CardType;
        public string cardName;
        public Sprite icon;
    }

    public enum CardType
    {
        None,
        Concurrent,
        GoneWithTheWind,
        FutureForeseen
    }
}