using System;
using Framework.Resolver;
using Gameplay.Entities.Stage;
using Gameplay.Player;
using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.PlayTurn
{
    [CreateAssetMenu(menuName = "Generator/PlayTurnGenerator")]
    public class PlayTurnDataGenerator : BaseGenericDependencyInversionScriptableObject<PlayTurnDataGenerator>
    {
        public PlayTurnTeller Generate(int turnNum)
        {
            var playTurnTeller = new PlayTurnTeller();
            var turns = CreateTurns(turnNum);
            playTurnTeller.SetTurns(turns, 0);
            return playTurnTeller;
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
            var board = Resolver.Resolve<IGameplayContainer>().Board;
            var boardSide = board.Sides[turnIndex];

            var player = playerFactory.CreatePlayer();
            var decisionMaking = playerFactory.CreatePlayerDecisionMaking(boardSide);
            var pieceBench = playerFactory.CreatePieceBench(boardSide);

            return new PlayTurnData(player, boardSide, decisionMaking, pieceBench);
        }
    }
}