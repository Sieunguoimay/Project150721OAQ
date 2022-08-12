using System;
using System.Collections.Generic;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay.Board
{
    public class TileSelector : MonoBehaviour
    {
        [SerializeField] private MeshBoundsClicker left;
        [SerializeField] private MeshBoundsClicker right;

        private Gameplay.Board.Board.TileGroup _tileGroup;
        private Tile _selectedTile;
        private readonly List<ISelectionAdaptor> _selectionAdaptors = new List<ISelectionAdaptor>();

        public Action<Tile, bool> OnDone = delegate { };
        public event Action<bool> OnTouched;

        private void Awake()
        {
            Resolver.Instance.Bind(this);
            gameObject.SetActive(false);
            left.OnClick += InvokeOnTouchedLeft;
            right.OnClick += InvokeOnTouchedRight;
        }

        private void OnDestroy()
        {
            Resolver.Instance.Unbind(this);

            left.OnClick -= InvokeOnTouchedLeft;
            right.OnClick -= InvokeOnTouchedRight;
        }

        public void Display(Gameplay.Board.Board.TileGroup tileGroup)
        {
            _selectedTile = null;
            _tileGroup = tileGroup;
        }

        public void SelectTile(Tile tile)
        {
            if (tile.Pieces.Count <= 0) return;

            _selectedTile = tile;

            InvokeDeselect(false);

            foreach (var sa in _selectedTile.Pieces.Where(p => p is Citizen).Select(p => new CitizenToTileSelectorAdaptor(p as Citizen)))
            {
                ((ISelectionAdaptor) sa).OnTileSelected();
                _selectionAdaptors.Add(sa);
            }

            transform.position = tile.transform.position + Vector3.up * 0.3f;
            var tiles = _tileGroup?.Tiles;
            if (tiles == null) return;
            var dir = ((Tile) tiles[^1]).transform.position - ((Tile) tiles[0]).transform.position;
            dir = SNM.Math.Projection(dir, Vector3.up);
            transform.rotation = Quaternion.LookRotation(dir, transform.up);

            gameObject.SetActive(true);
        }

        private void InvokeDeselect(bool success)
        {
            foreach (var sa in _selectionAdaptors)
            {
                sa.OnTileDeselected(success);
            }

            _selectionAdaptors.Clear();
        }

        public void ChooseDirection(bool forward)
        {
            InvokeDeselect(true);

            _tileGroup = null;

            OnDone?.Invoke(_selectedTile, forward);
            gameObject.SetActive(false);
        }

        private void InvokeOnTouchedRight()
        {
            OnTouched?.Invoke(true);
        }

        private void InvokeOnTouchedLeft()
        {
            OnTouched?.Invoke(false);
        }

        public interface ISelectionAdaptor
        {
            void OnTileSelected();
            void OnTileDeselected(bool success);
        }
    }
}