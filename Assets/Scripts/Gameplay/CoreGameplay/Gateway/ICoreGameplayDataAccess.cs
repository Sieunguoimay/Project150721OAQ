namespace Gameplay.CoreGameplay.Gateway
{
    public interface ICoreGameplayDataAccess
    {
        void RefreshData();
        BoardData GetBoardData();
        TurnData GetTurnData();
    }

    public class BoardData
    {
        public int NumSides;
        public int TilesPerSide;
        public int PiecesPerTile;
    }

    public class TurnData
    {
        public int InitialTurnIndex;
        public int NumTurns;
    }
}