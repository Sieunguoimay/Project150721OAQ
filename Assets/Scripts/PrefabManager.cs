using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    [SerializeField] private Board boardPrefab;
    [SerializeField] private Tile citizenTilePrefab;
    [SerializeField] private Tile mandarinTilePrefab;
    [SerializeField] private Bunnie bunniePrefab;

    public Board BoardPrefab => boardPrefab;

    public static PrefabManager Instance { get; private set; }

    public Bunnie BunniePrefab => bunniePrefab;

    public Tile CitizenTilePrefab => citizenTilePrefab;

    public Tile MandarinTilePrefab => mandarinTilePrefab;

    private void Awake()
    {
        Instance = this;
        if (Instance == null)
        {
            Debug.LogError("Cannot create singleton");
        }
    }
}