using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "PrefabManager")]
public class PrefabManager : ScriptableObject
{
    [SerializeField] private Board boardPrefab;
    [SerializeField] private Tile citizenTilePrefab;
    [SerializeField] private Tile mandarinTilePrefab;
    [SerializeField] private Citizen citizenPrefab;
    [SerializeField] private Mandarin mandarinPrefab;
    [SerializeField] private TileSelector tileSelector;

    public Board BoardPrefab => boardPrefab;
    
    public Citizen CitizenPrefab => citizenPrefab;

    public Tile CitizenTilePrefab => citizenTilePrefab;

    public Tile MandarinTilePrefab => mandarinTilePrefab;

    public Mandarin MandarinPrefab => mandarinPrefab;

    public TileSelector TileSelector => tileSelector;

}