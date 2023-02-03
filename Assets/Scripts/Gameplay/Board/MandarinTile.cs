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
        IEnumerable<IPiece> AllPieces { get; }
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

        public IEnumerable<IPiece> AllPieces
        {
            get
            {
                foreach (var p in HeldPieces)
                {
                    yield return p;
                }

                yield return Mandarin;
            }
        }

        public override Vector3 GetPositionInFilledCircle(int index, bool local = false)
        {
            return base.GetPositionInFilledCircle(index + (HasMandarin ? 9 : 0), local);
        }
    }
}