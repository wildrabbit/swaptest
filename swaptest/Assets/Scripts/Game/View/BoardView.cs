﻿using Game.Board;
using Game.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;


namespace Game.View
{
    /// <summary>
    /// This class keeps track of the overall board's visuals.
    /// It handles generation of piece views and the grid, and also
    /// offers functionality to try swapping two pieces.
    /// </summary>
    public class BoardView: MonoBehaviour
    {
        [SerializeField] GridView _gridView;
        [SerializeField] float _cellWidth = 1.0f;
        [SerializeField] float _cellHeight = 1.0f;
        [SerializeField] List<PieceView> _viewPrefabs;
        [SerializeField] Transform _piecesRoot;

        [SerializeField] float _swapDuration = 0.3f;
        [SerializeField] float _swapDelayDuration = 0.1f;
        [SerializeField] float _swapRejectAnimationDuration = 0.5f;
        [SerializeField] float _explodeDelayDuration = 0.3f;
        [SerializeField] float _dropDelayDuration = 0.2f;
        [SerializeField] float _reshuffleDelayDuration = 0.4f;
        [SerializeField] AnimationCurve _swapAnimationCurve;

        public bool IsSwapping => _swapping;

        Rect _playableArea;
        int _rows;
        int _cols;
        List<PieceView> _pieceInstances = new List<PieceView>();

        float _boardOffsetX;
        float _boardOffsetY;
        WaitForSeconds _swapDelay;
        WaitForSeconds _swapRejectAnimationDelay;
        WaitForSeconds _explodeDelay;
        WaitForSeconds _dropDelay;
        WaitForSeconds _reshuffleDelay;

        ViewEvents _viewEvents;
        bool _swapping = false;

        void Start()
        {
            _swapDelay = new WaitForSeconds(_swapDelayDuration);
            _explodeDelay = new WaitForSeconds(_explodeDelayDuration);
            _dropDelay = new WaitForSeconds(_dropDelayDuration);
            _reshuffleDelay = new WaitForSeconds(_reshuffleDelayDuration);
            _swapRejectAnimationDelay = new WaitForSeconds(_swapRejectAnimationDuration);

            _viewEvents = GameEvents.Instance.View;
        }

        public void LoadView(Piece[,] pieces)
        {
            _rows = pieces.GetLength(0);
            _cols = pieces.GetLength(1);
            Cleanup();

            float halfCellHeightOffset = (1 - (_rows % 2)) * _cellHeight * 0.5f;
            float halfCellWidthOffset = (1 - (_cols % 2)) * _cellWidth * 0.5f;

            _boardOffsetY = _cellHeight * (_rows / 2) - halfCellHeightOffset;
            _boardOffsetX = _cellWidth * (_cols / 2) - halfCellWidthOffset;

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

            _gridView.Init(_rows, _cols, new Vector3(-_boardOffsetX, -_boardOffsetY, 0.0f));

            Vector3 centerPos = _piecesRoot.transform.position;
            float totalWidth = _cols * _cellWidth;
            float totalHeight = _rows * _cellHeight;
            _playableArea = new Rect(centerPos.x - 0.5f * totalWidth, centerPos.y - 0.5f * totalHeight, totalWidth, totalHeight);

            _swapping = false;
        }

        public IEnumerator ExplodePieces(IEnumerable<Vector2Int> matchingCoordinates, int chainStep)
        {
            var pieces = GetPieces(matchingCoordinates);
            foreach(var piece in pieces)
            {
                StartCoroutine(piece.Explode(_explodeDelayDuration));
            }
            yield return _explodeDelay;
            _viewEvents.DispatchPiecesExploded(pieces, chainStep);
           foreach (var piece in pieces)
            {
                Destroy(piece.gameObject);
                _pieceInstances.Remove(piece);
            }
        }

        public IEnumerator Drop(HashSet<Vector2Int> droppingPieces)
        {
            var pieces = GetPieces(droppingPieces);
            foreach(var piece in pieces)
            {
                Vector2Int dropCoords = piece.Coords;
                dropCoords.x = dropCoords.x - 1;
                TryConvertCoordsToBoardPos(dropCoords, out var pos);
                StartCoroutine(piece.Drop(dropCoords, pos, _dropDelayDuration));
            }          
            yield return _dropDelay;
        }

        public IEnumerator Reshuffle(Piece[,] swaps)
        {
            foreach (var piece in _pieceInstances)
            {
                StartCoroutine(piece.Disappear(_reshuffleDelayDuration * 0.5f));
            }
            _viewEvents.DispatchReshuffling();
            yield return _reshuffleDelay;
            LoadView(swaps);
            foreach (var piece in _pieceInstances)
            {
                StartCoroutine(piece.Appear(_reshuffleDelayDuration * 0.5f));
            }
        }

        public IEnumerator Refill(List<(Piece, Vector2Int)> newPieces)
        {
            var generatedPieces = GeneratePieceList(newPieces);
            foreach (var piece in generatedPieces)
            {
                StartCoroutine(piece.Appear(_reshuffleDelayDuration * 0.5f));
            }
            yield return null;
        }

        private List<PieceView> GeneratePieceList(List<(Piece, Vector2Int)> newPieces)
        {
            var generatedPieces = new List<PieceView>();
            foreach(var generationData in newPieces)
            {
                (Piece pieceData, (int row, int col)) = (generationData.Item1, (generationData.Item2.x, generationData.Item2.y));

                // TODO: If performance becomes an issue, consider using pools to replace instantiations
                PieceView instance = Instantiate(GetPrefabForPiece(pieceData), _piecesRoot);
                if (instance != null)
                {
                    Vector3 position = new Vector3
                    {
                        x = col * _cellWidth - _boardOffsetX,
                        y = row * _cellHeight - _boardOffsetY,
                        z = 0.0f
                    };
                    instance.Init(generationData.Item2, position, startEnabled:false);
                    _pieceInstances.Add(instance);
                    generatedPieces.Add(instance);
                }
            }
            return generatedPieces;
        }

        public List<PieceView> GetPieces(IEnumerable<Vector2Int> pieceCoords)
        {
            List<PieceView> pieces = new List<PieceView>();
            foreach (var coords in pieceCoords)
            {
                TryGetPieceView(coords, out var pieceView);
                if(pieceView != null)
                {
                    pieces.Add(pieceView);
                }
            }
            return pieces;
        }

        public void PrepareBoardUpdate()
        {
            _viewEvents.DispatchBoardUpdateStarted();
        }

        public void CompleteBoardUpdate()
        {
            _viewEvents.DispatchBoardUpdateCompleted();
        }

        public void AttemptSwap(PieceView selectedPiece, PieceView swapCandidatePiece)
        {
            _swapping = true;
            _viewEvents.DispatchSwapAttemptStarted();
            StartCoroutine(TrySwapPieces(selectedPiece, swapCandidatePiece));
        }

        private IEnumerator TrySwapPieces(PieceView selectedPiece, PieceView swapCandidatePiece)
        {
            TryConvertCoordsToBoardPos(selectedPiece.Coords, out var selectedPos);
            TryConvertCoordsToBoardPos(swapCandidatePiece.Coords, out var candidatePos);

            Action<float> updatePieces = (easeValue) =>
            {
                selectedPiece.transform.localPosition = Vector3.Lerp(selectedPos, candidatePos, easeValue);
                swapCandidatePiece.transform.localPosition = Vector3.Lerp(candidatePos, selectedPos, easeValue);
            };
            yield return AnimationRoutineUtils.AnimateFloatWithEaseCurve(_swapDuration, _swapAnimationCurve, updatePieces);
            yield return _swapDelay;

            _viewEvents.DispatchSwapAnimationCompleted(selectedPiece.Coords, swapCandidatePiece.Coords);
        }

        public void ConfirmSwapAttempt(Vector2Int selected, Vector2Int candidate)
        {
            TryGetPieceView(selected, out var selectedPiece);
            TryGetPieceView(candidate, out var targetPiece);
            if(selectedPiece != null)
            {
                selectedPiece.UpdateCoords(candidate);
            }
            if(targetPiece != null)
            {
                targetPiece.UpdateCoords(selected);
            }
            _swapping = false;
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

            selectedPiece.PlayInvalidSwapAnimation();
            candidatePiece.PlayInvalidSwapAnimation();
            yield return _swapRejectAnimationDelay;
            Action<float> updatePieces = (easeValue) =>
            {
                selectedPiece.transform.localPosition = Vector3.Lerp(candidatePos, selectedPos, easeValue);
                candidatePiece.transform.localPosition = Vector3.Lerp(selectedPos, candidatePos, easeValue);
            };
            yield return AnimationRoutineUtils.AnimateFloatWithEaseCurve(_swapDuration, _swapAnimationCurve, updatePieces);
            _viewEvents.DispatchFailedSwapAttempt();
            _swapping = false;
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
                Destroy(piece.gameObject);
            }
            _pieceInstances.Clear();
        }
    }
}
