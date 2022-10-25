using System;
using UnityEngine;

namespace Gameplay.UI
{
    [CreateAssetMenu]
    public class MatchConfig : ScriptableObject
    {
        public MatchOption[] optionItems;
    }

    [Serializable]
    public class MatchOption : IMatchOption
    {
        [SerializeField] private int playerNum;
        [SerializeField] private int tilesPerGroup;
        public int PlayerNum => playerNum;
        public int TilesPerGroup => tilesPerGroup;
    }
}