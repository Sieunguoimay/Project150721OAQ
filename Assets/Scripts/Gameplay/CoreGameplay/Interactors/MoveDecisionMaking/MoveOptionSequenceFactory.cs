using System;
using System.Collections.Generic;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class MoveOptionSequenceFactory
    {
        private readonly BoardEntityAccess _boardEntityAccess;

        public MoveOptionSequenceFactory(BoardEntityAccess boardEntityAccess)
        {
            _boardEntityAccess = boardEntityAccess;
        }

        public OptionQueue CreateMoveOptionSequence(ExtractedTurnData turnData)
        {
            var tileOptionValues = CreateTileOptionValues(turnData.CitizenTileEntitiesOfCurrentTurn);
            return new OptionQueue
            {
                Options = new[]
                {
                    new TileOptionItem { Values = tileOptionValues },
                    // new() {OptionItemType = MoveOptionItemType.Tile, Values = tileOptionValues},
                    CreateDirectionOptionItem(),
                },
                TurnIndex = turnData.CurrentTurnIndex
            };
        }

        private static OptionItem CreateDirectionOptionItem()
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
}