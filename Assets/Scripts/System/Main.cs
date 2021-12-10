using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private GameObject boardPrefab;
    [SerializeField] private GameObject tileSelector;

    private class StateData
    {
        public bool GameOver;
    }

    private readonly StateData _state = new StateData();
    private Board _board;
    private PlayersManager _playerManager;
    private PieceDropper _pieceDropper;
    private TileSelector _tileSelector;
    private Drone _drone;

    private Player CurrentPlayer => _playerManager.CurrentPlayer;
    public static Main Instance { get; private set; }
    public RayPointer RayPointer { get; private set; }

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
        _board = Instantiate(boardPrefab).GetComponent<Board>();
        _board.Setup();

        _pieceDropper = new PieceDropper();
        _pieceDropper.Setup(_board);
        _pieceDropper.OnDone -= OnBunnieDropperDone;
        _pieceDropper.OnDone += OnBunnieDropperDone;
        _pieceDropper.OnEat -= OnEatPieces;
        _pieceDropper.OnEat += OnEatPieces;

        _tileSelector = Instantiate(tileSelector).GetComponent<TileSelector>();
        _tileSelector.Setup();

        _playerManager = new PlayersManager();
        _playerManager.Setup(_board.TileGroups, _tileSelector);
        foreach (var player in _playerManager.Players)
        {
            player.OnDecisionResult += OnDecisionResult;
        }

        this.Delay(1f, StartNewMatch);
    }

    private void StartNewMatch()
    {
        _perMatchData = new PerMatchData(_playerManager.Players.Length);
        CurrentPlayer.MakeDecision(_board);
    }

    private void Update()
    {
        RayPointer.Update(Time.deltaTime);

        if (_state.GameOver)
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                ResetGame();
            }
        }
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

    private void MakeDecision(bool changePlayer)
    {
        bool gameOver = true;

        if (!_board.AreMandarinTilesAllEmpty())
        {
            if (changePlayer)
            {
                _playerManager.ChangePlayer();
            }

            if (Board.IsTileGroupEmpty(CurrentPlayer.TileGroup))
            {
                if (CurrentPlayer.PieceBench.Pieces.Count > 0)
                {
                    if (!CurrentPlayer.TileGroup.TakeBackTiles(CurrentPlayer.PieceBench.Pieces, _pieceDropper))
                    {
                        gameOver = false;
                    }
                }
            }
            else
            {
                CurrentPlayer.MakeDecision(_board);
                gameOver = false;
            }
        }

        if (gameOver)
        {
            GameOver();
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
                var movePos = bench.GetPlacement(bench.Pieces.Count - bench.MandarinCount - 1).Position;
                c.JumpingMoveTo(movePos);
            }
        });
    }

    private void GameOver()
    {
        CheckForWinner();

        if (!_state.GameOver)
        {
            _state.GameOver = true;
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

    private static void TellWinner(int[] scores)
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
        _state.GameOver = false;
        new GameReset(_board, _playerManager).Reset();
        this.Delay(1f, () => { CurrentPlayer.MakeDecision(_board); });
    }
}