using System;
using Framework.Resolver;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.UI
{
    public class MatchChooserUI : MonoBehaviour, IInjectable, MatchOptionItemUI.IItemHandler
    {
        [SerializeField] private MatchConfig matchConfig;
        [SerializeField] private MatchOptionItemUI itemPrefab;
        [SerializeField] private UnityEvent onSelected;
        [SerializeField] private UnityEvent onReset;
        private IMatchChooser _matchChooser;

        private MatchOptionItemUI[] _itemUIs;

        public void Inject(IResolver resolver)
        {
            _matchChooser = resolver.Resolve<IMatchChooser>();
        }

        private void Start()
        {
            _itemUIs = new MatchOptionItemUI[matchConfig.optionItems.Length];
            for (var i = 0; i < matchConfig.optionItems.Length; i++)
            {
                _itemUIs[i] = Instantiate(itemPrefab, transform);
                _itemUIs[i].Setup(matchConfig.optionItems[i], this);
            }
        }

        private void OnDestroy()
        {
            foreach (var t in _itemUIs)
            {
                t.TearDown();
            }
        }

        public void ItemSelected(IMatchOption item)
        {
            (_matchChooser as MatchChooser)?.SetMatchOption(item);
            onSelected?.Invoke();
        }

        private void OnGameReset()
        {
            onReset?.Invoke();
        }

        public void StartGame()
        {
        }

    }
}