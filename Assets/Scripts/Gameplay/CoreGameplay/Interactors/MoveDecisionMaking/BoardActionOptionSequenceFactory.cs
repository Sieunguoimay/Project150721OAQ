using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.CoreGameplay.Entities;
using Gameplay.OptionSystem;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class BoardActionOptionSequenceFactory
    {
        private readonly BoardEntityAccess _boardEntityAccess;

        public BoardActionOptionSequenceFactory(BoardEntityAccess boardEntityAccess)
        {
            _boardEntityAccess = boardEntityAccess;
        }

        public OptionQueue CreateOptionSequence(ExtractedTurnData turnData)
        {
            var tileOptionValues = CreateTileOptionValues(turnData.CitizenTileEntitiesOfCurrentTurn);
            var basicOptionItem = new List<OptionItem>
            {
                new TileOptionItem { Values = tileOptionValues },
                CreateDirectionOptionItem()
            };
            var optionItems2 = new List<OptionItem>
            {
                new TileOptionItem { Values = tileOptionValues },
                new TileOptionItem { Values = tileOptionValues },
                CreateDirectionOptionItem()
            };
            var optionQueue = new OptionQueue();
            var dynamicOptionItem = new DynamicOptionItem(optionQueue)
            {
                Values = new OptionValue[]
                {
                    new OptionItemArrayOptionValue(basicOptionItem.ToArray()),
                    new OptionItemArrayOptionValue(optionItems2.ToArray()),
                }
            };
            optionQueue.Options = new List<OptionItem> { dynamicOptionItem };
            optionQueue.TurnIndex = turnData.CurrentTurnIndex;
            return optionQueue;
        }

        public static OptionItem CreateDirectionOptionItem()
        {
            return new DirectionOptionItem
            {
                Values = new OptionValue[]
                {
                    new BooleanOptionValue(true),
                    new BooleanOptionValue(false)
                }
            };
        }

        private OptionValue[] CreateTileOptionValues(IEnumerable<TileEntity> tileEntities)
        {
            var tilesOptionValues = new List<OptionValue>();
            foreach (var tileEntity in tileEntities)
            {
                if (tileEntity.PieceEntities.Count <= 0) continue;
                tilesOptionValues.Add(CreateTileOptionValue(tileEntity));
            }

            return tilesOptionValues.ToArray();
        }

        private IntegerOptionValue CreateTileOptionValue(TileEntity tileEntity)
        {
            var tileIndex = Array.IndexOf(_boardEntityAccess.TileEntities, tileEntity);
            return new IntegerOptionValue(tileIndex);
        }
    }


    public class DirectionOptionItem : OptionItem
    {
        public bool SelectedDirection => ((BooleanOptionValue)SelectedValue).Value;
    }

    public class TileOptionItem : OptionItem
    {
        public int SelectedTileIndex => ((IntegerOptionValue)SelectedValue).Value;
    }

    public class CardOptionItem : DynamicOptionItem
    {
        private readonly string[] _cardIds;
        private readonly OptionValue[] _tileOptionValues;

        public CardOptionItem(OptionQueue optionQueue, string[] cardIds, OptionValue[] tileOptionValues) : base(optionQueue)
        {
            _tileOptionValues = tileOptionValues;
            _cardIds = cardIds;
            CreateOptionValues();
        }

        private void CreateOptionValues()
        {
            var basicOptionItem = new List<OptionItem>
            {
                new TileOptionItem { Values = _tileOptionValues },
                BoardActionOptionSequenceFactory.CreateDirectionOptionItem()
            };
            var optionItems2 = new List<OptionItem>
            {
                new TileOptionItem { Values = _tileOptionValues },
                new TileOptionItem { Values = _tileOptionValues },
                BoardActionOptionSequenceFactory.CreateDirectionOptionItem()
            };
            Values = new OptionValue[]
            {
                new CardOptionValue(basicOptionItem.ToArray(), ""),
                new CardOptionValue(optionItems2.ToArray(), ""),
            };
        }
    }

    public class CardOptionValue : OptionItemArrayOptionValue
    {
        private string _cardId;

        public CardOptionValue(IEnumerable items, string cardId) : base(items)
        {
            _cardId = cardId;
        }
    }
}