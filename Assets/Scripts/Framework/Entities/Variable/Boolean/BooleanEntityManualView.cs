using Framework.Entities;
using Framework.Resolver;

namespace Framework.Entities.Variable.Boolean
{
    public class BooleanEntityManualView : VariableEntityManualView<bool>
    {
        public void SetToTrue()
        {
            Entity.SetValue(true);
        }
    }
}