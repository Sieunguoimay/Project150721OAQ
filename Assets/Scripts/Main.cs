using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Main : MonoBehaviour
{
    [SerializeField] private int playerNum = 2;
    [SerializeField] private TileSelector tileSelector;
    [SerializeField] private CitizenContainer[] citizenContainers;
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
        PositionThePlayers();
    }

    private void OnTileSelectorDone(Tile tile, bool forward)
    {
        CitizenDropper.GetReady(tile);
        CitizenDropper.DropAll(forward);
    }

    private void OnBunnieDropperDone(CitizenDropper.ActionID actionID)
    {
        if (Board.AreMandarinTilesAllEmpty())
        {
            GameOver();
        }
        else
        {
            if (actionID == CitizenDropper.ActionID.DROPPING_IN_TURN)
            {
                turn = (turn + 1) % Board.TileGroups.Count;
            }

            if (Board.IsTileGroupEmpty(turn))
            {
                if (citizenContainers[turn].Citizens.Count > 0)
                {
                    TakeBackTiles(Board.TileGroups[turn], citizenContainers[turn].Citizens);
                }
                else
                {
                    GameOver();
                }
            }
            else
            {
                tileSelector.Display(Board.TileGroups[turn]);
            }
        }
    }

    private void OnBunnieDropperEat(CitizenContainer citizenContainerMb)
    {
        citizenContainers[turn].Grasp(citizenContainerMb);
    }

    private void TakeBackTiles(Board.TileGroup tileGroup, List<Citizen> citizens)
    {
        CitizenDropper.GetReadyForTakingBackCitizens(tileGroup, citizens);
        CitizenDropper.DropAll(true);
    }

    public void GameOver()
    {
        Debug.Log("Game over");
    }

    private void PositionThePlayers()
    {
        int n = Board.TileGroups.Count;
        for (int i = 0; i < n; i++)
        {
            var location = CalculatePlayerPosition(Board.TileGroups[i]);
            citizenContainers[i].transform.position = location.Item1;
            citizenContainers[i].transform.rotation = location.Item2;
        }
    }

    private (Vector3, Quaternion) CalculatePlayerPosition(Board.TileGroup tg)
    {
        var pos1 = tg.tiles[0].transform.position;
        var pos2 = tg.tiles[tg.tiles.Count - 1].transform.position;
        var diff = pos2 - pos1;
        var pos = pos1 + new Vector3(diff.z, diff.y, -diff.x) * 0.5f;
        var qua = Quaternion.LookRotation(pos1 - pos, Vector3.up);
        return (pos, qua);
    }
}