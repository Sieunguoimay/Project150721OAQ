using System;
using UnityEngine;

public class Tile : CitizenContainer
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
    }

    public void Connect(Tile prev, Tile next)
    {
        this.prev = prev;
        this.next = next;
    }

    public Tile Success(bool forward) => forward ? Next : Prev;

    private void OnMouseDown()
    {
        OnSelect?.Invoke(this);
    }

    public enum Type
    {
        Citizen,
        Mandarin
    }
}