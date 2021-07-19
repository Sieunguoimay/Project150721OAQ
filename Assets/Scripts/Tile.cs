using System;
using UnityEngine;

public class Tile : BunnieStop
{
    [SerializeField] private Type type;
    [SerializeField] private Tile prev;
    [SerializeField] private Tile next;
    public int Id { get; private set; } = 0;

    public Tile Prev => prev;
    public Tile Next => next;
    public Type TileType => type;

    public bool IsConnected => Prev && Next;

    public event Action<Tile> OnSelect = delegate(Tile tile) { };

    private void Awake()
    {
        Id = gameObject.GetInstanceID();
    }

    public void Setup()
    {
        for (int i = 0; i < 5; i++)
        {
            Keep(Prefab.Instantiates(PrefabManager.Instance.BunniePrefab));
        }
    }

    public void Connect(Tile prev, Tile next)
    {
        this.prev = prev;
        this.next = next;
    }

    public void OnSelected()
    {
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnUnselected()
    {
        transform.localScale = Vector3.one;
    }

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