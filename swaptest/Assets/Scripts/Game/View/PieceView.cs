using Game.Board;
using Game.Utils;
using System;
using System.Collections;
using UnityEngine;

namespace Game.View
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] PieceType _pieceType;
        [SerializeField] PieceColour _colour;
        [SerializeField] GameObject _selectionOverlay;
        [SerializeField] Animator _animator;
        [SerializeField] GameObject _pieceExplosionPrefab;

        public PieceType PieceType => _pieceType;
        public PieceColour Colour => _colour;
        public Vector2Int Coords => _coords;

        AnimationCurve _linearEaseCurve;

        Vector2Int _coords;

        int _idleHash;
        int _happyHash;
        int _happierHash;
        int _spawnHash;
        int _nayHash;

        void Awake()
        {
            HashAnimationParameters();            
            _selectionOverlay.SetActive(false);
            _linearEaseCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        }

        void HashAnimationParameters()
        {
            _idleHash = Animator.StringToHash("idle");
            _happyHash = Animator.StringToHash("happy");
            _happierHash = Animator.StringToHash("happier");
            _nayHash = Animator.StringToHash("nay");
            _spawnHash = Animator.StringToHash("spawn");
        }

        public void UpdateCoords(Vector2Int coords)
        {
            _coords = coords;
            name = $"Piece {_coords}";
        }

        public void Init(Vector2Int coords, Vector3 position, bool startEnabled = true)
        {
            _coords = coords;
            transform.localPosition = position;
            gameObject.SetActive(startEnabled);
            name = $"Piece {_coords}";
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
            PlayHappy();
        }

        public void Deselect()
        {
            _selectionOverlay.SetActive(false);
            PlayIdle();
        }

        public IEnumerator Explode(float animationHoldDuration)
        {
            PlayHappy();
            yield return new WaitForSeconds(animationHoldDuration);
            gameObject.SetActive(false);
            var vfx = Instantiate(_pieceExplosionPrefab, transform.parent);
            vfx.transform.localPosition = transform.localPosition;
            yield return null;
        }

        public IEnumerator Disappear(float duration)
        {
            Action<Vector3> scaleFunc = (lerpVector) => transform.localScale = lerpVector;
            yield return AnimationRoutineUtils.LerpVectorWithEaseCurve(Vector3.one, Vector3.zero, duration, _linearEaseCurve, scaleFunc);
            gameObject.SetActive(false);
        }

        public IEnumerator Appear(float duration)
        {
            gameObject.SetActive(true);
            PlaySpawn();
            yield return new WaitForSeconds(duration);
        }

        public IEnumerator Drop(Vector2Int dropCoords, Vector3 newPos, float duration)
        {
            Vector3 startPos = transform.localPosition;
            Action<Vector3> positionFunc = (lerpVector) => transform.localPosition = lerpVector;
            yield return AnimationRoutineUtils.LerpVectorWithEaseCurve(startPos, newPos, duration, _linearEaseCurve, positionFunc);
            _coords = dropCoords;
        }

        public void PlayHappy()
        {
            _animator.SetTrigger(_happyHash);
        }

        public void PlayNay()
        {
            _animator.SetTrigger(_nayHash);
        }

        void PlayIdle()
        {
            _animator.SetTrigger(_idleHash);
        }

        private void PlaySpawn()
        {
            _animator.SetTrigger(_spawnHash);
        }
    }
}
