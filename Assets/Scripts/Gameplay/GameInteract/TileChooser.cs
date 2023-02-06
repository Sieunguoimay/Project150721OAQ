using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtend.Attribute;
using Gameplay.Board;
using Gameplay.GameInteract.Button;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class TileChooser : MonoBehaviour
    {
        [SerializeField, TypeConstraint(typeof(IButtonContainer))]
        private UnityEngine.Object buttonContainer;

        [SerializeField] private TileChoosingButton tileChoosingButtonPrefab;

        public IButtonContainer ButtonContainer => buttonContainer as IButtonContainer;

        private TileChoosingButton[] _tileChoosingButtons;
        [field: System.NonSerialized] public ITile SelectedTile { get; private set; }
        public event Action<TileChooser, SelectedTileEventArgs> SelectedTileChangedEvent;

        public class SelectedTileEventArgs : EventArgs
        {
            public SelectedTileEventArgs(ITile prevSelectedTile)
            {
                PrevSelectedTile = prevSelectedTile;
            }

            public ITile PrevSelectedTile { get; }
        }

        public void Setup(int numButtons)
        {
            _tileChoosingButtons = new TileChoosingButton[numButtons];
            for (var i = 0; i < numButtons; i++)
            {
                _tileChoosingButtons[i] = Instantiate(tileChoosingButtonPrefab, transform);
                _tileChoosingButtons[i].ClickedEvent += OnButtonClicked;
            }

            ButtonContainer.Setup(_tileChoosingButtons);
        }

        public void TearDown()
        {
            ButtonContainer.TearDown();
            
            foreach (var bt in _tileChoosingButtons)
            {
                bt.ClickedEvent -= OnButtonClicked;
                Destroy(bt.gameObject);
            }

            _tileChoosingButtons = null;
        }

        public void ResetAll()
        {
            ButtonContainer.HideButtons();
        }

        private void OnButtonClicked(IButton obj)
        {
            var prevTile = SelectedTile;
            SelectedTile = (obj as TileChoosingButton)?.Tile;
            SelectedTileChangedEvent?.Invoke(this, new SelectedTileEventArgs(prevTile));

            ButtonContainer.HideButtons();
        }

        public void ChooseTile(IReadOnlyList<ICitizenTile> tiles)
        {
            for (var i = 0; i < _tileChoosingButtons.Length; i++)
            {
                _tileChoosingButtons[i].SetTile(i >= tiles.Count ? null : tiles[i]);
            }

            ButtonContainer.ShowButtons();
        }

        public class ButtonCommand : ButtonContainer.ButtonCommand
        {
            private IButtonContainer _buttonContainer2;

            public ButtonCommand SetButtonContainer2(IButtonContainer buttonContainer)
            {
                _buttonContainer2 = buttonContainer;
                return this;
            }

            public override void Execute(IButton button)
            {
                base.Execute(button);

                foreach (var bv in _buttonContainer2.Buttons)
                {
                    if (bv.Command == this)
                    {
                        bv.HideAway();
                    }
                    else
                    {
                        if (!bv.IsShowing && bv.Command != null)
                        {
                            bv.ShowUp();
                        }
                    }
                }
            }
        }
    }
}