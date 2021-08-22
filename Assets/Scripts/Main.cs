using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Manager;
using UnityEngine;
using UnityEngine.Serialization;

public class Main : MonoBehaviour
{
    [SerializeField] private GameCommonConfig gameCommonConfig;
    [SerializeField] private PrefabManager prefabManager;

    [Serializable]
    public class Config
    {
        [SerializeField] private int playerNum;
        public int PlayerNum => playerNum;
    }

    public class StateData
    {
        public int turn = 0;
        public bool gameOver = false;
    }

    private Config config;
    private StateData state = new StateData();

    private Board board;
    private PlayersManager playerManager;
    private PieceDropper pieceDropper;
    private TileSelector tileSelector;

    private Player CurrentPlayer => playerManager.CurrentPlayer;

    public static Main Instance { get; private set; }
    public RayPointer RayPointer { get; private set; }
    public GameCommonConfig GameCommonConfig => gameCommonConfig;
    public PrefabManager PrefabManager => prefabManager;
    public StateData State => state;

    private PerMatchData perMatchData;

    private void Awake()
    {
        Instance = this;
        if (!Instance)
        {
            Debug.LogError("Main: Error - not instantiatable");
        }

        RayPointer = new RayPointer();
        RayPointer.Reset();
        Debug.Log("Main Awake");
    }

    void Start()
    {
        config = gameCommonConfig.Main;

        board = Prefab.Instantiates(PrefabManager.BoardPrefab);
        board.Setup();

        pieceDropper = new PieceDropper();
        pieceDropper.Setup(board, gameCommonConfig.PieceDropper);
        pieceDropper.OnDone += OnBunnieDropperDone;
        pieceDropper.OnEat += OnBunnieDropperEat;

        tileSelector = Prefab.Instantiates(PrefabManager.TileSelector);
        tileSelector.Setup(gameCommonConfig.TileSelector);

        playerManager = new PlayersManager();
        playerManager.Setup(board.TileGroups, tileSelector);
        foreach (var player in playerManager.Players)
        {
            player.OnDecisionResult += OnDecisionResult;
        }

        this.Delay(1f, StartNewMatch);
    }

    private void StartNewMatch()
    {
        perMatchData = new PerMatchData(playerManager.Players.Length);
        CurrentPlayer.MakeDecision(board);
    }

    private void Update()
    {
        RayPointer.Update(Time.deltaTime);

        if (state.gameOver)
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                ResetGame();
            }
        }
    }

    private void OnDecisionResult(Tile tile, bool forward)
    {
        pieceDropper.GetReady(tile);
        pieceDropper.DropAll(forward);
    }

    private void OnBunnieDropperDone(PieceDropper.ActionID actionID)
    {
        MakeDecision(actionID == PieceDropper.ActionID.DROPPING_IN_TURN);
    }

    private void MakeDecision(bool canChangePlayer)
    {
        if (board.AreMandarinTilesAllEmpty())
        {
            GameOver();
        }
        else
        {
            if (canChangePlayer)
            {
                playerManager.ChangePlayer();
            }

            if (Board.IsTileGroupEmpty(CurrentPlayer.TileGroup))
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
                CurrentPlayer.MakeDecision(board);
            }
        }
    }

    private void OnBunnieDropperEat(PieceContainer pieceContainerMb)
    {
        CurrentPlayer.pieceBench.Grasp(pieceContainerMb);
    }

    private void TakeBackTiles()
    {
        pieceDropper.GetReadyForTakingBackCitizens(CurrentPlayer.TileGroup, CurrentPlayer.pieceBench.Pieces);
        pieceDropper.DropAll(true);
    }

    public void GameOver()
    {
        CheckForWinner();

        if (!state.gameOver)
        {
            state.gameOver = true;
        }
    }

    private void CheckForWinner()
    {
        for (int i = 0; i < playerManager.Players.Length; i++)
        {
            int sum = 0;

            foreach (var p in playerManager.Players[i].pieceBench.Pieces)
            {
                if (p is Citizen)
                {
                    sum += p.ConfigDataProp.point;
                }
                else if (p is Mandarin)
                {
                    sum += p.ConfigDataProp.point;
                }
            }

            foreach (var tile in board.TileGroups[i].tiles)
            {
                foreach (var p in tile.Pieces)
                {
                    sum += p.ConfigDataProp.point;
                }
            }

            Debug.Log("sum " + sum);
            perMatchData.SetPlayerScore(i, sum);
        }

        TellWinner(perMatchData.PlayerScores);
    }

    private void TellWinner(int[] scores)
    {
        if (scores[0] > scores[1])
        {
            Debug.Log("Player 1 win! " + scores[0] + " - " + scores[1]);
        }
        else if (scores[0] < scores[1])
        {
            Debug.Log("Player 2 win! " + scores[0] + " - " + scores[1]);
        }
        else
        {
            Debug.Log("Draw! " + scores[0] + " - " + scores[1]);
        }
    }

    private void ResetGame()
    {
        state.gameOver = false;
        var gameResetter = new GameResetter(board, playerManager);
        gameResetter.Reset();
        this.Delay(1f, () => { CurrentPlayer.MakeDecision(board); });
    }
}