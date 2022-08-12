using Gameplay.Piece;

namespace Gameplay.Board
{
    public class MandarinTile : Tile
    {
        [field: System.NonSerialized] public bool HasMandarin { get; private set; }

        public override void Setup()
        {
            base.Setup();
            HasMandarin = true;
        }

        public override void OnGrasp(IPieceHolder whom)
        {
            if (HasMandarin)
            {
                HasMandarin = false;
            }
        }
    }
}