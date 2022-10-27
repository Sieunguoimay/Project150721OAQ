using System;

namespace Framework.Services
{
    public interface IMessageService
    {
        void Dispatch<TMessage, TSender>(TMessage message, TSender sender) where TMessage : IMessage<TSender>;

        void Register<TMessage, TSender>(Action<TMessage> messageHandler, TSender sender = default)
            where TMessage : IMessage<TSender>;

        void Unregister<TMessage, TSender>(Action<TMessage> messageHandler, TSender sender = default)
            where TMessage : IMessage<TSender>;
    }

    public interface IMessage<out TSender>
    {
        TSender Sender { get; }
    }
}