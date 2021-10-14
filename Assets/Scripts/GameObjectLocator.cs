using System;
using UnityEngine;

public class GameObjectLocator : MonoBehaviour
{
    public ILocatorTarget Target { get; private set; } = null;

    public GameObjectLocator SetTarget(ILocatorTarget target)
    {
        Target = target;
        return this;
    }

    public T GetTargetObject<T>()
    {
        if (Target.GetType() is T)
        {
            return (T) Target.GetData();
        }

        return default;
    }

    public interface ILocatorTarget
    {
        Type GetType();
        object GetData();
    }
}