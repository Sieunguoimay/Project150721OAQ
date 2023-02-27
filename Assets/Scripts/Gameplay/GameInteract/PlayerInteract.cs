﻿using System;
using Gameplay.Board;
using Gameplay.Player;
using Gameplay.PlayTurn;
using UnityEngine;

namespace Gameplay.GameInteract
{
    public interface IPlayerInteract
    {
        void Initialize();
        void Cleanup();
        void Show();
        event Action<PlayerInteractResult> ResultEvent;
    }

    public class PlayerInteractResult
    {
        public PlayerInteractResult(Tile selectedTile, bool direction)
        {
            SelectedTile = selectedTile;
            Direction = direction;
        }

        public Tile SelectedTile { get; }
        public bool Direction { get; }
    }

    public class PlayerInteract : BaseGenericDependencyInversionUnit<PlayerInteract>, IPlayerInteract
    {
        [SerializeField] private TileChooser tileChooser;
        [SerializeField] private ActionChooser actionChooser;
        private IActionChooser ActionChooser => actionChooser;

        public void Initialize()
        {
            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;
            tileChooser.SelectedTileChangedEvent += OnSelectedTileChanged;

            ActionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            ActionChooser.DirectionSelectedEvent += OnDirectionSelected;

            var turnTeller = Resolver.Resolve<IPlayTurnTeller>();
            tileChooser.Setup(turnTeller);
        }

        public void Cleanup()
        {
            ActionChooser.DirectionSelectedEvent -= OnDirectionSelected;
            tileChooser.SelectedTileChangedEvent -= OnSelectedTileChanged;

            tileChooser.ResetAll();
            ActionChooser.HideAway();
            tileChooser.TearDown();
        }

        public void Show()
        {
            tileChooser.ShowUp();
        }

        private void OnSelectedTileChanged()
        {
            UpdateActionChooserPosition();
            ActionChooser.ShowUp();
        }

        private void UpdateActionChooserPosition()
        {
            var tileTransform = tileChooser.SelectedTile.Transform;
            var rot = tileTransform.rotation;
            var pos = tileTransform.position + rot * Vector3.forward * tileChooser.SelectedTile.Size;

            ActionChooser.SetPositionAndRotation(pos, rot);
        }

        private void OnDirectionSelected()
        {
            var result = new PlayerInteractResult(tileChooser.SelectedTile, actionChooser.SelectedDirection);
            ResultEvent?.Invoke(result);
        }

        public event Action<PlayerInteractResult> ResultEvent;
    }
}