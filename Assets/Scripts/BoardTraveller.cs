using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTraveller
{
    private Board board;

    public Tile CurrentTile { get; private set; } = null;
    public int Steps { get; private set; } = 0;
    public bool Forward { get; private set; } = false;
    public int StepCount { get; private set; } = -1;
    public bool Travelling => StepCount >= 0 && StepCount < Steps;
    public Board Board => board;

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
        if (Travelling)
        {
            StepCount++;
            CurrentTile = Forward ? CurrentTile.Next : CurrentTile.Prev;
        }

        return Travelling;
    }
}