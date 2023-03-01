using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Interactors.Simulation;
using Gameplay.CoreGameplay.Presenters;
using UnityEditor;
using UnityEngine;

namespace Gameplay.CoreGameplay
{
    public static class CoreGameplayTest
    {
        [MenuItem("Test/CoreGameplayTest")]
        public static void Test()
        {
            var installController = new CoreGameplayInstallController();
            installController.Install();

            var container = installController.Container;

            var presenter = installController.RefreshResultPresenter;
            var view = new CoreGameplayDataView(presenter);

            // container.RefreshRequester.Refresh();

            // var moveData = new PieceInteractData.PieceMoveData
            // {
            //     CurrentTileIndex = 5 % presenter.RefreshData.PiecesInTiles.Length,
            //     TargetTileIndex = 6 % presenter.RefreshData.PiecesInTiles.Length,
            // };
            // container.PiecesInteractor.MovePieceToNewTile(moveData);
            // container.PiecesInteractor.MovePieceToNewTile(moveData);
            //
            // moveData = new PieceInteractData.PieceMoveData
            // {
            //     CurrentTileIndex = 1 % presenter.RefreshData.PiecesInTiles.Length,
            //     TargetTileIndex = 3 % presenter.RefreshData.PiecesInTiles.Length,
            // };
            // container.PiecesInteractor.MovePieceToNewTile(moveData);
            //
            // container.PiecesInteractor.MovePiecesToPocket(new PieceInteractData.PieceMoveToPocketData
            // {
            //     CurrentTileIndex = 3,
            //     TargetPocketIndex = 1
            // });
            
            container.BoardMoveSimulator.RunSimulation(new MoveSimulationInputData {StartingTileIndex = 1, Direction = true, SideIndex = 0});

            installController.Uninstall();
        }

        private class CoreGameplayDataView
        {
            public CoreGameplayDataView(RefreshResultPresenter presenter)
            {
                presenter.RefreshDataAvailableEvent += HandleRefreshData;
            }

            private static void HandleRefreshData(RefreshResultPresenter refreshResultPresenter)
            {
                var str = "Pocket:";
                foreach (var piecesInTile in refreshResultPresenter.RefreshData.PiecesInPockets)
                {
                    str += " " + piecesInTile;
                }

                Debug.Log(str);
                
                str = "Tiles:";
                foreach (var piecesInTile in refreshResultPresenter.RefreshData.PiecesInTiles)
                {
                    str += " " + piecesInTile;
                }

                Debug.Log(str);
            }
        }
    }
}