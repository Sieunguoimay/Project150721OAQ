using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public Board Board { get; private set; }

    private BunnieDropper bunnieDropper;
    private BunnieDropper BunnieDropper => bunnieDropper ?? (bunnieDropper = GetComponent<BunnieDropper>());

    void Start()
    {
        Board = Prefab.Instantiates(PrefabManager.Instance.BoardPrefab);
        Board.Setup();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BunnieDropper.Take(Board.Tiles[Random.Range(0, Board.Tiles.Length)], Board);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            BunnieDropper.Drop();
        }
    }
}