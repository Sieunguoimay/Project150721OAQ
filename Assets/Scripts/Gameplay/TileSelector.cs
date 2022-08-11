using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using SNM;
using UnityEngine;

namespace Gameplay
{
    public class TileSelector : MonoBehaviour
    {
        [SerializeField] private MeshBoundsClicker left;
        [SerializeField] private MeshBoundsClicker right;

        private Board.TileGroup _tileGroup;
        private Tile _selectedTile;
        private Color _prevColor = Color.black;
        private readonly List<ISelectionAdaptor> _selectionAdaptors = new List<ISelectionAdaptor>();

        public Action<Tile, bool> OnDone = delegate { };
        public event Action<bool> OnTouched;

        public void Setup()
        {
            gameObject.SetActive(false);
            left.OnClick += InvokeOnTouchedLeft;
            right.OnClick += InvokeOnTouchedRight;
        }

        public void TearDown()
        {
            left.OnClick -= InvokeOnTouchedLeft;
            right.OnClick -= InvokeOnTouchedRight;
        }

        public void Display(Board.TileGroup tileGroup)
        {
            _selectedTile = null;
            _tileGroup = tileGroup;
            foreach (var t in tileGroup.Tiles)
            {
                var p = t.GetComponent<PerObjectMaterial>();
                _prevColor = p.Color;

                if (t.Pieces.Count > 0)
                {
                    p.Color = Color.black;
                }
            }
        }

        public void SelectTile(Tile tile)
        {
            if (tile.Pieces.Count <= 0) return;

            _selectedTile = tile;

            InvokeDeselect(false);

            Debug.Log("Selected tile " + tile.Pieces.Count);

            foreach (var sa in _selectedTile.Pieces.Select(p => new PieceToTileSelectorAdaptor(p)))
            {
                ((ISelectionAdaptor) sa).OnTileSelected();
                _selectionAdaptors.Add(sa);
            }

            transform.position = tile.transform.position + Vector3.up * 0.3f;
            var tiles = _tileGroup?.Tiles;
            if (tiles == null) return;
            var dir = tiles[^1].transform.position - tiles[0].transform.position;
            dir = SNM.Math.Projection(dir, Vector3.up);
            transform.rotation = Quaternion.LookRotation(dir, transform.up);

            gameObject.SetActive(true);

            foreach (var t in tiles)
            {
                t.GetComponent<PerObjectMaterial>().Color = t == _selectedTile ? Color.black : _prevColor;
            }
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
            if (_tileGroup?.Tiles != null)
            {
                foreach (var t in _tileGroup.Tiles)
                {
                    t.GetComponent<PerObjectMaterial>().Color = _prevColor;
                }
            }

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