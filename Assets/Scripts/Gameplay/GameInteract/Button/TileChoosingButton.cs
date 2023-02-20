using Gameplay.Board;
using UnityEngine;

namespace Gameplay.GameInteract.Button
{
    public class TileChoosingButton : OnGroundButton
    {
        private bool _isAvailable;
        public override bool IsAvailable => _isAvailable;

        // [field: System.NonSerialized] public ITile Tile { get; private set; }

        // public void SetTile(ITile tile)
        // {
        //     Tile = tile;
        //
        //     // Display.SetDisplayInfo(new ButtonDisplayInfoText($"{tile.HeldPieces.Count}"));
        // }

        public void SetPosition(Transform target, float offset)
        {
            var tilePos = target.position;
            var tileRot = target.rotation;

            var t = transform;
            t.position = tilePos + target.forward * offset;
            t.rotation = tileRot;
        }

        public void SetAvailable(bool available) => _isAvailable = available;
    }
}