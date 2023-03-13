using System.Linq;
using Gameplay.Visual.Views;

namespace Gameplay.CoreGameplay.Interactors.MoveDecisionMaking
{
    public class PlayerMoveDecisionMaking : IMoveDecisionMaking, MoveOptionQueueIterator.IMoveOptionQueueIterationHandler
    {
        private readonly InteractSystem _interactSystem;
        private IMoveDecisionMakingResultHandler _driver;
        private MoveOptionQueueIterator _queueIterator;

        public PlayerMoveDecisionMaking(InteractSystem interactSystem)
        {
            _interactSystem = interactSystem;
        }

        public void MakeDecision(MoveOptionQueue moveOptionQueue, IMoveDecisionMakingResultHandler driver)
        {
            _driver = driver;
            _queueIterator = new MoveOptionQueueIterator(moveOptionQueue, this);
            _queueIterator.DequeueNextOptionItem();
        }

        public void ForceEnd()
        {
            _interactSystem.Dismiss();
        }

        public void OnOptionsQueueEmpty()
        {
            _driver.OnDecisionResult(IMoveDecisionMaking.CreateResultData(_queueIterator.MoveOptionQueue));
        }

        public void HandleTilesOption()
        {
            var values = _queueIterator.CurrentOptionItem.Values.Where(v => _queueIterator.MoveOptionQueue.Options.All(o => o.SelectedValue != v));

            _interactSystem.ShowTileSelector(values.Select(v => ((IntegerOptionValue) v).Value), tileIndex =>
            {
                _queueIterator.CurrentOptionItem.SelectedValue = _queueIterator.CurrentOptionItem.Values.FirstOrDefault(v => ((IntegerOptionValue) v).Value == tileIndex);

                _queueIterator.DequeueNextOptionItem();
            });
        }

        public void HandleDirectionsOption()
        {
            _interactSystem.ShowActionChooser((direction) =>
            {
                _queueIterator.CurrentOptionItem.SelectedValue = _queueIterator.CurrentOptionItem.Values.FirstOrDefault(v => ((BooleanOptionValue) v).Value == direction);
                _queueIterator.DequeueNextOptionItem();
            });
        }
    }
}