using System;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : Prefab
{
    [Serializable]
    public class Config
    {
        [SerializeField] private Color activeColor;
        public Color ActiveColor => activeColor;
    }

    private Config config;
    private Board.TileGroup? tileGroup;
    private Tile selectedTile = null;
    private Color prevColor = Color.black;
    private readonly List<ISelectionAdaptor> selectionAdaptors = new List<ISelectionAdaptor>();

    public Action<Tile, bool> OnDone = delegate { };

    public void Setup(Config config)
    {
        this.config = config;
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

            if (t.Pieces.Count > 0)
            {
                t.PerObjectMaterial.Color = config.ActiveColor;
            }
        }
    }

    private void OnTileSelect(Tile tile)
    {
        if (tile.Pieces.Count <= 0) return;

        selectedTile = tile;

        InvokeDeselect();

        Debug.Log("Selected tile " + tile.Pieces.Count);
        foreach (var p in selectedTile.Pieces)
        {
            var sa = new Piece.PieceToTileSelectorAdaptor(p);
            sa.OnTileSelected();
            selectionAdaptors.Add(sa);
        }

        transform.position = tile.transform.position + Main.Instance.GameCommonConfig.UpVector * 0.3f;
        var tiles = tileGroup?.tiles;
        if (tiles != null)
        {
            var dir = tiles[tiles.Count - 1].transform.position - tiles[0].transform.position;
            dir = SNM.Math.Projection(dir, Main.Instance.GameCommonConfig.UpVector);
            transform.rotation = Quaternion.LookRotation(dir, transform.up);
        }

        gameObject.SetActive(true);

        foreach (var t in tileGroup?.tiles)
        {
            t.PerObjectMaterial.Color = t == selectedTile ? config.ActiveColor : prevColor;
        }
    }

    private void InvokeDeselect()
    {
        foreach (var sa in selectionAdaptors)
        {
            sa.OnTileDeselected();
        }

        selectionAdaptors.Clear();
    }

    public void ChooseDirection(bool forward)
    {
        foreach (var t in tileGroup?.tiles)
        {
            t.OnSelect -= OnTileSelect;
            t.PerObjectMaterial.Color = prevColor;
        }

        InvokeDeselect();

        tileGroup = null;

        OnDone?.Invoke(selectedTile, forward);
        gameObject.SetActive(false);
    }

    public interface ISelectionAdaptor
    {
        void OnTileSelected();
        void OnTileDeselected();
    }
}