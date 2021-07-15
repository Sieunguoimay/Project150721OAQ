using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    [SerializeField] private Board boardPrefab;

    public Board BoardPrefab => boardPrefab;

    public static PrefabManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        if (Instance == null)
        {
            Debug.LogError("Cannot create singleton");
        }
    }
}
