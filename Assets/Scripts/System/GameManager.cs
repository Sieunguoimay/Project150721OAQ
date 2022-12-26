using Common.Misc;
using Framework.Entities;
using Framework.Resolver;
using Framework.Services;
using Gameplay;
using Gameplay.BambooStick;
using Gameplay.Board;
using Gameplay.GameInteract;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace System
{
    public class GameManager : MonoBehaviour, ISelfBindingInjectable
    {
        // [SerializeField, IdSelector(typeof(ICurrencyProcessorData))]
        // private string matchProcessorId;

        private readonly Gameplay _gameplay = new();

        // private readonly IMatchChooser _matchChooser = new MatchChooser();
        // private readonly GameFlowManager _gameFlowManager = new();

        private BambooFamilyManager _bambooFamily;
        private PlayersManager _playersManager;
        private BoardManager _boardManager;
        private PieceManager _pieceManager;

        private IResolver _resolver;

        public void Bind(IBinder binder)
        {
            // binder.Bind<GameFlowManager>(_gameFlowManager);
            // binder.Bind<IMatchChooser>(_matchChooser);
        }

        public void Unbind(IBinder binder)
        {
            // binder.Unbind<GameFlowManager>();
            // binder.Unbind<IMatchChooser>();
        }

        public void Inject(IResolver resolver)
        {
            _resolver = resolver;

            _playersManager = resolver.Resolve<PlayersManager>();
            _boardManager = resolver.Resolve<BoardManager>();
            _pieceManager = resolver.Resolve<PieceManager>();
            _bambooFamily = resolver.Resolve<BambooFamilyManager>();

            RayPointer.Instance.SetCamera(resolver.Resolve<CameraManager>().Camera);

            // var matchProcessor = _resolver.Resolve<ICurrencyProcessor>(matchProcessorId);
            // _resolver.Resolve<IMessageService>().Register<IMessage<ICurrencyProcessor>, ICurrencyProcessor>(MatchProcessorSuccess, matchProcessor);
        }

        // private void OnEnable()
        // {
        //     _matchChooser.OnMatchOptionChanged += OnMatchChooserResult;
        // }
        //
        // private void OnDisable()
        // {
        //     _matchChooser.OnMatchOptionChanged -= OnMatchChooserResult;
        // }

        // private void OnCleanup()
        // {
        //     _gameplay.TearDown();
        // }

        private void Update()
        {
            _gameplay.ActivityQueue.Update(Time.deltaTime);
        }

        // private void OnMatchChooserResult()
        // {
        //     // _resolver.Resolve<ICurrencyProcessor>(matchProcessorId).Process();
        // }

        // private void MatchProcessorSuccess(IMessage<ICurrencyProcessor> message)
        // {
        //     var playerNum = _matchChooser.MatchOption.PlayerNum;
        //     var tilesPerGroup = _matchChooser.MatchOption.TilesPerGroup;
        //     GenerateMatch(playerNum, tilesPerGroup);
        //     StartGame();
        // }

        public void GenerateMatch(int playerNum, int tilesPerGroup)
        {
            _boardManager.CreateBoard(playerNum, tilesPerGroup);

            _playersManager.FillWithFakePlayers(playerNum);
            _playersManager.CreatePieceBench(_boardManager.Board);

            _gameplay.Setup(_playersManager, _boardManager.Board, _pieceManager,
                _resolver.Resolve<GameInteractManager>());

            _pieceManager.SpawnPieces(playerNum, tilesPerGroup);

            _bambooFamily.BeginAnimSequence();
        }

        public void StartGame()
        {
            _gameplay.StartNewMatch();
        }
        
        //
        // public void ReplayMatch()
        // {
        //     _gameplay.ResetGame();
        // }
        //
        // public void ResetGame()
        // {
        //     _gameplay.ResetGame();
        //     _gameplay.TearDown();
        // }
    }
}