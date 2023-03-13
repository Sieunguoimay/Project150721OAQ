using System;
using System.Collections.Generic;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class MoveDecisionOptionFactory
    {
        private readonly BoardEntityAccess _boardEntityAccess;

        public MoveDecisionOptionFactory(BoardEntityAccess boardEntityAccess)
        {
            _boardEntityAccess = boardEntityAccess;
        }

        public MoveOptionQueue CreateDecisionMakingData(ExtractedTurnData turnData)
        {
            var tileOptionValues = CreateTileOptionValues(turnData.CitizenTileEntitiesOfCurrentTurn);
            return new MoveOptionQueue
            {
                Options = new MoveOptionItem[]
                {
                    new() {OptionItemType = MoveOptionItemType.Tile, Values = tileOptionValues},
                    new() {OptionItemType = MoveOptionItemType.Tile, Values = tileOptionValues},
                    new() {OptionItemType = MoveOptionItemType.Direction, Values = new MoveOptionValue[] {new BooleanOptionValue(true), new BooleanOptionValue(false)}}
                },
                TurnIndex = turnData.CurrentTurnIndex
            };
        }

        private MoveOptionValue[] CreateTileOptionValues(IEnumerable<TileEntity> tileEntities)
        {
            var tilesOptionValues = new List<MoveOptionValue>();
            foreach (var tileEntity in tileEntities)
            {
                if (tileEntity.PieceEntities.Count <= 0) continue;

                var tileIndex = Array.IndexOf(_boardEntityAccess.TileEntities, tileEntity);
                tilesOptionValues.Add(new IntegerOptionValue(tileIndex));
            }

            return tilesOptionValues.ToArray();
        }
    }


    public class MoveOptionQueue
    {
        public MoveOptionItem[] Options;
        public int TurnIndex;
    }

    public class MoveOptionItem
    {
        public MoveOptionValue[] Values;
        public MoveOptionValue SelectedValue;
        public MoveOptionItemType OptionItemType;
    }

    public enum MoveOptionItemType
    {
        Direction,
        Tile
    }

    public abstract class MoveOptionValue
    {
        protected object DynamicValue;

        public MoveOptionValue(object value)
        {
            DynamicValue = value;
        }
    }

    public class BooleanOptionValue : MoveOptionValue
    {
        public bool Value => DynamicValue is true;

        public BooleanOptionValue(object value) : base(value)
        {
        }
    }

    public class IntegerOptionValue : MoveOptionValue
    {
        public int Value => DynamicValue is int value ? value : 0;

        public IntegerOptionValue(object value) : base(value)
        {
        }
    }

    public class MoveOptionQueueIterator
    {
        private readonly IMoveOptionQueueIterationHandler _handler;

        private int _optionQueueIndex;

        public MoveOptionQueueIterator(MoveOptionQueue moveOptionQueue, IMoveOptionQueueIterationHandler handler)
        {
            MoveOptionQueue = moveOptionQueue;
            _handler = handler;
            _optionQueueIndex = 0;
        }

        public MoveOptionQueue MoveOptionQueue { get; }

        public MoveOptionItem CurrentOptionItem { get; private set; }

        public void DequeueNextOptionItem()
        {
            if (_optionQueueIndex >= MoveOptionQueue.Options.Length)
            {
                _handler.OnOptionsQueueEmpty();
                return;
            }

            CurrentOptionItem = MoveOptionQueue.Options[_optionQueueIndex++];

            switch (CurrentOptionItem.OptionItemType)
            {
                case MoveOptionItemType.Tile:
                    _handler.HandleTilesOption();
                    break;
                case MoveOptionItemType.Direction:
                    _handler.HandleDirectionsOption();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public interface IMoveOptionQueueIterationHandler
        {
            void OnOptionsQueueEmpty();
            void HandleTilesOption();
            void HandleDirectionsOption();
        }
    }
}