using UnityEngine;
using Board;
using System;
using System.Collections;

namespace View
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] PieceType _pieceType;
        [SerializeField] Colour _colour;
        [SerializeField] GameObject _selectionOverlay;
        [SerializeField] Animator _animator;

        public PieceType PieceType => _pieceType;
        public Colour Colour => _colour;
        public Vector2Int Coords => _coords;

        Vector2Int _coords;

        void Awake()
        {
            _selectionOverlay.SetActive(false);
        }

        public void UpdateCoords(Vector2Int coords)
        {
            _coords = coords;
        }

        public void Init(Vector2Int coords, Vector3 position)
        {
            _coords = coords;
            transform.localPosition = position;
        }

        public bool IsAdjacentTo(PieceView selectedPiece)
        {
            int deltaRow = Mathf.Abs(selectedPiece.Coords.x - _coords.x);
            int deltaCol = Mathf.Abs(selectedPiece.Coords.y - _coords.y);
            return deltaRow + deltaCol == 1;                 
        }

        public void Select()
        {
            _selectionOverlay.SetActive(true);
        }

        public void Deselect()
        {
            _selectionOverlay.SetActive(false);
        }

        public IEnumerator Explode()
        {
            // Play explode animation
            // Disable view
            // Play explode VFX
            yield return null;
        }
    }
}
