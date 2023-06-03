using System;
using UnityEngine;

namespace Gameplay.Cards
{
    public class Card : ScriptableObject
    {
        public CardType CardType;
        public string cardName;
        [SerializeField, AssetSelector.AssetType(typeof(Sprite))]
        private AssetSelector iconSelector;

        [field: System.NonSerialized] public bool IsSelected { get; private set; }
        public event Action<Card> OnSelectedChanged;

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            OnSelectedChanged?.Invoke(this);
        }
        public Sprite Icon => iconSelector.GetAsset<Sprite>();
    }

    public enum CardType
    {
        None,
        Concurrent,
        GoneWithTheWind,
        FutureForeseen
    }
}