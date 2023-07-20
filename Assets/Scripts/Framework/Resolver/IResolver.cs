using System;

namespace Framework.Resolver
{
    public interface IResolver
    {
        object Resolve(Type type);
        object Resolve(Type type, string id);
        TType Resolve<TType>();
        TType Resolve<TType>(string id);
    }
}