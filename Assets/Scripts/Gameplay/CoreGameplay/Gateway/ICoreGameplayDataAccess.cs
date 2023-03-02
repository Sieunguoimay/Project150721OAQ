namespace Gameplay.CoreGameplay.Gateway
{
    public interface ICoreGameplayDataAccess
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