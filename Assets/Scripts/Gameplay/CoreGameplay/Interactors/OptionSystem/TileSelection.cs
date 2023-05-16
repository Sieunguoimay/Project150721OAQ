using Common;
using Gameplay.CoreGameplay.Interactors;
using SNM;
using System;
using System.Linq;

public class TileSelection : BaseSelection
{
    private TurnDataExtractor _turnDataExtractor;
    private BoardEntityAccess _boardEntityAccess;

    private int[] _tileIndices;
    protected override void OnSetupDependencies()
    {
        base.OnSetupDependencies();
        _turnDataExtractor = Resolver.Resolve<TurnDataExtractor>();
        _boardEntityAccess = Resolver.Resolve<BoardEntityAccess>();
    }
    public override object GetOptionDataByIndex(int selectedIndex)
    {
        if (selectedIndex == -1) return -1;
        return _tileIndices[selectedIndex];
    }

    protected override void OnStartSelection()
    {
        _tileIndices = _turnDataExtractor.ExtractedTurnData.CitizenTileEntitiesOfCurrentTurn.Where(t => t.PieceEntities.Any()).Select(t => Array.IndexOf(_boardEntityAccess.TileEntities, t)).ToArray();
        InvokeOnSelectionResult(new SimulationArgument
        {
            argumentType = SimulationArgumentType.Tile,
            selectedValue = _tileIndices.Length > 0 ? UnityEngine.Random.Range(0, _tileIndices.Length) : -1
        });
    }
}