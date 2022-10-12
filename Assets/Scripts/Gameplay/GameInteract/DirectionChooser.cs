using System;
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
            _buttons = new ButtonChooser.ButtonData[3];
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
        }

        public void ChooseDirection(Vector3 position, Quaternion rotation, Action<int> onResult)
        {
            _onResult = onResult;
            _buttons[0].position = position + rotation * Vector3.right * spacing;
            _buttons[0].rotation = rotation;
            _buttons[1].position = position - rotation * Vector3.right * spacing;
            _buttons[1].rotation = rotation;
            _buttons[2].position = position;
            _buttons[2].rotation = rotation;
            buttonChooser.ShowButtons(_buttons, OnTileChooserResult);
        }

        private void OnTileChooserResult(int id)
        {
            _onResult?.Invoke(_buttons[id].ID);
        }
    }
}