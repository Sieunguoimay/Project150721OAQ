using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshRenderer))]
public class MeshBoundsClicker : MonoBehaviour, RayPointer.IListener
{
    [SerializeField] private UnityEvent onClick;
    public event Action OnClick = delegate { };

    public Bounds Bounds
    {
        get { return GetComponent<MeshRenderer>().bounds; }
    }

    private void OnEnable()
    {
        Main.Instance?.RayPointer.Register(this);
        Debug.Log("MeshBoundsClicker Enable");
    }

    private void OnDisable()
    {
        Main.Instance?.RayPointer.Unregister(this);
        Debug.Log("MeshBoundsClicker Disable");
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }

    public void OnHit(Ray ray, float distance)
    {
        OnClick?.Invoke();
        onClick?.Invoke();
    }
}