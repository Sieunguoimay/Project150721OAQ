using Framework.DependencyInversion;
using Framework.Resolver;
using UnityEngine;

public class ObjectPublisher : DependencyInversionMonoBehaviour
{
    [SerializeField] private UnityEngine.Object targetObject;
    protected override void OnBind(IBinder binder)
    {
        base.OnBind(binder);
        binder.Bind(targetObject.GetType(), targetObject);
    }
    protected override void OnUnbind(IBinder binder)
    {
        base.OnUnbind(binder);
        binder.Unbind(targetObject.GetType());
    }
}