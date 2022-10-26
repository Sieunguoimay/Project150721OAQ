namespace Framework.Services
{
    public interface IMessageService
    {
        void Dispatch<TMessage>(TMessage message, object sender = null) where TMessage : IMessage;
        void Register<TMessage>(IMessageHandler<TMessage> messageHandler, object sender = null) where TMessage : IMessage;
        void Unregister<TMessage>(IMessageHandler<TMessage> messageHandler, object sender = null) where TMessage : IMessage;
    }

    public interface IMessage
    {
    }

    public interface IMessageHandler<in TMessage> where TMessage : IMessage
    {
        void OnReceiveMessage(TMessage message);
    }
}