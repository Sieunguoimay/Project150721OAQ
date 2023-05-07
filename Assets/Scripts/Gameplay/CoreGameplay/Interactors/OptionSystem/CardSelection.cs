using Common;
using Framework.DependencyInversion;
using Gameplay.Cards;
using SNM;
using System;
using UnityEngine;

public interface ISelection
{
    SimulationArgumentType ArgumentType { get; }
    void StartSelection(SimulationArgumentSelectionController selectionController);
    object GetSelectedData(int selectedIndex);
    event Action<SimulationArgument> OnSelectionResult;
}
public abstract class BaseSelection : DependencyInversionScriptableObjectNode, ISelection
{
    [SerializeField] private SimulationArgumentType argumentType;

    public SimulationArgumentType ArgumentType => argumentType;

    public event Action<SimulationArgument> OnSelectionResult;

    public abstract object GetSelectedData(int selectedIndex);

    public abstract void StartSelection(SimulationArgumentSelectionController selectionController);
    protected void InvokeOnSelectionResult(SimulationArgument argument)
    {
        OnSelectionResult?.Invoke(argument);
    }
}

public class CardSelection : BaseSelection
{
    [SerializeField] private Card[] cards;

    private SimulationArgumentSelectionController _selectionController;

    public override object GetSelectedData(int selectedIndex)
    {
        return cards[selectedIndex];
    }

    public override void StartSelection(SimulationArgumentSelectionController selectionController)
    {
        _selectionController = selectionController;
        PublicExecutor.Instance.Delay(1, () =>
        {

        });
        var selected = cards.GetRandom();
        OnCardSelectionResult(selected);
    }

    private void OnCardSelectionResult(Card card)
    {
        InvokeOnSelectionResult(new SimulationArgument { argumentType = SimulationArgumentType.Card, selectedValue = Array.IndexOf(cards, card) });
    }
}