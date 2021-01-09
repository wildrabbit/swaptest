using UnityEngine;
using Game.Board;
using System;
using System.Collections;

namespace Game.View
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] PieceType _pieceType;
        [SerializeField] Colour _colour;
        [SerializeField] GameObject _selectionOverlay;
        [SerializeField] Animator _animator;
        [SerializeField] GameObject _pieceExplosionPrefab;

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

        public void Init(Vector2Int coords, Vector3 position, bool startEnabled = true)
        {
            _coords = coords;
            transform.localPosition = position;
            gameObject.SetActive(startEnabled);
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
            gameObject.SetActive(false);
            var vfx = Instantiate(_pieceExplosionPrefab, transform.parent);
            vfx.transform.localPosition = transform.localPosition;
            yield return null;
        }

        public IEnumerator Disappear(float duration)
        {
            float time = 0.0f;
            while (time < duration)
            {
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / duration);
                yield return null;
                time += Time.deltaTime;
            }
            gameObject.SetActive(false);
        }

        public IEnumerator Appear(float duration)
        {
            gameObject.SetActive(true);
            var time = 0.0f;
            while (time < duration)
            {
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / duration);
                yield return null;
                time += Time.deltaTime;
            }

        }

        public IEnumerator Drop(Vector2Int dropCoords, Vector3 newPos, float duration)
        {
            float time = 0;
            Vector3 startPos = transform.localPosition;
            while (time < duration)
            {
                transform.localPosition = Vector3.Lerp(startPos, newPos, time / duration);
                yield return null;
                time += Time.deltaTime;
            }
            _coords = dropCoords;
        }
    }
}
