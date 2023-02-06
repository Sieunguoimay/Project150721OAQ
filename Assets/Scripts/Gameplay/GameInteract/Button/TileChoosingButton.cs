using Gameplay.Board;

namespace Gameplay.GameInteract.Button
{
    public class TileChoosingButton : OnGroundButton
    {
        private bool AnyPiecesInTile => Tile != null && Tile.HeldPieces.Count > 0;
        
        public override bool IsAvailable => AnyPiecesInTile;

        [field: System.NonSerialized] public ITile Tile { get; private set; }

        public void SetTile(ITile tile)
        {
            Tile = tile;

            var tilePos = tile.Transform.position;
            var tileRot = tile.Transform.rotation;
            var offset = tile.Transform.forward * tile.Size;

            var t = transform;
            t.position = tilePos + offset;
            t.rotation = tileRot;
            Display.SetDisplayInfo(new ButtonDisplayInfoText($"{tile.HeldPieces.Count}"));
        }
    }
}