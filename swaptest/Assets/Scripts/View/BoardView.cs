using System;
using System.Collections;
using System.Collections.Generic;
using Board;
using UnityEngine;

namespace View
{
    public class BoardView: MonoBehaviour
    {
        [SerializeField] float _cellWidth = 1.0f;
        [SerializeField] float _cellHeight = 1.0f;
        [SerializeField] List<PieceView> _viewPrefabs;
        [SerializeField] Transform _piecesRoot;

        [SerializeField] float _swapDuration = 0.3f;
        [SerializeField] float _swapDelayDuration = 0.1f;
        [SerializeField] AnimationCurve _swapAnimationCurve;

        Rect _playableArea;
        int _rows;
        int _cols;
        List<PieceView> _pieceInstances = new List<PieceView>();

        float _boardOffsetX;
        float _boardOffsetY;
        WaitForSeconds _swapDelay;

        public event Action SwapAttemptStarted;
        public event Action<Vector2Int, Vector2Int> SwapAnimationCompleted;
        public event Action FailedSwapAttempt;
        public event Action BoardUpdateStarted;
        public event Action BoardUpdateCompleted;

        void Awake()
        {
            _swapDelay = new WaitForSeconds(_swapDelayDuration);
        }

        public void LoadView(Piece[,] pieces)
        {
            _rows = pieces.GetLength(0);
            _cols = pieces.GetLength(1);
            Cleanup();

            _boardOffsetY = _cellHeight * (_rows / 2) - (1 - (_rows % 2)) * _cellHeight * 0.5f;
            _boardOffsetX = _cellWidth * (_cols / 2) - (1 - (_cols % 2)) * _cellWidth* 0.5f;

            for (int i = 0; i < _rows; ++i)
            {
                for(int j = 0; j < _cols; ++j)
                {
                    PieceView instance = Instantiate(GetPrefabForPiece(pieces[i, j]), _piecesRoot);
                    if(instance != null)
                    {
                        Vector3 position = new Vector3
                        {
                            x = j * _cellWidth - _boardOffsetX,
                            y = i * _cellHeight - _boardOffsetY,
                            z = 0.0f
                        };
                        instance.Init(new Vector2Int(i, j), position);
                        _pieceInstances.Add(instance);
                    }
                }
            }

            Vector3 centerPos = _piecesRoot.transform.position;
            float totalWidth = _cols * _cellWidth;
            float totalHeight = _rows * _cellHeight;
            _playableArea = new Rect(centerPos.x - 0.5f * totalWidth, centerPos.y - 0.5f * totalHeight, totalWidth, totalHeight);
        }

        public void AttemptSwap(PieceView selectedPiece, PieceView swapCandidatePiece)
        {
            Debug.Log($"Attempting swap between @ {selectedPiece.Coords} and {swapCandidatePiece.Coords} ");
            SwapAttemptStarted?.Invoke();
            StartCoroutine(TrySwapPieces(selectedPiece, swapCandidatePiece));
        }

        private IEnumerator TrySwapPieces(PieceView selectedPiece, PieceView swapCandidatePiece)
        {
            TryConvertCoordsToBoardPos(selectedPiece.Coords, out var selectedPos);
            TryConvertCoordsToBoardPos(swapCandidatePiece.Coords, out var candidatePos);

            float duration = 0;
            float t = 0;
            while (duration < _swapDuration)
            {
                t = _swapAnimationCurve.Evaluate(duration / _swapDuration);
                selectedPiece.transform.localPosition = Vector3.Lerp(selectedPos, candidatePos, t);
                swapCandidatePiece.transform.localPosition = Vector3.Lerp(candidatePos, selectedPos, t);
                yield return null;
                duration += Time.deltaTime;
            }
            yield return _swapDelay;

            SwapAnimationCompleted?.Invoke(selectedPiece.Coords, swapCandidatePiece.Coords);
        }

        public void ConfirmSwapAttempt(Vector2Int selected, Vector2Int candidate)
        {

        }

        public void OnFailedSwapAttempt(Vector2Int selected, Vector2Int candidate)
        {
            StartCoroutine(RestoreSwapRoutine(selected, candidate));
        }

        IEnumerator RestoreSwapRoutine(Vector2Int selected, Vector2Int candidate)
        {
            TryGetPieceView(selected, out var selectedPiece);
            TryGetPieceView(candidate, out var candidatePiece);
            TryConvertCoordsToBoardPos(selected, out var selectedPos);
            TryConvertCoordsToBoardPos(candidate, out var candidatePos);

            float duration = 0;
            float t = 0;
            while (duration < _swapDuration)
            {
                t = _swapAnimationCurve.Evaluate(duration / _swapDuration);
                candidatePiece.transform.localPosition = Vector3.Lerp(candidatePos, selectedPos, t);
                selectedPiece.transform.localPosition = Vector3.Lerp(selectedPos, candidatePos, t);
                yield return null;
                duration += Time.deltaTime;
            }
            FailedSwapAttempt?.Invoke();
        }

        public bool PositionInsideBounds(Vector3 pos)
        {
            return _playableArea.Contains(pos);
        }

        public bool TryGetPieceView(Vector3 pos, out PieceView piece)
        {
            piece = null;
            if (TryConvertWorldToCoords(pos, out var coords))
            {
                return TryGetPieceView(coords, out piece);
            }
            return false;
        }

        public bool TryGetPieceView(Vector2Int coords, out PieceView piece)
        {
            piece = _pieceInstances.Find(pieceView => pieceView.Coords == coords);
            return piece != null;
        }

        public bool TryConvertWorldToCoords(Vector3 pos, out Vector2Int coords)
        {
            coords = new Vector2Int();
            if (PositionInsideBounds(pos))
            {
                int columnIdx = (int)((pos.x - _playableArea.x) / _cellWidth);
                int rowIdx = (int)((pos.y - _playableArea.y) / _cellHeight);
                coords.Set(rowIdx, columnIdx); 
                return true;
            }
            return false;
        }

        public bool TryConvertCoordsToBoardPos(Vector2Int coords, out Vector3 worldPos)
        {
            (int row, int col) = (coords.x, coords.y);
            worldPos = Vector3.zero;
            if (row < 0 || row >= _rows || col < 0 || col >= _cols)
            {
                return false;
            }
            worldPos.Set(col * _cellWidth - _boardOffsetX, row * _cellHeight - _boardOffsetY, 0);
            return true;
        }

        PieceView GetPrefabForPiece(Piece piece)
        {
            var mapping = _viewPrefabs.Find(pieceViewData => pieceViewData.PieceType == piece.PieceType && pieceViewData.Colour == piece.Colour);
            Debug.Assert(mapping != null, "Piece mapping not found!");
            return mapping;
        }

        void Cleanup()
        {
            foreach(var piece in _pieceInstances)
            {
                Destroy(piece);
            }
            _pieceInstances.Clear();
        }
    }
}
