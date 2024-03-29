﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTraveller
{
    private Board board = null;

    private Tile currentTile = null;
    private Color activeColor;
    private Color oldColor;

    public Tile CurrentTile
    {
        get => currentTile;
        private set
        {
            if (currentTile != null)
            {
                currentTile.PerObjectMaterial.Color = oldColor;
            }

            currentTile = value;
            if (currentTile != null)
            {
                oldColor = currentTile.PerObjectMaterial.Color;
                currentTile.PerObjectMaterial.Color = activeColor;
            }
        }
    }

    public int Steps { get; private set; } = 0;
    public int StepCount { get; private set; } = -1;
    public bool IsTravelling => StepCount >= 0 && StepCount < Steps;
    public Board Board => board;

    public Action OnEnd = delegate { };

    public BoardTraveller(Board board, Color activeColor)
    {
        this.board = board;
        this.activeColor = activeColor;
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