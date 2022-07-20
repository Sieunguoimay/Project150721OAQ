namespace Interfaces
{
    public interface ITransporter : ITransformable, IItemHolder<IPassenger>
    {
        void Loop(float deltaTime);
    }
}