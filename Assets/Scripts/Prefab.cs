using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Prefab : MonoBehaviour
{
    protected bool isPrefab = true;

    protected virtual void Start()
    {
        if (isPrefab)
        {
            Destroy(gameObject);
        }
    }

    public static T Instantiates<T>(T prefab) where T : Prefab
    {
        var instance = Instantiate(prefab);
        instance.isPrefab = false;
        return instance;
    }
}