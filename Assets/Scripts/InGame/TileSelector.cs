using System;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    private Board.TileGroup _tileGroup;
    private Tile _selectedTile;
    private Color _prevColor = Color.black;
    private readonly List<ISelectionAdaptor> _selectionAdaptors = new List<ISelectionAdaptor>();

    public Action<Tile, bool> OnDone = delegate { };

    public void Setup()
    {
        gameObject.SetActive(false);
    }

    public void Display(Board.TileGroup tileGroup)
    {
        _selectedTile = null;
        _tileGroup = tileGroup;
        foreach (var t in tileGroup.Tiles)
        {
            t.OnSelect -= OnTileSelect;
            t.OnSelect += OnTileSelect;

            var p = t.GetComponent<PerObjectMaterial>();
            _prevColor = p.Color;

            if (t.Pieces.Count > 0)
            {
                p.Color = Color.black;
            }
        }
    }

    private void OnTileSelect(Tile tile)
    {
        if (tile.Pieces.Count <= 0) return;

        _selectedTile = tile;

        InvokeDeselect();

        Debug.Log("Selected tile " + tile.Pieces.Count);
        foreach (var p in _selectedTile.Pieces)
        {
            ISelectionAdaptor sa = new PieceToTileSelectorAdaptor(p);
            sa.OnTileSelected();
            _selectionAdaptors.Add(sa);
        }

        transform.position = tile.transform.position + Vector3.up * 0.3f;
        var tiles = _tileGroup?.Tiles;
        if (tiles != null)
        {
            var dir = tiles[tiles.Count - 1].transform.position - tiles[0].transform.position;
            dir = SNM.Math.Projection(dir, Vector3.up);
            transform.rotation = Quaternion.LookRotation(dir, transform.up);

            gameObject.SetActive(true);

            foreach (var t in tiles)
            {
                t.GetComponent<PerObjectMaterial>().Color = t == _selectedTile ? Color.black : _prevColor;
            }
        }
    }

    private void InvokeDeselect()
    {
        foreach (var sa in _selectionAdaptors)
        {
            sa.OnTileDeselected();
        }

        _selectionAdaptors.Clear();
    }

    public void ChooseDirection(bool forward)
    {
        if (_tileGroup?.Tiles != null)
        {
            foreach (var t in _tileGroup.Tiles)
            {
                t.OnSelect -= OnTileSelect;
                t.GetComponent<PerObjectMaterial>().Color = _prevColor;
            }
        }

        InvokeDeselect();

        _tileGroup = null;

        OnDone?.Invoke(_selectedTile, forward);
        gameObject.SetActive(false);
    }

    public interface ISelectionAdaptor
    {
        void OnTileSelected();
        void OnTileDeselected();
    }
}