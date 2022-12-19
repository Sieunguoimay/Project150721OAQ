using System;
using Framework.Entities;
using Framework.Resolver;

namespace Gameplay.Entities.Stage
{
    public class StageEntityView : BaseEntityView<IStage, IStageData>
    {
        public void OnClicked()
        {
            Entity.Unlock();
        }
    }
}