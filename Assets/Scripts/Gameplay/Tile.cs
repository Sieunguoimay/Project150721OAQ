using System;
using UnityEngine;

[RequireComponent(typeof(PerObjectMaterial))]
// [DisallowMultipleComponent]
[SelectionBase]
public class Tile : PieceContainer, RayPointer.IListener
{
    [SerializeField] private Tile prev;
    [SerializeField] private Tile next;
    public int Id { get; private set; } = 0;

    public Tile Prev => prev;
    public Tile Next => next;

    public bool IsConnected => Prev != null && Next != null;

    public event Action<Tile> OnSelect = delegate(Tile tile) { };

    private void Awake()
    {
        Id = gameObject.GetInstanceID();
    }

    public override void Setup()
    {
        base.Setup();
        Main.Instance.RayPointer.Register(this);
    }

    public void Connect(Tile prev, Tile next)
    {
        this.prev = prev;
        this.next = next;
    }

    public Tile Success(bool forward) => forward ? Next : Prev;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }

    public Bounds Bounds => GetComponent<BoxCollider>().bounds;

    public void OnHit(Ray ray, float distance)
    {
        OnSelect?.Invoke(this);
    }
}