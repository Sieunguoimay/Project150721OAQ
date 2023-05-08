using UnityEngine;

namespace Gameplay.Cards
{
    public class Card : ScriptableObject
    {
        public CardType CardType;
        public string cardName;
        public Sprite icon;
        private void Awake()
        {
            Debug.Log($"{name} Awake");
        }
        private void OnEnable()
        {
            Debug.Log($"{name} OnEnable");
        }
        private void OnDisable()
        {
            Debug.Log($"{name} OnDisable");
        }
        private void OnDestroy()
        {
            Debug.Log($"{name} OnDestroy");
        }
    }

    public enum CardType
    {
        None,
        Concurrent,
        GoneWithTheWind,
        FutureForeseen
    }
}