using Common;
using SNM;
using System;

public class TileSelection : BaseSelection
{
    public override object GetSelectedData(int selectedIndex)
    {
        return UnityEngine.Random.Range(0, 5);
    }

    public override void StartSelection(SimulationArgumentSelectionController selectionController)
    {
        InvokeOnSelectionResult(new SimulationArgument { argumentType = SimulationArgumentType.Tile, selectedValue = 0 });
    }
}