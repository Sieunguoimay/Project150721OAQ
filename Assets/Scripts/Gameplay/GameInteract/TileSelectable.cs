using System;
using System.Linq;
using Gameplay.Visual.Board;
using Gameplay.Visual.Piece;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class TileSelectable : MonoBehaviour
    {
        private TileVisual _tileVisual;
        private void OnEnable()
        {
            _tileVisual = GetComponent<TileVisual>();
        }

        public void Select()
        {
            var selectionAdaptors = _tileVisual.HeldPieces.OfType<Citizen>();
            foreach (var sa in selectionAdaptors)
            {
                sa.Animator.PlayAnimStandUp();
            }
        }

        public void Unselect()
        {
            var selectionAdaptors = _tileVisual.HeldPieces.OfType<Citizen>();
            foreach (var sa in selectionAdaptors)
            {
                sa.Animator.PlayAnimSitDown();
            }
        }
    }
}