using System.Linq;
using Gameplay.Visual.Views;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class PlayerDecisionMaking : IMoveDecisionMaking, OptionQueueIterator.IOptionQueueIterationHandler
    {
        private readonly InteractSystem _interactSystem;
        private IMoveDecisionMakingResultHandler _driver;
        private OptionQueueIterator _queueIterator;

        public PlayerDecisionMaking(InteractSystem interactSystem)
        {
            _interactSystem = interactSystem;
        }

        public void MakeDecision(OptionQueue optionQueue, IMoveDecisionMakingResultHandler driver)
        {
            _driver = driver;
            _queueIterator = new OptionQueueIterator(optionQueue, this);
            _queueIterator.NextOptionItem();
        }

        public void ForceEnd()
        {
            _interactSystem.Dismiss();
        }

        public void OnOptionsQueueEmpty()
        {
            _driver.OnDecisionResult(IMoveDecisionMaking.CreateResultData(_queueIterator.OptionQueue));
        }

        public void HandleOptionItem()
        {
            switch (_queueIterator.CurrentOptionItem)
            {
                case TileOptionItem:
                    HandleTilesOption();
                    break;
                case DirectionOptionItem:
                    HandleDirectionsOption();
                    break;
            }
        }

        private void HandleTilesOption()
        {
            var values = _queueIterator.CurrentOptionItem.Values.Where(v => _queueIterator.OptionQueue.Options.All(o => o.SelectedValue != v));

            _interactSystem.ShowTileSelector(values.Select(v => ((IntegerOptionValue)v).Value), tileIndex =>
            {
                _queueIterator.CurrentOptionItem.SelectedValue = _queueIterator.CurrentOptionItem.Values.FirstOrDefault(v => ((IntegerOptionValue)v).Value == tileIndex);

                _queueIterator.NextOptionItem();
            });
        }

        private void HandleDirectionsOption()
        {
            _interactSystem.ShowActionChooser((direction) =>
            {
                _queueIterator.CurrentOptionItem.SelectedValue = _queueIterator.CurrentOptionItem.Values.FirstOrDefault(v => ((BooleanOptionValue)v).Value == direction);
                _queueIterator.NextOptionItem();
            });
        }
    }
}