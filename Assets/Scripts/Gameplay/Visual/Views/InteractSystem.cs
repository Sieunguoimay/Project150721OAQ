using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Framework.DependencyInversion;
using Gameplay.CoreGameplay.Interactors.MoveDecisionMaking;
using Gameplay.CoreGameplay.Interactors.OptionSystem;
using Gameplay.Visual.Board;
using Gameplay.Visual.GameInteract;
using SNM;
using UnityEngine;

namespace Gameplay.Visual.Views
{
    public class InteractSystem : SelfBindingDependencyInversionMonoBehaviour
    {
        [SerializeField] private TileSelector tileSelector;
        [SerializeField] private ActionChooser actionChooser;
        
        private BoardVisualView _boardVisualView;
        private Action<OptionQueue> _optionSelectedHandler;
        private TileVisual _selectedTileVisual;
        private Action<bool> _directionSelectedHandler;
        private Action<int> _tileSelectedHandler;

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            _boardVisualView = Resolver.Resolve<BoardVisualView>();
        }

        public void ShowActionChooser(Action<bool> directionSelectedHandler)
        {
            _directionSelectedHandler = directionSelectedHandler;
            UpdateActionChooserPosition();
            actionChooser.ShowUp(OnDirectionSelected);
        }

        public void ShowTileSelector(IEnumerable<int> tileIndices, Action<int>tileSelectedHandler)
        {
            _tileSelectedHandler = tileSelectedHandler;
            var tileVisuals = tileIndices.Select(v => _boardVisualView.BoardVisual.TileVisuals[v]).ToArray();
            tileSelector.Show(tileVisuals, OnTileSelected);
        }

        private void OnTileSelected(TileVisual tileVisual)
        {
            tileSelector.Dismiss();
            PublicExecutor.Instance.Delay(.2f, () =>
            {
                _selectedTileVisual = tileVisual;
                _tileSelectedHandler?.Invoke(tileVisual.TileIndex);
            });
        }

        public void Dismiss()
        {
            tileSelector.Dismiss();
            actionChooser.HideAway();
        }

        private void UpdateActionChooserPosition()
        {
            var tileTransform = _selectedTileVisual.transform;
            var rot = tileTransform.rotation;
            var pos = tileTransform.position + rot * Vector3.forward;

            actionChooser.transform.SetPositionAndRotation(pos, rot);
        }

        private void OnDirectionSelected(bool direction)
        {
            actionChooser.HideAway();
            _directionSelectedHandler?.Invoke(direction);
        }
    }
}