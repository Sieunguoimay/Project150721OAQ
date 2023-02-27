using System;
using Framework.Resolver;
using Gameplay.Board;
using Gameplay.Entities.Stage;
using Gameplay.Player;
using UnityEngine;

namespace Gameplay.PlayTurn
{
    [CreateAssetMenu(menuName = "Generator/PlayTurnGenerator")]
    public class PlayTurnDataGenerator : BaseGenericDependencyInversionScriptableObject<PlayTurnDataGenerator>
    {
        private readonly PlayTurnTeller _playTurnTeller = new();

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            binder.Bind<IPlayTurnTeller>(_playTurnTeller);
        }

        public void Generate(int turnNum)
        {
            var turns = CreateTurns(turnNum);
            _playTurnTeller.SetTurns(turns, 0);
        }

        private PlayTurnData[] CreateTurns(int turnNum)
        {
            var turns = new PlayTurnData[turnNum];

            turns[0] = CreatePlayTurnDataForRealPlayer(0);

            for (var i = 1; i < turnNum; i++)
            {
                turns[i] = CreatePlayTurnDataForFakePlayer(i);
            }

            return turns;
        }

        private PlayTurnData CreatePlayTurnDataForRealPlayer(int turnIndex)
        {
            return CreatePlayTurnData(turnIndex, new RealPlayerFactory());
        }

        private PlayTurnData CreatePlayTurnDataForFakePlayer(int turnIndex)
        {
            return CreatePlayTurnData(turnIndex, new FakePlayerFactory());
        }

        private PlayTurnData CreatePlayTurnData(int turnIndex, IPlayerFactory playerFactory)
        {
            var board = Resolver.Resolve<BoardManager>().Board;
            var boardSide = board.Sides[turnIndex];

            var player = playerFactory.CreatePlayer();
            var decisionMaking = playerFactory.CreatePlayerDecisionMaking(boardSide);
            var pieceBench = playerFactory.CreatePieceBench(boardSide);

            return new PlayTurnData(player, boardSide, decisionMaking, pieceBench);
        }
    }
}