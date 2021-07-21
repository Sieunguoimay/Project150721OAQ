using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PrefabManager : MonoBehaviour
{
    [SerializeField] private Board boardPrefab;
    [SerializeField] private Tile citizenTilePrefab;
    [SerializeField] private Tile mandarinTilePrefab;
    [FormerlySerializedAs("bunniePrefab")] [SerializeField] private Citizen citizenPrefab;

    public Board BoardPrefab => boardPrefab;

    public static PrefabManager Instance { get; private set; }

    public Citizen CitizenPrefab => citizenPrefab;

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