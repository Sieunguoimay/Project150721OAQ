using System;
using System.Linq;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class DirectionChooser : MonoBehaviour
    {
        [SerializeField] private ButtonChooser buttonChooser;
        [SerializeField, Min(0f)] private float spacing = .3f;

        private ButtonChooser.ButtonData[] _buttons;
        private Action<int> _onResult;

        private void Start()
        {
            _buttons = new ButtonChooser.ButtonData[6];
            _buttons[0] = new ButtonChooser.ButtonData
            {
                ID = 0
            };
            _buttons[1] = new ButtonChooser.ButtonData
            {
                ID = 1
            };
            _buttons[2] = new ButtonChooser.ButtonData
            {
                ID = 2
            };
            _buttons[3] = new ButtonChooser.ButtonData
            {
                ID = 3
            };
            _buttons[4] = new ButtonChooser.ButtonData
            {
                ID = 4
            };
            _buttons[5] = new ButtonChooser.ButtonData
            {
                ID = 5
            };
        }

        public void ChooseDirection(Vector3 position, Quaternion rotation, Action<int> onResult)
        {
            _onResult = onResult;
            _buttons[0].Position = position + rotation * Vector3.right * spacing;
            _buttons[0].Rotation = rotation;
            _buttons[1].Position = position - rotation * Vector3.right * spacing;
            _buttons[1].Rotation = rotation;
            _buttons[2].Position = position;
            _buttons[2].Rotation = rotation;
            buttonChooser.Setup(_buttons, OnTileChooserResult);
            buttonChooser.SetButtonsPositionAndRotation(_buttons.Select(b=>(b.Position,b.Rotation)).ToList().GetRange(0,3));
            buttonChooser.ShowButtons();
        }

        private void OnTileChooserResult(int id)
        {
            _onResult?.Invoke(_buttons[id].ID);
        }
    }
}