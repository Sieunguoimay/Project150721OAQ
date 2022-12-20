using Common.Activity;

namespace Common.DecisionMaking.Actions
{
    public class MonoActivityEmpty : MonoActivity
    {
        private readonly Activity.Activity _activity = new();

        public void End()
        {
            _activity.End();
        }

        public override Activity.Activity CreateActivity()
        {
            return _activity;
        }
    }
}