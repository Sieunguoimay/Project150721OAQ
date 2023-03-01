using Gameplay.DecisionMaking;
using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.Player
{
    public interface IPlayer
    {
    }

    public class BasePlayer : IPlayer
    {
    }

    public interface IPlayerFactory
    {
        IPlayer CreatePlayer();
        IPlayerDecisionMaking CreatePlayerDecisionMaking(BoardSide boardSide);
        PieceBench CreatePieceBench(BoardSide boardSide);
    }

    public abstract class BasePlayerFactory : IPlayerFactory
    {
        public abstract IPlayer CreatePlayer();
        public abstract IPlayerDecisionMaking CreatePlayerDecisionMaking(BoardSide boardSide);
        public abstract PieceBench CreatePieceBench(BoardSide boardSide);

        protected static PieceBench CreatePieceBench(BoardSide boardSide, Transform transform)
        {
            var pos1 = boardSide.CitizenTiles[0].transform.position;
            var pos2 = boardSide.CitizenTiles[^1].transform.position;
            var diff = pos2 - pos1;
            var pos = pos1 + new Vector3(diff.z, diff.y, -diff.x) * 0.5f;
            var rot = Quaternion.LookRotation(pos1 - pos, Vector3.up);

            var b = new GameObject($"{nameof(PieceBench)}").AddComponent<PieceBench>();
            b.SetArrangement(0.25f, 15);

            var t = b.transform;
            t.SetParent(transform);
            t.position = pos;
            t.rotation = rot;
            return b;
        }
    }

    public class RealPlayerFactory : BasePlayerFactory
    {
        public override IPlayer CreatePlayer()
        {
            return new BasePlayer();
        }

        public override IPlayerDecisionMaking CreatePlayerDecisionMaking(BoardSide boardSide)
        {
            return new PlayerDecisionMaking(boardSide);
        }

        public override PieceBench CreatePieceBench(BoardSide boardSide)
        {
            return CreatePieceBench(boardSide, null);
        }
    }

    public class FakePlayerFactory : BasePlayerFactory
    {
        public override IPlayer CreatePlayer()
        {
            return new BasePlayer();
        }

        public override IPlayerDecisionMaking CreatePlayerDecisionMaking(BoardSide boardSide)
        {
            return new PlayerDecisionMaking(boardSide);
        }

        public override PieceBench CreatePieceBench(BoardSide boardSide)
        {
            return CreatePieceBench(boardSide, null);
        }
    }
}