﻿using System;
using Common.ResolveSystem;
using UnityEngine;
using UnityEngine.Events;

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

    public class MatchChooserUI : MonoBehaviour, IInjectable
    {
        [SerializeField] private MatchOptionItem[] optionItems;
        [SerializeField] private MatchOptionItemUI itemPrefab;
        [SerializeField] private UnityEvent onSelected;
        [SerializeField] private UnityEvent onReset;
        private IMatchChooser _matchChooser;

        private MatchOptionItemUI[] _itemUIs;

        public void Bind(IResolver resolver)
        {

        }

        public void Setup(IResolver resolver)
        {
            _matchChooser = resolver.Resolve<IMatchChooser>();
        }

        public void TearDown()
        {
        }

        public void Unbind(IResolver resolver)
        {
        }

        private void Start()
        {
            _itemUIs = new MatchOptionItemUI[optionItems.Length];
            for (var i = 0; i < optionItems.Length; i++)
            {
                optionItems[i].Index = i;
                _itemUIs[i] = Instantiate(itemPrefab, transform);
                _itemUIs[i].Setup(optionItems[i]);
                optionItems[i].Selected += OnSelected;
            }
        }

        private void OnDestroy()
        {
            for (var i = 0; i < _itemUIs.Length; i++)
            {
                _itemUIs[i].TearDown();
                optionItems[i].Selected -= OnSelected;
            }
        }

        private void OnSelected(int index)
        {
            SetMatchOption(optionItems[index].PlayerNum, optionItems[index].TilesPerGroup);
            onSelected?.Invoke();
        }

        private void OnGameReset()
        {
            onReset?.Invoke();
        }

        private void SetMatchOption(int playerNum, int tilesPerGroup)
            => (_matchChooser as MatchChooser)?.SetMatchOption(playerNum, tilesPerGroup);

        public void StartGame()
        {
            
        }
    }
}