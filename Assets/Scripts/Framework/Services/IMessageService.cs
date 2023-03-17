using System;

namespace Framework.Services
{
    public interface IMessageService
    {
        void Dispatch(string messageType, object sender, EventArgs eventArgs);
        void Register(string messageType, Action<object, EventArgs> messageHandler);
        void Unregister(string messageType, Action<object, EventArgs> messageHandler);
    }
}