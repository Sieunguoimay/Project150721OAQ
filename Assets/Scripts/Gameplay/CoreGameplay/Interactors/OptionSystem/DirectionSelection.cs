public class DirectionSelection : BaseSelection
{
    private readonly bool[] _directions = new bool[2] { true, false };
    public override object GetOptionDataByIndex(int selectedIndex)
    {
        return _directions[selectedIndex];
    }

    protected override void OnStartSelection()
    {
        InvokeOnSelectionResult(new SimulationArgument
        {
            argumentType = SimulationArgumentType.Direction,
            selectedValue = UnityEngine.Random.Range(0, 2)
        });
    }
}