using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTraveller
{
    private Board board;

    private Tile currentTile;

    public Tile CurrentTile
    {
        get => currentTile;
        private set
        {
            if (currentTile)
            {
                currentTile.OnUnselected();
            }

            currentTile = value;
            if (currentTile)
            {
                currentTile.OnSelected();
            }
        }
    }

    public int Steps { get; private set; } = 0;
    public bool Forward { get; private set; } = false;
    public int StepCount { get; private set; } = -1;
    public bool IsTravelling => StepCount >= 0 && StepCount < Steps;
    public Board Board => board;

    public Action OnEnd = delegate { };

    public BoardTraveller(Board board)
    {
        this.board = board;
    }

    public void Start(Tile startTile, int steps, bool forward)
    {
        CurrentTile = startTile;
        Steps = steps;
        Forward = forward;
        StepCount = 0;
    }

    public bool Next()
    {
        if (IsTravelling)
        {
            StepCount++;
            CurrentTile = Forward ? CurrentTile.Next : CurrentTile.Prev;
            if (StepCount == Steps)
            {
                OnEnd?.Invoke();
            }
        }

        return IsTravelling;
    }

    public void Reset()
    {
        CurrentTile = null;
    }
}