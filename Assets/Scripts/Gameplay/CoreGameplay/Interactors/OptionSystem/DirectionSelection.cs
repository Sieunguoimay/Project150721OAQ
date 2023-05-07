using Common;
using SNM;

public class DirectionSelection : BaseSelection
{
    public override object GetSelectedData(int selectedIndex)
    {
        return UnityEngine.Random.Range(0, 5);
    }

    public override void StartSelection(SimulationArgumentSelectionController selectionController)
    {
        InvokeOnSelectionResult(new SimulationArgument { argumentType = SimulationArgumentType.Direction, selectedValue = 0 });
    }
}