using System;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    [SerializeField] private Color activeColor = Color.white;
    public event Action<Tile, bool> OnDone = delegate { };

    private Board.TileGroup? tileGroup;
    private Tile selectedTile = null;

    private Color prevColor = Color.black;

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

            prevColor = t.PerObjectMaterial.Color;

            if (t.Citizens.Count > 0)
            {
                t.PerObjectMaterial.Color = activeColor;
            }
        }
    }

    private void OnTileSelect(Tile tile)
    {
        if (tile.Citizens.Count <= 0) return;

        selectedTile = tile;
        gameObject.SetActive(true);

        foreach (var t in tileGroup?.tiles)
        {
            t.PerObjectMaterial.Color = t == selectedTile ? activeColor : prevColor;
        }
    }

    public void ChooseDirection(bool forward)
    {
        foreach (var t in tileGroup?.tiles)
        {
            t.OnSelect -= OnTileSelect;
            t.PerObjectMaterial.Color = prevColor;
        }

        tileGroup = null;

        OnDone?.Invoke(selectedTile, forward);
        gameObject.SetActive(false);
    }
}