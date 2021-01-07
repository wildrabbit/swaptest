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
            // Disable view
            // Play explode VFX
            yield return null;
        }

        public IEnumerator Drop1()
        {
            yield return null;
        }

        public IEnumerator SpawnDrop1()
        {
            gameObject.SetActive(true);
            yield return null;
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

        public IEnumerator Shuffle(Vector2Int newCoords, Vector3 newPos, float duration)
        {
            float time = 0.0f;
            float halvedDuration = duration * 0.5f;
            while (time < halvedDuration)
            {
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / halvedDuration);
                yield return null;
                time += Time.deltaTime;
            }
            transform.localPosition = newPos;
            time = 0.0f;
            while (time < halvedDuration)
            {
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / halvedDuration);
                yield return null;
                time += Time.deltaTime;
            }
            _coords = newCoords;
        }
    }
}
