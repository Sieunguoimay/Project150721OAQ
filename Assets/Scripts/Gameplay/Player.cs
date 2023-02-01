using System;
using Gameplay.Board;
using Gameplay.GameInteract;

namespace Gameplay
{
    [Serializable]
    public class Player
    {
        public PieceBench PieceBench { get; set; }
        public int Index { get; private set; }

        public Player(int index)
        {
            Index = index;
        }

        public virtual void ResetAll()
        {
            PieceBench.Clear();
        }

        public virtual void ReleaseTurn()
        {
        }

        public virtual void AcquireTurn()
        {
        }
    }
}