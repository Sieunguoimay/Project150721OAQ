using System;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    public event Action<Tile, bool> OnDone = delegate { };

    private Board.TileGroup? tileGroup;
    private Tile selectedTile = null;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Display(Board.TileGroup tileGroup)
    {
        selectedTile = null;
        this.tileGroup = tileGroup;
        foreach (var t in tileGroup.tiles)
        {
            t.OnSelect -= OnTileSelect;
            t.OnSelect += OnTileSelect;
        }
    }

    private void OnTileSelect(Tile tile)
    {
        if (selectedTile != null)
        {
            selectedTile.OnUnselected();
        }

        selectedTile = tile;
        gameObject.SetActive(true);
        selectedTile.OnSelected();
    }

    public void ChooseDirection(bool forward)
    {
        foreach (var t in tileGroup?.tiles)
        {
            t.OnSelect -= OnTileSelect;
        }

        tileGroup = null;

        selectedTile.OnUnselected();
        OnDone?.Invoke(selectedTile, forward);
        gameObject.SetActive(false);
    }
}