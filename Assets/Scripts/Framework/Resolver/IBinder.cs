using System;

namespace Framework.Resolver
{
    public interface IBinder
    {
        void Bind<TType>(object target);
        void Bind<TType>(object target, string id);
        void Bind(Type type, object target);
        void Bind(Type type, string id, object target);

        void Unbind<TType>();
        void Unbind(Type type);
        void Unbind<TType>(string id);
        void Unbind(Type type, string id);
    }
}