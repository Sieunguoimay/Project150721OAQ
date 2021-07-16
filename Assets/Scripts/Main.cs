using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private DirectionSelector directionSelector;
    public Board Board { get; private set; }

    private BunnieDropper bunnieDropper;
    private BunnieDropper BunnieDropper => bunnieDropper ?? (bunnieDropper = GetComponent<BunnieDropper>());

    void Start()
    {
        Board = Prefab.Instantiates(PrefabManager.Instance.BoardPrefab);
        Board.Setup(OnTileSelected);
        directionSelector.OnDone += OnDirectionSelected;
    }

    private void OnDirectionSelected(Tile tile, bool forward)
    {
        BunnieDropper.Take(tile, Board, forward);
        BunnieDropper.Drop();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BunnieDropper.Take(Board.Tiles[Random.Range(0, Board.Tiles.Length)], Board, true);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            BunnieDropper.Drop();
        }
    }

    private void OnTileSelected(Tile tile)
    {
        if (!BunnieDropper.IsTravelling)
        {
            directionSelector.Display(tile);
        }
    }
}