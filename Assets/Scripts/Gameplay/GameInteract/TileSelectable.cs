using System;
using System.Linq;
using Gameplay.Board;
using Gameplay.Piece;
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
                sa.PlayAnimStandUp();
            }
        }

        public void Unselect()
        {
            var selectionAdaptors = _tile.HeldPieces.OfType<Citizen>();
            foreach (var sa in selectionAdaptors)
            {
                sa.PlayAnimSitDown();
            }
        }
    }
}