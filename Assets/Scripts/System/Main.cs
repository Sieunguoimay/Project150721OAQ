using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Manager;
using SNM;
using SNM.Bezier;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class Main : MonoBehaviour
{
    [SerializeField] private PrefabManager prefabManager;
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private BezierPlotter bezierPlotter;
    [SerializeField] private GameObject boardPrefab;
    [SerializeField] private GameObject tileSelector;

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

    [Serializable]
    public class ReferenceData
    {
        [HideInInspector] public Camera camera;
    }

    [SerializeField] private Config config;
    private readonly StateData _state = new StateData();
    private readonly ReferenceData _references = new ReferenceData();

    private Board _board;
    private PlayersManager _playerManager;
    private PieceDropper _pieceDropper;
    private TileSelector _tileSelector;
    private Drone _drone;

    private Player CurrentPlayer => _playerManager.CurrentPlayer;

    public static Main Instance { get; private set; }

    public RayPointer RayPointer { get; private set; }

    public PrefabManager PrefabManager => prefabManager;
    public StateData State => _state;
    public ReferenceData References => _references;

    private PerMatchData _perMatchData;

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
        PrefabManager.Setup();
        SetupReferences();

        _board = Instantiate(boardPrefab).GetComponent<Board>();
        _board.Setup();

        _pieceDropper = new PieceDropper();
        _pieceDropper.Setup(_board);
        _pieceDropper.OnDone += OnBunnieDropperDone;
        _pieceDropper.OnEat += OnEatPieces;

        _tileSelector = Instantiate(tileSelector).GetComponent<TileSelector>();
        _tileSelector.Setup();

        _playerManager = new PlayersManager();
        _playerManager.Setup(_board.TileGroups, _tileSelector);
        foreach (var player in _playerManager.Players)
        {
            player.OnDecisionResult += OnDecisionResult;
        }

        bezierPlotter?.Setup();
        _drone = Instantiate(dronePrefab).GetComponent<Drone>();
        _drone?.Setup(transform);
        this.Delay(1f, StartNewMatch);
    }

    private void SetupReferences()
    {
        _references.camera = Camera.main;
    }

    private void StartNewMatch()
    {
        _perMatchData = new PerMatchData(_playerManager.Players.Length);
        CurrentPlayer.MakeDecision(_board);
    }

    private void Update()
    {
        RayPointer.Update(Time.deltaTime);

        if (_state.gameOver)
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                ResetGame();
            }
        }

        _drone.Loop(Time.deltaTime);
    }

    private void OnDecisionResult(Tile tile, bool forward)
    {
        _pieceDropper.GetReady(tile);
        _pieceDropper.DropAll(forward);
    }

    private void OnBunnieDropperDone(PieceDropper.ActionID actionID)
    {
        MakeDecision(actionID == PieceDropper.ActionID.DROPPING_IN_TURN);
    }

    private void MakeDecision(bool canChangePlayer)
    {
        if (_board.AreMandarinTilesAllEmpty())
        {
            GameOver();
        }
        else
        {
            if (canChangePlayer)
            {
                _playerManager.ChangePlayer();
            }

            if (Board.IsTileGroupEmpty(CurrentPlayer.TileGroup))
            {
                if (CurrentPlayer.PieceBench.Pieces.Count > 0)
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
                CurrentPlayer.MakeDecision(_board);
            }
        }
    }

    private void OnEatPieces(IPieceHolder pieceContainerMb)
    {
        var bench = CurrentPlayer.PieceBench;

        bench.Grasp(pieceContainerMb, p =>
        {
            if (p is Mandarin)
            {
                var pos = bench.GetMandarinPlacement(bench.MandarinCount - 1);
                _drone.GraspObjectToTarget(p, pos);
            }
            else if (p is Citizen c)
            {
                var movePos = bench
                    .GetPlacement(bench.Pieces.Count - bench.MandarinCount - 1)
                    .Position;
                c.JumpingMoveTo(movePos);
            }
        });
    }

    private void TakeBackTiles()
    {
        if (CurrentPlayer.PieceBench.Pieces.Count > 0)
        {
            _pieceDropper.GetReadyForTakingBackCitizens(CurrentPlayer.TileGroup, CurrentPlayer.PieceBench.Pieces);
            _pieceDropper.DropAll(true);
        }
        else
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        CheckForWinner();

        if (!_state.gameOver)
        {
            _state.gameOver = true;
        }
    }

    private void CheckForWinner()
    {
        for (int i = 0; i < _playerManager.Players.Length; i++)
        {
            foreach (var tile in _board.TileGroups[i].Tiles)
            {
                _playerManager.Players[i].PieceBench.Grasp(tile);
            }

            int sum = 0;

            foreach (var p in _playerManager.Players[i].PieceBench.Pieces)
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
            _perMatchData.SetPlayerScore(i, sum);
        }

        TellWinner(_perMatchData.PlayerScores);
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
        _state.gameOver = false;
        var gameResetter = new GameReset(_board, _playerManager);
        gameResetter.Reset();
        this.Delay(1f, () => { CurrentPlayer.MakeDecision(_board); });
    }

    // private void Test()
    // {
    //     var points = new Vector3[] {new Vector3(), new Vector3(1, -2, 0), new Vector3(2, 1, 0), new Vector3(3, 0, 0)};
    //     float t = 0.0f;
    //     for (; t <= 1.0f; t += 0.1f)
    //     {
    //         var c = Instantiate(PrefabManager.GetPrefab<Drone>());
    //         c.transform.position = SNM.Bezier.Bezier.ComputeBezierCurve3D(points, t);
    //     }
    // }
}