using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private TileSelector tileSelector;
    [SerializeField] private CitizenContainer[] bunnieStops;
    public Board Board { get; private set; }

    private BunnieDropper bunnieDropper;
    private BunnieDropper BunnieDropper => bunnieDropper ?? (bunnieDropper = GetComponent<BunnieDropper>());

    private int turn = 0;

    void Start()
    {
        Board = Prefab.Instantiates(PrefabManager.Instance.BoardPrefab);
        Board.Setup();

        BunnieDropper.Setup(Board);

        BunnieDropper.OnDone += OnBunnieDropperDone;
        BunnieDropper.OnEat += OnBunnieDropperEat;
        tileSelector.OnDone += OnTileSelectorDone;

        tileSelector.Display(Board.TileGroups[turn]);
    }

    private void OnTileSelectorDone(Tile tile, bool forward)
    {
        BunnieDropper.GetReady(tile);
        BunnieDropper.DropAll(forward);
    }

    private void OnBunnieDropperDone()
    {
        turn = (turn + 1) % Board.TileGroups.Count;
        tileSelector.Display(Board.TileGroups[turn]);
    }

    private void OnBunnieDropperEat(CitizenContainer citizenContainerMb)
    {
        bunnieStops[turn].Grasp(citizenContainerMb.Bunnies);
    }
}