namespace Gameplay.CoreGameplay.Interactors
{
    public interface IEntitiesDataAccess
    {
        BoardData GetBoardData();
    }

    public class BoardData
    {
        public int NumSides;
        public int TilesPerSide;
        public int PiecesPerTile;
    }
}