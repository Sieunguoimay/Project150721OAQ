using System;
using UnityEngine;

public class Tile : BunnieStop
{
    [SerializeField] private Tile prev;
    [SerializeField] private Tile next;
    public int Id { get; private set; } = 0;

    public Tile Prev => prev;
    public Tile Next => next;

    public bool IsConnected => Prev && Next;

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
}