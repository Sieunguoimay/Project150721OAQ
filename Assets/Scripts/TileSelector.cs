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
            
            if (t.Bunnies.Count > 0)
            {
                t.PerObjectMaterial.Color = activeColor;
            }
        }
    }

    private void OnTileSelect(Tile tile)
    {
        if (tile.Bunnies.Count <= 0) return;
        // if (selectedTile != null)
        // {
        //     selectedTile.OnUnselected();
        // }
        
        selectedTile = tile;
        gameObject.SetActive(true);
        // selectedTile.OnSelected();
        
        foreach (var t in tileGroup?.tiles)
        {
            if (t != selectedTile)
            {
                t.PerObjectMaterial.Color = prevColor;
            }
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