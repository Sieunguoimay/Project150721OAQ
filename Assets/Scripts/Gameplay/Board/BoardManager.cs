using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.Board
{
    public interface ITileFactory
    {
        MandarinTile CreateMandarinTile(Vector2 cornerPos);

        CitizenTile CreateCitizenTile(Vector2 cornerPos, Vector2 verticalOffset,
            Vector2 horizontalOffset);
    }

    public class BoardManager : BaseGenericDependencyInversionUnit<BoardManager>, ITileFactory
    {
        [SerializeField] private MandarinTile mandarinTilePrefab;
        [SerializeField] private CitizenTile citizenTilePrefab;
        [field: System.NonSerialized] public Board Board { get; private set; }
        public event Action<BoardManager> BoardCreatedEvent;

        public void DeleteBoard()
        {
            foreach (var tile in Board.Tiles)
            {
                Destroy(tile.Transform.gameObject);
            }

            Board = null;
        }

        public void CreateBoard(int groupNum, int tilesPerGroup)
        {
            Board = BoardCreator.CreateBoard(groupNum, tilesPerGroup, citizenTilePrefab, this);
            BoardCreatedEvent?.Invoke(this);
        }

        public MandarinTile CreateMandarinTile(Vector2 cornerPos)
        {
            var parent = transform;
            var localPos = cornerPos + cornerPos.normalized * mandarinTilePrefab.Size / 2f;
            var worldPos = parent.TransformPoint(BoardCreator.ToVector3(localPos));
            var worldRot = parent.rotation * Quaternion.LookRotation(BoardCreator.ToVector3(cornerPos));
            return SpawnTile(mandarinTilePrefab, worldPos, worldRot, parent) as MandarinTile;
        }

        public CitizenTile CreateCitizenTile(Vector2 cornerPos, Vector2 verticalOffset, Vector2 horizontalOffset)
        {
            var parent = transform;
            var localPos = cornerPos + verticalOffset * citizenTilePrefab.Size +
                           horizontalOffset * citizenTilePrefab.Size;
            var worldPos = parent.TransformPoint(BoardCreator.ToVector3(localPos));
            var worldRot = parent.rotation * Quaternion.LookRotation(BoardCreator.ToVector3(verticalOffset));
            return SpawnTile(citizenTilePrefab, worldPos, worldRot, parent) as CitizenTile;
        }

        private static Tile SpawnTile(Tile tilePrefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            var tile = Instantiate(tilePrefab, parent);
            tile.transform.SetPositionAndRotation(position, rotation);
            return tile;
        }
    }
}