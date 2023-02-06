using System;
using System.Collections.Generic;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IMandarinTile : ITile
    {
        IMandarin Mandarin { get; }
        void SetMandarin(IMandarin mandarin);
        event Action<IMandarinTile> MandarinChangedEvent;
    }

    public class MandarinTile : Tile, IMandarinTile
    {
        [field: NonSerialized] public IMandarin Mandarin { get; private set; }
        public bool HasMandarin => Mandarin != null;

        public void SetMandarin(IMandarin mandarin)
        {
            Mandarin = mandarin;
            MandarinChangedEvent?.Invoke(this);
        }

        public event Action<IMandarinTile> MandarinChangedEvent;

        public override Vector3 GetGridPosition(int index, bool local = false)
        {
            return base.GetGridPosition(index + (HasMandarin ? 9 : 0), local);
        }
    }
}