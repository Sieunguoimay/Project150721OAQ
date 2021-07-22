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
    [SerializeField] private Citizen citizenPrefab;
    [SerializeField] private Mandarin mandarinPrefab;

    public Board BoardPrefab => boardPrefab;

    public static PrefabManager Instance { get; private set; }

    public Citizen CitizenPrefab => citizenPrefab;

    public Tile CitizenTilePrefab => citizenTilePrefab;

    public Tile MandarinTilePrefab => mandarinTilePrefab;

    public Mandarin MandarinPrefab => mandarinPrefab;

    private void Awake()
    {
        Instance = this;
        if (Instance == null)
        {
            Debug.LogError("Cannot create singleton");
        }
    }
}