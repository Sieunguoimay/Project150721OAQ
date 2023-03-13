using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;
using Gameplay.GameInteract;
using Gameplay.Visual.Board;
using UnityEngine;

namespace Gameplay.Visual.Views
{
    public class InteractSystem : BaseGenericDependencyInversionUnit<InteractSystem>
    {
        [SerializeField] private TileSelector tileSelector;
        private BoardVisualView _boardVisualView;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _boardVisualView = Resolver.Resolve<BoardVisualView>();
        }

        public void DisplayOptions(SimpleMoveOption[] moveOptions, Action<SimpleMoveOption> onSelected)
        {
            var tiles = GetCitizenTiles(moveOptions).ToArray();
            tileSelector.Show(tiles, tile =>
            {
                onSelected?.Invoke(moveOptions.FirstOrDefault(mo => mo.TileIndex == tile.TileIndex));
            });
        }

        private IEnumerable<Tile> GetCitizenTiles(IEnumerable<SimpleMoveOption> moveOptions)
        {
            return moveOptions.Select(mo => _boardVisualView.BoardVisual.Tiles[mo.TileIndex]).Distinct();
        }

        public void Dismiss()
        {
            tileSelector.Dismiss();
        }
    }
}