namespace Common.DecisionMaking.Actions
{
    public class MonoActivityEmpty : MonoActivity
    {
        private readonly Activity _activity = new();

        public void End()
        {
            _activity.NotifyDone();
        }

        public override Activity CreateActivity()
        {
            return _activity;
        }
    }
}