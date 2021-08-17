using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Prefab : MonoBehaviour
{
    [SerializeField] protected bool destroyOnStart = true;

    protected virtual void Start()
    {
        if (destroyOnStart)
        {
            Destroy(gameObject);
        }
    }

    public static T Instantiates<T>(T prefab) where T : Prefab
    {
        var instance = Instantiate(prefab);
        instance.destroyOnStart = false;
        return instance;
    }
}