using Gameplay.CoreGameplay.Controllers;
using Gameplay.CoreGameplay.Gateway;
using Gameplay.CoreGameplay.Interactors.Simulation;
using UnityEditor;

namespace Gameplay.CoreGameplay
{
    public static class CoreGameplayTest
    {
        [MenuItem("Test/CoreGameplayTest")]
        public static void Test()
        {
            var installController = new CoreGameplayController();
            var container = installController.Container;
           // var presenter = new SimpleInteractResultPresenter(container);
            //installController.Install(presenter, new BoardConfigDatabase(), null);

            //var view = new CoreGameplayDataView(presenter);

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

            container.BoardMoveSimulator.RunSimulation(new MoveSimulationInputData
                {StartingTileIndex = 1, Direction = true, SideIndex = 0});

            installController.Uninstall();
        }

        private class BoardConfigDatabase : ICoreGameplayDataAccess
        {
            public BoardData GetBoardData()
            {
                return new()
                {
                    NumSides = 2,
                    TilesPerSide = 5,
                    PiecesPerTile = 5
                };
            }
        }
/*
        private class CoreGameplayDataView
        {
            public CoreGameplayDataView(SimpleInteractResultPresenter presenter)
            {
                presenter.RefreshDataAvailableEvent += HandleRefreshData;
            }

            private static void HandleRefreshData(SimpleInteractResultPresenter interactResultPresenter)
            {
                var str = "Pocket:";
                foreach (var piecesInTile in interactResultPresenter.RefreshData.PiecesInPockets)
                {
                    str += " " + piecesInTile;
                }

                Debug.Log(str);

                str = "Tiles:";
                foreach (var piecesInTile in interactResultPresenter.RefreshData.PiecesInTiles)
                {
                    str += " " + piecesInTile;
                }

                Debug.Log(str);
            }
        }*/
    }
}