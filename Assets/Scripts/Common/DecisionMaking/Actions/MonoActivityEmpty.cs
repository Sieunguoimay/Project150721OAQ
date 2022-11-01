namespace Common.DecisionMaking.Actions
{
    public class MonoActivityEmpty : MonoActivity
    {
        private readonly Activity _activity = new();

        public void End()
        {
            _activity.MarkAsDone();
        }

        public override Activity CreateActivity()
        {
            _activity.MarkAsDone();
            return _activity;
        }
    }
}