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
    [SerializeField] private GameCommonConfig gameCommonConfig;
    [SerializeField] private PrefabManager prefabManager;
    [SerializeField] private Drone drone;
    [SerializeField] private BezierPlotter bezierPlotter;

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

    private Config _config;
    private readonly StateData _state = new StateData();
    private readonly ReferenceData _references = new ReferenceData();

    private Board _board;
    private PlayersManager _playerManager;
    private PieceDropper _pieceDropper;
    private TileSelector _tileSelector;

    private Player CurrentPlayer => _playerManager.CurrentPlayer;

    public static Main Instance { get; private set; }
    public RayPointer RayPointer { get; private set; }
    public GameCommonConfig GameCommonConfig => gameCommonConfig;
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
        _config = gameCommonConfig.Main;
        PrefabManager.Setup();
        SetupReferences();

        _board = Instantiate(PrefabManager.GetPrefab<Board>()).GetComponent<Board>();
        _board.Setup();

        _pieceDropper = new PieceDropper();
        _pieceDropper.Setup(_board, gameCommonConfig.PieceDropper);
        _pieceDropper.OnDone += OnBunnieDropperDone;
        _pieceDropper.OnEat += OnEatPieces;

        _tileSelector = Instantiate(PrefabManager.GetPrefab<TileSelector>()).GetComponent<TileSelector>();
        _tileSelector.Setup(gameCommonConfig.TileSelector);

        _playerManager = new PlayersManager();
        _playerManager.Setup(_board.TileGroups, _tileSelector);
        foreach (var player in _playerManager.Players)
        {
            player.OnDecisionResult += OnDecisionResult;
        }

        bezierPlotter?.Setup();
        drone?.Setup(null, null);
        this.Delay(1f, StartNewMatch);
        // Test();
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

        drone.Loop(Time.deltaTime);
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
                CurrentPlayer.MakeDecision(_board);
            }
        }
    }

    private void OnEatPieces(IPieceHolder pieceContainerMb)
    {
        var tile = pieceContainerMb as Tile;
        var player = CurrentPlayer;
        var boids = new Boid[pieceContainerMb.Pieces.Count(p => p is Citizen)];
        var index = 0;
        player.pieceBench.Grasp(pieceContainerMb, p =>
        {
            if (p is Mandarin)
            {
                var pos = player.pieceBench.GetMandarinPlacement(player.pieceBench.MandarinCount - 1);
                drone.GraspObjectToTarget(p, pos);
                return;
            }

            var forward = player.TileGroup.GetForward();
            var right = -Vector3.Cross(forward, GameCommonConfig.UpVector);
            var offset = UnityEngine.Random.insideUnitCircle;
            var jumpPos = tile.transform.position + right + new Vector3(offset.x, 0, offset.y);

            var movePos = player.pieceBench
                .GetPlacement(player.pieceBench.Pieces.Count - player.pieceBench.MandarinCount - 1)
                .Position;

            // p is Mandarin
            //     ? player.pieceBench.GetMandarinPlacement(player.pieceBench.MandarinCount - 1).Position
            //     :

            // p.PieceAnimator.Add(new PieceAnimator.JumpAnim(p.transform, new PieceAnimator.JumpTarget {target = jumpPos, height = 2f}));
            boids[index] = new JumpingBoid(
                new Boid.ConfigData()
                {
                    arriveDistance = GameCommonConfig.BoidConfigData.arriveDistance,
                    maxAcceleration = GameCommonConfig.BoidConfigData.maxAcceleration,
                    maxSpeed = GameCommonConfig.BoidConfigData.maxSpeed,
                    spacing = GameCommonConfig.BoidConfigData.spacing,
                },
                //GameCommonConfig.BoidConfigData,
                new Boid.InputData()
                {
                    target = movePos,
                    transform = p.transform
                }, boids);
            p.PieceActor.Add(new CommonActivities.Delay(0.08f * index));
            p.PieceActor.Add(boids[index]);
            index++;
        });
    }

    private void TakeBackTiles()
    {
        if (CurrentPlayer.pieceBench.Pieces.Count > 0)
        {
            _pieceDropper.GetReadyForTakingBackCitizens(CurrentPlayer.TileGroup, CurrentPlayer.pieceBench.Pieces);
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
            foreach (var tile in _board.TileGroups[i].tiles)
            {
                _playerManager.Players[i].pieceBench.Grasp(tile);
            }

            int sum = 0;

            foreach (var p in _playerManager.Players[i].pieceBench.Pieces)
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
        var gameResetter = new GameResetter(_board, _playerManager);
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