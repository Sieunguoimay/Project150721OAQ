using System;
using System.Collections.Generic;
using System.Linq;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Entities;

namespace Gameplay.CoreGameplay.Interactors.OptionSystem
{
    public class OptionSequenceFactory : SelfBindingDependencyInversionUnit
    {
        private BoardEntityAccess _boardEntityAccess;
        private TurnDataExtractor _turnDataExtractor;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _boardEntityAccess = Resolver.Resolve<BoardEntityAccess>();
            _turnDataExtractor = Resolver.Resolve<TurnDataExtractor>();
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
            return (from tileEntity in tileEntities
                where tileEntity.PieceEntities.Count > 0
                select CreateTileOptionValue(tileEntity)).Cast<OptionValue>().ToArray();
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