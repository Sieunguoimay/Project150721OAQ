using System;

namespace Framework.Entities.Variable
{
    public class VariableEntityManualView<TPrimitive> : BaseEntityManualView<IVariable<TPrimitive>>
    {
        protected override void OnSetup()
        {
            base.OnSetup();
            Entity.ValueChanged -= OnValueChanged;
            Entity.ValueChanged += OnValueChanged;
        }

        protected override void OnTearDown()
        {
            base.OnTearDown();
            if (Entity != null)
            {
                Entity.ValueChanged -= OnValueChanged;
            }
        }

        protected virtual void OnValueChanged(object arg1, EventArgs arg2)
        {
        }
    }
}