using Framework.Entities.ContainerEntity;

namespace Framework.Entities.SimpleContainer
{
    public interface ISimpleContainer : IContainerEntity<ISimpleContainerData, ISimpleContainerSavedData>
    {
    }

    public class SimpleContainer : ContainerEntity<ISimpleContainerData, ISimpleContainerSavedData>, ISimpleContainer
    {
        public SimpleContainer(ISimpleContainerData data, ISimpleContainerSavedData savedData) 
            : base(data, savedData)
        {
        }
    }
}