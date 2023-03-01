using System;
using System.Linq;
using Gameplay.Visual.Board;
using Gameplay.Visual.Piece;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public class TileSelectable : MonoBehaviour
    {
        private Tile _tile;
        private void OnEnable()
        {
            _tile = GetComponent<Tile>();
        }

        public void Select()
        {
            var selectionAdaptors = _tile.HeldPieces.OfType<Citizen>();
            foreach (var sa in selectionAdaptors)
            {
                sa.Animator.PlayAnimStandUp();
            }
        }

        public void Unselect()
        {
            var selectionAdaptors = _tile.HeldPieces.OfType<Citizen>();
            foreach (var sa in selectionAdaptors)
            {
                sa.Animator.PlayAnimSitDown();
            }
        }
    }
}