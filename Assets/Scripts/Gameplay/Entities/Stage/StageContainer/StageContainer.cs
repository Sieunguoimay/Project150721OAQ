using Framework.Entities.ContainerEntity;

namespace Gameplay.Entities.Stage.StageContainer
{
    public interface IStageContainer : IContainerEntity<IStageContainerData, IStageContainerSavedData>
    {
    }

    public class StageContainer : ContainerEntity<IStageContainerData, IStageContainerSavedData>, IStageContainer
    {
        public StageContainer(IStageContainerData data, IStageContainerSavedData savedData) 
            : base(data, savedData)
        {
        }

        protected override void OnSetupDependencies()
        {
            base.OnSetupDependencies();
            var index = 0;
            foreach (var child in Children)
            {
                if (child is IStage stage)
                {
                    stage.SetIndex(index++);
                }
            }
        }
    }
}