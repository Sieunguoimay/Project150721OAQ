using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTraveller
{
    [Serializable]
    public class Config
    {
        public Color activeColor;
    }

    private readonly Config _config;
    private Tile _currentTile;
    private Color _oldColor;

    public Tile CurrentTile
    {
        get => _currentTile;
        private set
        {
            if (_currentTile != null)
            {
                var perObjectMaterial = _currentTile.gameObject.GetComponent<PerObjectMaterial>();
                perObjectMaterial.Color = _oldColor;
            }

            _currentTile = value;
            if (_currentTile != null)
            {
                var perObjectMaterial = _currentTile.gameObject.GetComponent<PerObjectMaterial>();
                _oldColor = perObjectMaterial.Color;
                perObjectMaterial.Color = _config.activeColor;
            }
        }
    }

    public int Steps { get; private set; } = 0;
    public int StepCount { get; private set; } = -1;
    public bool IsTravelling => StepCount >= 0 && StepCount < Steps;
    public Board Board { get; }

    public readonly Action OnEnd = delegate { };

    public BoardTraveller(Board board, Config config)
    {
        Board = board;
        _config = config;
    }

    public void Start(Tile startTile, int steps)
    {
        CurrentTile = startTile;
        Steps = steps;
        StepCount = 0;
    }

    public bool Next(bool forward)
    {
        if (IsTravelling)
        {
            StepCount++;
            CurrentTile = forward ? CurrentTile.Next : CurrentTile.Prev;
            if (StepCount == Steps)
            {
                OnEnd?.Invoke();
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public void Reset()
    {
        CurrentTile = null;
    }
}