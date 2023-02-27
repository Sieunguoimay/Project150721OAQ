using System;
using Framework.Entities;
using Framework.Entities.ContainerEntity;
using Framework.Resolver;

namespace Gameplay.Entities.Stage
{
    public interface IStage : IContainerEntity<IStageData, IStageSavedData>
    {
        int Index { get; }
        void SetIndex(int index);
    }

    public class Stage : ContainerEntity<IStageData, IStageSavedData>, IStage
    {
        public Stage(IStageData data, IStageSavedData savedData) : base(data, savedData)
        {
        }

        public void SetIndex(int index) => Index = index;
        public int Index { get; private set; }

        protected override void OnBind(IBinder binder)
        {
            base.OnBind(binder);
            binder.Bind<MatchData>(Data.MatchData);
        }

        protected override void OnUnbind(IBinder binder)
        {
            base.OnUnbind(binder);
            binder.Unbind<MatchData>();
        }
    }
}