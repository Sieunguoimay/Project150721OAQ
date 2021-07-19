using System;
using UnityEngine;

public class DirectionSelector : MonoBehaviour
{
    public event Action<Tile, bool> OnDone = delegate { };

    private Tile tile;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Display(Tile tile)
    {
        if (this.tile != null)
        {
            this.tile.OnUnselected();
        }

        this.tile = tile;
        gameObject.SetActive(true);
        this.tile.OnSelected();
    }

    public void ChooseDirection(bool forward)
    {
        tile.OnUnselected();
        OnDone?.Invoke(tile, forward);
        gameObject.SetActive(false);
    }
}