using System;
using System.Linq;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class ActionChooser : MonoBehaviour
    {
        [SerializeField] private ButtonChooser buttonChooser;
        
        private Action<int> _onResult;

        public void ShowUp(Vector3 position, Quaternion rotation, Action<int> onResult)
        {
            _onResult = onResult;
            transform.position = position;
            transform.rotation = rotation;
            buttonChooser.Setup(6, OnTileChooserResult);
            buttonChooser.ShowButtons();
        }

        private void OnTileChooserResult(int index)
        {
            _onResult?.Invoke(index);
        }
    }
}