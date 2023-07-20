using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gameplay.Cards
{
    public class Card : ScriptableObject
    {
        public CardType CardType;
        public string cardName;
        public Sprite icon;

        [field: System.NonSerialized] public bool IsSelected { get; private set; }
        public event Action<Card> OnSelectedChanged;

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            OnSelectedChanged?.Invoke(this);
        }
        public Sprite Icon => icon;
    }

    public enum CardType
    {
        None,
        Concurrent,
        GoneWithTheWind,
        FutureForeseen
    }
}