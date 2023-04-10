using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Cards
{
    public class CardRepository : ScriptableObject
    {
        [SerializeField] private Card[] cards;

        public IEnumerable<Card> GetAll() => cards.ToList();
    }
}