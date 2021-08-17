using System;
using UnityEngine;

[RequireComponent(typeof(PerObjectMaterial))]
[DisallowMultipleComponent]
[SelectionBase]
public class Tile : PieceContainer, RayPointer.IListener
{
    [SerializeField] private Type type;
    [SerializeField] private Tile prev;
    [SerializeField] private Tile next;
    public int Id { get; private set; } = 0;

    public Tile Prev => prev;
    public Tile Next => next;
    public Type TileType => type;

    public bool IsConnected => Prev != null && Next != null;

    public event Action<Tile> OnSelect = delegate(Tile tile) { };

    public PerObjectMaterial PerObjectMaterial { get; private set; }

    private void Awake()
    {
        Id = gameObject.GetInstanceID();
    }

    public void Setup()
    {
        PerObjectMaterial = GetComponent<PerObjectMaterial>();
        Main.Instance.RayPointer.Register(this);
    }

    public void Connect(Tile prev, Tile next)
    {
        this.prev = prev;
        this.next = next;
    }

    public Tile Success(bool forward) => forward ? Next : Prev;

    // private void OnMouseDown()
    // {
    //     OnSelect?.Invoke(this);
    // }

    public enum Type
    {
        Citizen,
        Mandarin
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }

    public Bounds Bounds
    {
        get
        {
            return GetComponent<BoxCollider>().bounds;
        }
    }

    public void OnHit(Ray ray, float distance)
    {
        OnSelect?.Invoke(this);
    }
}