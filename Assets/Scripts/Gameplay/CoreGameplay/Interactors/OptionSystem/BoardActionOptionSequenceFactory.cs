using System;
using System.Collections.Generic;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors.OptionSystem
{
    public class BoardActionOptionSequenceFactory
    {
        private readonly BoardEntityAccess _boardEntityAccess;
        private readonly TurnDataExtractor _turnDataExtractor;

        public BoardActionOptionSequenceFactory(BoardEntityAccess boardEntityAccess,
            TurnDataExtractor turnDataExtractor)
        {
            _boardEntityAccess = boardEntityAccess;
            _turnDataExtractor = turnDataExtractor;
        }

        public OptionQueue CreateOptionSequence()
        {
            return CreateOptionQueueDefault();
        }

        public OptionQueue CreateOptionQueueDefault()
        {
            var tileOptionValues =
                CreateTileOptionValues(_turnDataExtractor.ExtractedTurnData.CitizenTileEntitiesOfCurrentTurn);
            return new OptionQueue
            {
                Options = new[]
                {
                    new TileOptionItem {Values = tileOptionValues},
                    CreateDirectionOptionItem()
                }
            };
        }

        public OptionQueue CreateMultiTilesOptionQueue()
        {
            var tileOptionValues =
                CreateTileOptionValues(_turnDataExtractor.ExtractedTurnData.CitizenTileEntitiesOfCurrentTurn);
            return new OptionQueue
            {
                Options = new[]
                {
                    new TileOptionItem {Values = tileOptionValues},
                    new TileOptionItem {Values = tileOptionValues},
                    CreateDirectionOptionItem()
                }
            };
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
        public bool SelectedDirection => ((BooleanOptionValue) SelectedValue).Value;
    }

    public class TileOptionItem : OptionItem
    {
        public int SelectedTileIndex => ((IntegerOptionValue) SelectedValue).Value;
    }
}