using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Cards
{
    public class CardContainer : ScriptableObject
    {
        [SerializeField] private Card[] cards;

        public IReadOnlyList<Card> Cards => cards;
    }
}