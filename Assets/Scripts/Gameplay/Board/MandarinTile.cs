using System;
using System.Collections.Generic;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public class MandarinTile : Tile
    {
        [field: NonSerialized] public Mandarin Mandarin { get; private set; }
        public bool HasMandarin => Mandarin != null;

        public void SetMandarin(Mandarin mandarin)
        {
            Mandarin = mandarin;
            MandarinChangedEvent?.Invoke(this);
        }

        public event Action<MandarinTile> MandarinChangedEvent;

        public override Vector3 GetPositionAtGridCellIndex(int index, bool local = false)
        {
            return base.GetPositionAtGridCellIndex(index + (HasMandarin ? 9 : 0), local);
        }
    }
}