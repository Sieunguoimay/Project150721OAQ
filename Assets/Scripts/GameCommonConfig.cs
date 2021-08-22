using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/GameCommonConfig")]
public class GameCommonConfig : ScriptableObject
{
    [SerializeField] private Main.Config main;
    [SerializeField] private TileSelector.Config tileSelector;
    [SerializeField] private PieceDropper.Config pieceDropper;
    [SerializeField] private Vector3 upVector = Vector3.up;
    [SerializeField] private Piece.ConfigData citizenConfigData;
    [SerializeField] private Piece.ConfigData mandarinConfigData;
    public PieceDropper.Config PieceDropper => pieceDropper;
    public TileSelector.Config TileSelector => tileSelector;
    public Main.Config Main => main;
    public Vector3 UpVector => upVector;
    public Piece.ConfigData CitizenConfigData => citizenConfigData;
    public Piece.ConfigData MandarinConfigData => mandarinConfigData;

    public const int PieceNumPerTile = 5;
}