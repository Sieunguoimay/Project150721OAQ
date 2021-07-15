using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private Tile[] tiles;
    private Tile[] Tiles => tiles ?? (tiles = GetComponentsInChildren<Tile>());

    void Start()
    {
    }

    void Update()
    {
    }

    public void Setup()
    {
    }
}