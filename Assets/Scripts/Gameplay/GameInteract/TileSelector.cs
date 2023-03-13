using System;
using System.Collections.Generic;
using Gameplay.GameInteract.Button;
using Gameplay.Visual.Board;
using SNM;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class TileSelector : MonoBehaviour
    {
        [SerializeField] private ButtonOnGround tileChoosingButtonPrefab;

        private IButtonFactory _buttonFactory;
        private IButton[] _buttons;
        private ButtonGroup _buttonGroup;
        private Action<TileVisual> _onSelected;
        private IReadOnlyList<TileVisual> _tiles;

        public void Show(IReadOnlyList<TileVisual> tiles, Action<TileVisual> onSelected)
        {
            _tiles = tiles;
            _onSelected = onSelected;
            TrySpawnButtons(tiles.Count);
            SetupButtons(tiles);
            this.ExecuteInNextFrame(() => { _buttonGroup.ShowButtons(); });
        }

        public void Dismiss()
        {
            _onSelected = null;
            _buttonGroup.HideButtons();
        }

        private void OnButtonClicked(IButton obj)
        {
            _onSelected?.Invoke(_tiles[Array.IndexOf(_buttons, obj)]);
        }

        private void TrySpawnButtons(int btnNum)
        {
            if (_buttons != null && btnNum > _buttons.Length)
            {
                DestroyButtons();
            }

            if (_buttons == null)
            {
                SpawnButtons(btnNum);
            }
        }

        private void SpawnButtons(int btnNum)
        {
            _buttonFactory = new ButtonFactory(tileChoosingButtonPrefab, transform);
            _buttons = new IButton[btnNum];

            for (var i = 0; i < btnNum; i++)
            {
                _buttons[i] = _buttonFactory.Spawn();
            }

            _buttonGroup = new ButtonGroup(_buttons);
        }

        private void SetupButtons(IReadOnlyList<TileVisual> tiles)
        {
            for (var i = 0; i < tiles.Count; i++)
            {
                SetupButton(_buttons[i], tiles[i].transform);
            }
        }

        private void SetupButton(IButton button, Transform tileTransform)
        {
            var rot = tileTransform.rotation;
            var pos = CalculateButtonPosition(tileTransform, 1);

            button.SetPositionAndRotation(pos, rot);
            button.ClickedEvent += OnButtonClicked;
        }

        private void DestroyButtons()
        {
            foreach (var button in _buttons)
            {
                Destroy((button as Component)?.gameObject);
            }

            _buttons = null;
        }

        private static Vector3 CalculateButtonPosition(Transform target, float offset)
        {
            return target.position + target.forward * offset;
        }
    }
}