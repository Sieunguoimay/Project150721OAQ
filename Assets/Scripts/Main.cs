﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Manager;
using SNM;
using UnityEngine;
using UnityEngine.Serialization;

public class Main : MonoBehaviour
{
    [SerializeField] private Transform tester;
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

    public class ReferenceData
    {
        public Camera Camera;
    }

    private Config config;
    private StateData state = new StateData();
    private ReferenceData references = new ReferenceData();

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
    public ReferenceData References => references;

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
        SetupReferenceData();

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

    private void SetupReferenceData()
    {
        references.Camera = Camera.main;
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

    private void OnBunnieDropperEat(IPieceHolder pieceContainerMb)
    {
        var tile = pieceContainerMb as Tile;
        var player = CurrentPlayer;
        var boids = new Boid[pieceContainerMb.Pieces.Count];
        var index = 0;
        player.pieceBench.Grasp(pieceContainerMb, p =>
        {
            var forward = player.TileGroup.GetForward();
            var right = -Vector3.Cross(forward, GameCommonConfig.UpVector);
            var offset = UnityEngine.Random.insideUnitCircle;
            var jumpPos = tile.transform.position + right + new Vector3(offset.x, 0, offset.y);

            var movePos = p is Mandarin
                ? player.pieceBench.GetMandarinPlacement(player.pieceBench.MandarinCount - 1).Position
                : player.pieceBench.GetPlacement(player.pieceBench.Pieces.Count - player.pieceBench.MandarinCount - 1).Position;

            // p.PieceAnimator.Add(new PieceAnimator.JumpAnim(p.transform, new PieceAnimator.JumpTarget {target = jumpPos, height = 2f}));
            boids[index] = new Boid(GameCommonConfig.BoidConfigData,
                new Boid.InputData()
                {
                    target = movePos,
                    transform = p.transform
                }, boids);
            p.PieceAnimator.Add(boids[index]);
            index++;
        });
    }

    private void TakeBackTiles()
    {
        if (CurrentPlayer.pieceBench.Pieces.Count > 0)
        {
            pieceDropper.GetReadyForTakingBackCitizens(CurrentPlayer.TileGroup, CurrentPlayer.pieceBench.Pieces);
            pieceDropper.DropAll(true);
        }
        else
        {
            GameOver();
        }
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
            foreach (var tile in board.TileGroups[i].tiles)
            {
                playerManager.Players[i].pieceBench.Grasp(tile);
            }

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