using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;
using UnityEngine.Serialization;

public class Main : MonoBehaviour
{
    [SerializeField] private ConfigData config = new ConfigData();

    public class ConfigData
    {
        public int playerNum = 2;
    }

    public class StateData
    {
        public int turn = 0;
    }

    private Board board;
    private TileSelector tileSelector;
    private PlayersManager playerManager;
    private PieceDropper pieceDropper;
    private StateData state = new StateData();
    private Player CurrentPlayer => playerManager.Players[state.turn];

    void Start()
    {
        board = Prefab.Instantiates(PrefabManager.Instance.BoardPrefab);
        board.Setup();

        pieceDropper = SNM.Utils.NewGameObject<PieceDropper>();
        pieceDropper.Setup(board);
        pieceDropper.OnDone += OnBunnieDropperDone;
        pieceDropper.OnEat += OnBunnieDropperEat;

        playerManager = new PlayersManager();
        playerManager.Setup(board.TileGroups);

        tileSelector = SNM.Utils.NewGameObject<TileSelector>();
        tileSelector.OnDone += OnTileSelectorDone;
        tileSelector.Display(CurrentPlayer.tileGroup);
    }

    private void OnTileSelectorDone(Tile tile, bool forward)
    {
        pieceDropper.GetReady(tile);
        pieceDropper.DropAll(forward);
    }

    private void OnBunnieDropperDone(PieceDropper.ActionID actionID)
    {
        if (board.AreMandarinTilesAllEmpty())
        {
            GameOver();
        }
        else
        {
            if (actionID == PieceDropper.ActionID.DROPPING_IN_TURN)
            {
                ChangePlayer();
            }

            if (Board.IsTileGroupEmpty(CurrentPlayer.tileGroup))
            {
                if (CurrentPlayer.pieceBench.Pieces.Count > 0)
                {
                    TakeBackTiles();
                }
                else
                {
                    GameOver();
                }
            }
            else
            {
                tileSelector.Display(CurrentPlayer.tileGroup);
            }
        }
    }

    private void ChangePlayer()
    {
        state.turn = (state.turn + 1) % playerManager.Players.Length;
    }

    private void OnBunnieDropperEat(PieceContainer pieceContainerMb)
    {
        CurrentPlayer.pieceBench.Grasp(pieceContainerMb);
    }

    private void TakeBackTiles()
    {
        pieceDropper.GetReadyForTakingBackCitizens(CurrentPlayer.tileGroup, CurrentPlayer.pieceBench.Pieces);
        pieceDropper.DropAll(true);
    }

    public void GameOver()
    {
        Debug.Log("Game over");
    }
}