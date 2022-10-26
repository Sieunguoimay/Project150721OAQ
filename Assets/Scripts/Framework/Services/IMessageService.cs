namespace Framework.Services
{
    public interface IMessageService
    {
        void Dispatch<TMessage, TSender>(TMessage message, TSender sender = default) where TMessage : IMessage<TSender>;

        void Register<TMessage, TSender>(IMessageHandler<TMessage, TSender> messageHandler, TSender sender = default)
            where TMessage : IMessage<TSender>;

        void Unregister<TMessage, TSender>(IMessageHandler<TMessage, TSender> messageHandler, TSender sender = default)
            where TMessage : IMessage<TSender>;
    }

    public interface IMessage<out TSender>
    {
        TSender Sender { get; }
    }

    public interface IMessageHandler<in TMessage, TSender> where TMessage : IMessage<TSender>
    {
        void OnReceiveMessage(TMessage message);
    }
}