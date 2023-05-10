using Framework.DependencyInversion;
using Gameplay.Cards;
using Gameplay.CoreGameplay.Interactors;
using Gameplay.CoreGameplay.Interactors.Simulation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimulationController : ScriptableEntity
{
    [SerializeField] private SimulationArgumentSelectionController selectionController;

    private ISimulatorFactory _simulatorFactory;
    private TurnDataExtractor _turnDataExtractor;

    protected override void OnSetupDependencies()
    {
        base.OnSetupDependencies();
        _simulatorFactory = Resolver.Resolve<ISimulatorFactory>();
        _turnDataExtractor = Resolver.Resolve<TurnDataExtractor>();
        selectionController.OnArgumentsSelectionResult -= OnSelectionResult;
        selectionController.OnArgumentsSelectionResult += OnSelectionResult;
    }

    protected override void OnTearDownDependencies()
    {
        base.OnTearDownDependencies();
        selectionController.OnArgumentsSelectionResult -= OnSelectionResult;
    }

    private void OnSelectionResult(SimulationArgumentSelectionController obj)
    {
        var arguments = selectionController.ArgumentList;
        if (arguments == null || arguments.Count <= 1) return;

        var firstArgument = arguments[0];
        if (firstArgument.argumentType == SimulationArgumentType.Card)
        {
            var cardSelection = selectionController.GetSelectionByArgumentType(SimulationArgumentType.Card);
            var cardType = (CardType)cardSelection.GetSelectedData(firstArgument.selectedValue);
            RunSimulationByCardType(arguments, cardType);
        }
        else if (firstArgument.argumentType == SimulationArgumentType.Tile)
        {
            RunBasicSimulation(arguments[0], arguments[1]);
        }
        else
        {
            Debug.LogError($"Not supported argument list {string.Join(",", arguments.Select(a => a.argumentType))}");
        }
    }

    private void RunSimulationByCardType(List<SimulationArgument> arguments, CardType cardType)
    {
        if (cardType == CardType.None || cardType == CardType.FutureForeseen)
        {
            RunBasicSimulation(arguments[1], arguments[2]);
        }
        else if (cardType == CardType.Concurrent)
        {
            RunConcurrentSimulation(arguments[1], arguments[2], arguments[3]);
        }
        else if (cardType == CardType.GoneWithTheWind)
        {
            RunGoneWithTheWindSimulation(arguments[1], arguments[2]);
        }
    }

    private void RunConcurrentSimulation(SimulationArgument arg1, SimulationArgument arg2, SimulationArgument arg3)
    {
        var tileSelection = selectionController.GetSelectionByArgumentType(SimulationArgumentType.Tile);
        var directionSelection = selectionController.GetSelectionByArgumentType(SimulationArgumentType.Direction);
        var tileIndex1 = (int)tileSelection.GetSelectedData(arg1.selectedValue);
        var tileIndex2 = (int)tileSelection.GetSelectedData(arg2.selectedValue);
        var direction = (bool)directionSelection.GetSelectedData(arg3.selectedValue);
        RunConcurrentSimulation(tileIndex1, tileIndex2, direction);
    }
    private void RunBasicSimulation(SimulationArgument arg1, SimulationArgument arg2)
    {
        var tileSelection = selectionController.GetSelectionByArgumentType(SimulationArgumentType.Tile);
        var directionSelection = selectionController.GetSelectionByArgumentType(SimulationArgumentType.Direction);
        var tileIndex = (int)tileSelection.GetSelectedData(arg1.selectedValue);
        var direction = (bool)directionSelection.GetSelectedData(arg2.selectedValue);
        RunBasicSimulation(tileIndex, direction);
    }
    private void RunGoneWithTheWindSimulation(SimulationArgument arg1, SimulationArgument arg2)
    {
        var tileSelection = selectionController.GetSelectionByArgumentType(SimulationArgumentType.Tile);
        var directionSelection = selectionController.GetSelectionByArgumentType(SimulationArgumentType.Direction);
        var tileIndex = (int)tileSelection.GetSelectedData(arg1.selectedValue);
        var direction = (bool)directionSelection.GetSelectedData(arg2.selectedValue);
        RunGoneWithTheWindSimulation(tileIndex, direction);
    }

    private void RunBasicSimulation(int tileIndex, bool direction)
    {
        var simulator = _simulatorFactory.GetSimulator(Gameplay.CoreGameplay.Interactors.MoveDecisionMaking.SimulationType.Basic);
        RunSimulation(simulator, new MoveSimulationInputData
        {
            SideIndex = _turnDataExtractor.ExtractedTurnData.CurrentTurnIndex,
            StartingTileIndex = tileIndex,
            Direction = direction,
        });
    }

    private void RunGoneWithTheWindSimulation(int tileIndex, bool direction)
    {
        var simulator = _simulatorFactory.GetSimulator(Gameplay.CoreGameplay.Interactors.MoveDecisionMaking.SimulationType.GoneWithTheWind);
        RunSimulation(simulator, new MoveSimulationInputData
        {
            SideIndex = _turnDataExtractor.ExtractedTurnData.CurrentTurnIndex,
            StartingTileIndex = tileIndex,
            Direction = direction,
        });
    }

    private void RunConcurrentSimulation(int tileIndex1, int tileIndex2, bool direction)
    {
        var simulator = _simulatorFactory.GetSimulator(Gameplay.CoreGameplay.Interactors.MoveDecisionMaking.SimulationType.Concurrent);
        RunSimulation(simulator, new MoveSimulationInputData
        {
            SideIndex = _turnDataExtractor.ExtractedTurnData.CurrentTurnIndex,
            StartingTileIndices = new[] { tileIndex1, tileIndex2 },
            Direction = direction,
        });
    }
    private void RunSimulation(IBoardMoveSimulator simulator, MoveSimulationInputData input)
    {
        simulator.RunSimulation(input);
        Debug.Log($"{simulator.GetType().Name} {input.SideIndex}, ({(input.StartingTileIndices != null ? string.Join(",", input.StartingTileIndices) : "")}) {input.StartingTileIndex} {input.Direction}");
    }
}