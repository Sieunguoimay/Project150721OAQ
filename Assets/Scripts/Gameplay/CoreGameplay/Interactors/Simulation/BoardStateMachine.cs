using System;
using System.Collections.Generic;
using Common.DecisionMaking;
using Gameplay.Helpers;

namespace Gameplay.CoreGameplay.Interactors.Simulation
{
    public class BoardStateMachine : BaseBoardStateMachine
    {
        private readonly IStateMachine _stateMachine;

        private bool _endedFlag;

        public BoardStateMachine(IMoveMaker executor)
        {
            _stateMachine = SetupStateMachine(executor);
        }

        protected override void OnHandleIdleStateEnter(IStateMachine stateMachine)
        {
            _endedFlag = true;
        }

        protected override void HandleAnyActionComplete(IStateMachine stateMachine)
        {
            if (_endedFlag)
            {
                InvokeEndEvent();
            }
            else
            {
                NextAction();
            }
        }

        protected override void OnNextAction()
        {
            _endedFlag = false;
            (_stateMachine.CurrentState as BaseBoardState)?.NextAction();
        }
    }

    public interface IBoardPrimitiveMove
    {
        void Grasp(Action doneHandler);
        void Drop(Action doneHandler);
        void Slam(Action doneHandler);
        void Eat(Action doneHandler);
    }

    public interface IMoveMaker : IBoardPrimitiveMove
    {
        bool CanDrop();
        IMoveInnerRules MoveInnerRules { get; }
    }

    public interface IMoveInnerRules
    {
        bool IsThereAnyPiece();
        bool HasReachDeadEnd();
        bool IsEatable();
        bool IsGraspable();
    }

    public class MoveInnerRules<TTile> : IMoveInnerRules
    {
        private readonly IMoveRuleDataHelper _ruleDataHelper;

        public MoveInnerRules(IMoveRuleDataHelper ruleDataHelper)
        {
            _ruleDataHelper = ruleDataHelper;
        }

        public bool IsThereAnyPiece()
        {
            return CurrentTileCount > 0;
        }

        public bool HasReachDeadEnd()
        {
            return CurrentTileCount == 0 && NextTileCount == 0 || IsCurrentTileMandarinTile;
        }

        public bool IsEatable()
        {
            return CurrentTileCount == 0 && NextTileCount > 0;
        }

        public bool IsGraspable()
        {
            return IsThereAnyPiece() && !IsCurrentTileMandarinTile;
        }

        private int CurrentTileCount => _ruleDataHelper.GetNumPiecesInTile(_ruleDataHelper.TileIterator.CurrentTile);
        private int NextTileCount => _ruleDataHelper.GetNumPiecesInTile(_ruleDataHelper.TileIterator.NextTile);
        private int NextTile2Count => _ruleDataHelper.GetNumPiecesInTile(_ruleDataHelper.TileIterator.NextTile2);

        private bool IsCurrentTileMandarinTile =>
            _ruleDataHelper.IsMandarinTile(_ruleDataHelper.TileIterator.CurrentTile);

        public interface IMoveRuleDataHelper
        {
            TileIterator<TTile> TileIterator { get; }
            int GetNumPiecesInTile(TTile tile);
            bool IsMandarinTile(TTile tile);
        }
    }

    public class TileIterator<TTile>
    {
        private readonly IReadOnlyList<TTile> _tiles;
        public TTile CurrentTile { get; private set; }
        public TTile NextTile { get; private set; }
        public TTile NextTile2 { get; private set; }
        private readonly bool _direction;
        public int CurrentTileIndex { get; private set; }

        public TileIterator(IReadOnlyList<TTile> tiles, bool direction)
        {
            _tiles = tiles;
            _direction = direction;
        }

        public void UpdateCurrentTileIndex(int currentTileIndex)
        {
            var nextTileIndex = GetNextTileIndex(currentTileIndex);
            var nextTileIndex2 = GetNextTileIndex(nextTileIndex);

            CurrentTile = _tiles[currentTileIndex];
            NextTile = _tiles[nextTileIndex];
            NextTile2 = _tiles[nextTileIndex2];

            CurrentTileIndex = currentTileIndex;
        }

        private int GetNextTileIndex(int currentTileIndex)
        {
            return BoardTraveller.MoveNext(currentTileIndex, _tiles.Count, _direction);
        }
    }
}