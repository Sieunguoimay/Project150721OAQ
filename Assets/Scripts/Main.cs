using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private TileSelector tileSelector;
    [SerializeField] private CitizenContainer[] bunnieStops;
    public Board Board { get; private set; }

    private CitizenDropper _citizenDropper;
    private CitizenDropper CitizenDropper => _citizenDropper ?? (_citizenDropper = GetComponent<CitizenDropper>());

    private int turn = 0;

    void Start()
    {
        Board = Prefab.Instantiates(PrefabManager.Instance.BoardPrefab);
        Board.Setup();

        CitizenDropper.Setup(Board);

        CitizenDropper.OnDone += OnBunnieDropperDone;
        CitizenDropper.OnEat += OnBunnieDropperEat;
        tileSelector.OnDone += OnTileSelectorDone;

        tileSelector.Display(Board.TileGroups[turn]);
    }

    private void OnTileSelectorDone(Tile tile, bool forward)
    {
        CitizenDropper.GetReady(tile);
        CitizenDropper.DropAllCitizen(forward);
    }

    private void OnBunnieDropperDone()
    {
        turn = (turn + 1) % Board.TileGroups.Count;
        tileSelector.Display(Board.TileGroups[turn]);
    }

    private void OnBunnieDropperEat(CitizenContainer citizenContainerMb)
    {
        bunnieStops[turn].Grasp(citizenContainerMb);
    }
}