using System;
using System.Collections.Generic;
using System.Linq;
using Common.DecisionMaking;
using UnityEngine;

namespace Gameplay.Board
{
    public interface IBoardStateDriver
    {
        void NextAction();
        event Action<IBoardStateDriver> EndEvent;
    }

    public abstract class BaseBoardStateMachine : IBoardStateDriver
    {
        /*
         * Action List:
         * - Grasp a tile
         * - Drop Single Piece into a tile
         * - Slam the empty tile before eating
         * - Eat a tile
         * - Do nothing
        */

        /*
         * State List:
         * - State Idle: Wait For Signal to begin
         * - Grasp a Tile
         * - Keep Dropping till end
         * - Slam & Eat
        */

        /*
         * Interface
         * - AssignStartingCondition(): void
         * - NextAction(): void
         * - EndEvent: Action
         */

        protected IStateMachine SetupStateMachine(IMoveMaker executor)
        {
            var stateMachine = new StateMachine();
            var boardStates = new BaseBoardState[]
            {
                new StateIdle(stateMachine, HandleIdleStateEnter),
                new GraspATile(stateMachine),
                new KeepDropping(stateMachine),
                new SlamAndEat(stateMachine)
            };

            stateMachine.SetStates(boardStates.Select(s => s as IState).ToArray());

            foreach (var s in boardStates)
            {
                s.Setup(HandleAnyActionComplete, executor);
            }

            return stateMachine;
        }

        private void HandleIdleStateEnter(BaseBoardState baseBoardState)
        {
            OnHandleIdleStateEnter(baseBoardState.StateMachine);
        }

        protected abstract void OnHandleIdleStateEnter(IStateMachine stateMachine);

        protected abstract void HandleAnyActionComplete();

        public abstract void NextAction();

        protected void InvokeEndEvent() => EndEvent?.Invoke(this);

        public event Action<IBoardStateDriver> EndEvent;


        protected abstract class BaseBoardState : BaseState
        {
            protected IMoveMaker Executor { get; private set; }
            private Action _actionCompleteHandler;

            protected BaseBoardState(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            protected abstract void OnSetup();
            public abstract void NextAction();

            public void Setup(Action actionCompleteHandler, IMoveMaker executor)
            {
                Executor = executor;
                _actionCompleteHandler = actionCompleteHandler;
                OnSetup();
            }

            protected void InvokeActionCompleteHandler()
            {
                _actionCompleteHandler?.Invoke();
            }
        }

        private class StateIdle : BaseBoardState
        {
            private GraspATile _graspATile;

            private readonly Action<BaseBoardState> _stateEnterHandler;

            public StateIdle(IStateMachine stateMachine, Action<BaseBoardState> stateEnterHandler) : base(stateMachine)
            {
                _stateEnterHandler = stateEnterHandler;
            }

            protected override void OnEnter()
            {
                Debug.Log("Perform Action: Idle");
                _stateEnterHandler?.Invoke(this);
            }

            protected override void OnExit()
            {
            }

            protected override void OnSetup()
            {
                _graspATile = StateMachine.States.FirstOrDefault(s => s is GraspATile) as GraspATile;
            }

            public override void NextAction()
            {
                StateMachine.ChangeState(_graspATile);
            }
        }

        private class GraspATile : BaseBoardState
        {
            private KeepDropping _keepDropping;
            private StateIdle _stateIdle;

            public GraspATile(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            protected override void OnEnter()
            {
                if (Executor.MoveInnerRules.IsThereAnyPiece())
                {
                    Debug.Log("Perform Action: Grasp a tile");
                    Executor.Grasp(InvokeActionCompleteHandler);
                }
                else
                {
                    StateMachine.ChangeState(_stateIdle);
                }
            }

            protected override void OnExit()
            {
            }

            protected override void OnSetup()
            {
                _keepDropping = StateMachine.States.FirstOrDefault(s => s is KeepDropping) as KeepDropping;
                _stateIdle = StateMachine.States.FirstOrDefault(s => s is StateIdle) as StateIdle;
            }

            public override void NextAction()
            {
                StateMachine.ChangeState(_keepDropping);
            }
        }

        private class KeepDropping : BaseBoardState
        {
            private StateIdle _stateIdle;
            private SlamAndEat _slamAndEat;
            private GraspATile _graspATile;

            public KeepDropping(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            protected override void OnEnter()
            {
                Drop();
            }

            protected override void OnExit()
            {
            }

            protected override void OnSetup()
            {
                _stateIdle = StateMachine.States.FirstOrDefault(s => s is StateIdle) as StateIdle;
                _slamAndEat = StateMachine.States.FirstOrDefault(s => s is SlamAndEat) as SlamAndEat;
                _graspATile = StateMachine.States.FirstOrDefault(s => s is GraspATile) as GraspATile;
            }

            public override void NextAction()
            {
                if (!Drop())
                {
                    StateMachine.ChangeState(GetNextState());
                }
            }

            private IState GetNextState()
            {
                if (Executor.MoveInnerRules.HasReachDeadEnd()) return _stateIdle;
                if (Executor.MoveInnerRules.IsGraspable()) return _graspATile;
                if (Executor.MoveInnerRules.IsEatable()) return _slamAndEat;
                throw new Exception("Invalid condition");
            }

            private bool Drop()
            {
                if (!Executor.CanDrop()) return false;
                Debug.Log($"Perform action: Drop a piece");
                Executor.Drop(InvokeActionCompleteHandler);
                return true;
            }
        }

        private class SlamAndEat : BaseBoardState
        {
            private StateIdle _stateIdle;
            private bool _slam;
            private bool _eat;

            public SlamAndEat(IStateMachine stateMachine) : base(stateMachine)
            {
            }

            protected override void OnEnter()
            {
                _slam = true;
                _eat = true;
                SlamOrEat();
            }

            protected override void OnExit()
            {
            }

            protected override void OnSetup()
            {
                _stateIdle = StateMachine.States.FirstOrDefault(s => s is StateIdle) as StateIdle;
            }

            public override void NextAction()
            {
                SlamOrEat();
            }

            private void SlamOrEat()
            {
                if (_slam)
                {
                    _slam = false;
                    Debug.LogError("Perform Action: Slam");
                    Executor.Slam(InvokeActionCompleteHandler);
                }
                else
                {
                    if (_eat)
                    {
                        _eat = false;
                        Debug.LogError("Perform Action: Eat");
                        Executor.Eat(InvokeActionCompleteHandler);
                    }
                    else
                    {
                        if (Executor.MoveInnerRules.IsEatable())
                        {
                            //Move next
                            _slam = true;
                            _eat = true;
                            SlamOrEat();
                        }
                        else
                        {
                            StateMachine.ChangeState(_stateIdle);
                        }
                    }
                }
            }
        }
    }

    public class BoardStateMachine : BaseBoardStateMachine
    {
        private readonly IStateMachine _stateMachine;

        public BoardStateMachine(IMoveMaker executor)
        {
            _stateMachine = SetupStateMachine(executor);
        }

        protected override void OnHandleIdleStateEnter(IStateMachine stateMachine)
        {
            InvokeEndEvent();
        }

        protected override void HandleAnyActionComplete()
        {
            NextAction();
        }

        public override void NextAction()
        {
            (_stateMachine.CurrentState as BaseBoardState)?.NextAction();
        }
    }

    public interface IMoveMaker
    {
        void Grasp(Action doneHandler);
        void Drop(Action doneHandler);
        void Slam(Action doneHandler);
        void Eat(Action doneHandler);
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
        private bool IsCurrentTileMandarinTile => _ruleDataHelper.IsMandarinTile(_ruleDataHelper.TileIterator.CurrentTile);

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
        }

        private int GetNextTileIndex(int currentTileIndex)
        {
            return BoardTraveller.MoveNext(currentTileIndex, _tiles.Count, _direction);
        }
    }
}