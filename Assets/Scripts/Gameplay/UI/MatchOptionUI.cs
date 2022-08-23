using System;
using Common.ResolveSystem;
using UnityEngine;

namespace Gameplay.UI
{
    [Serializable]
    public class MatchOptionItem
    {
        [SerializeField] private int playerNum;
        [SerializeField] private int tilesPerGroup;

        public int PlayerNum => playerNum;

        public int TilesPerGroup => tilesPerGroup;
        public int Index { get; set; }

        public event Action<int> Selected;

        public virtual void OnSelect()
        {
            Selected?.Invoke(Index);
        }
    }

    public class MatchOptionUI : MonoBehaviour, IInjectable
    {
        [SerializeField] private MatchOptionItem[] optionItems;
        [SerializeField] private MatchOptionItemUI itemPrefab;
        private IMatchOption _matchOption;
        private GameManager.IGameEvents _gameEvents;

        private MatchOptionItemUI[] _itemUIs;

        public void Inject(IResolver resolver)
        {
            _matchOption = resolver.Resolve<IMatchOption>();
            _gameEvents = resolver.Resolve<GameManager.IGameEvents>();
        }

        private void Awake()
        {
            _itemUIs = new MatchOptionItemUI[optionItems.Length];
            for (var i = 0; i < optionItems.Length; i++)
            {
                optionItems[i].Index = i;
                _itemUIs[i] = Instantiate(itemPrefab, transform);
                _itemUIs[i].Setup(optionItems[i]);
                optionItems[i].Selected += OnSelected;
            }
            _gameEvents.Reset += OnGameReset;
        }

        private void OnDestroy()
        {
            for (var i = 0; i < _itemUIs.Length; i++)
            {
                _itemUIs[i].TearDown();
                optionItems[i].Selected -= OnSelected;
            }
            _gameEvents.Reset -= OnGameReset;
        }

        private void OnSelected(int index)
        {
            SetMatchOption(optionItems[index].PlayerNum, optionItems[index].TilesPerGroup);
            gameObject.SetActive(false);
        }

        private void OnGameReset()
        {
            gameObject.SetActive(true);
        }

        private void SetMatchOption(int playerNum, int tilesPerGroup)
            => (_matchOption as MatchOption)?.SetMatchOption(playerNum, tilesPerGroup);
    }
}