using Game.Levels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.View;
using URandom = UnityEngine.Random;
using Game.Events;

namespace Game.Board
{
    public class BoardController : MonoBehaviour
    {
        enum BoardUpdatePhase
        {
            Stable,
            Matching,
            Exploding,
            Gravity,
            Reshuffling
        }

        [SerializeField] int _maxReshuffles;
        [SerializeField] BoardView _view;

        int _cols;
        int _rows;
        Piece[,] _pieces;
        string _lastSeed;
        BoardUpdatePhase _currentPhase;
        private int kMaxReshuffles;

        public int Cols => _cols;
        public int Rows => _rows;
        public BoardView View => _view;
        public bool IsStable => _currentPhase == BoardUpdatePhase.Stable;

        ViewEvents _viewEvents;
        BoardEvents _boardEvents;

        void Awake()
        {
            _viewEvents = GameEvents.Instance.View;
            _viewEvents.SwapAnimationCompleted += OnSwapAnimationCompleted;
            _boardEvents = GameEvents.Instance.Board;
        }

        void OnDestroy()
        {
            _viewEvents = GameEvents.Instance.View;
            _viewEvents.SwapAnimationCompleted -= OnSwapAnimationCompleted;
        }

        public void StartNewGame(BaseLevelData level)
        {
            InitPieces(level);
            LoadView();
            _currentPhase = BoardUpdatePhase.Stable;
        }

        void LoadView()
        {
            _view.LoadView(_pieces);            
        }

        void OnSwapAnimationCompleted(Vector2Int selectedCoords, Vector2Int targetCoords)
        {
            if(CanSwapPiecesAtCoords(selectedCoords, targetCoords))
            {
                SwapPiecesAtCoords(selectedCoords, targetCoords);
                BeginBoardUpdatePhase();
            }
            else
            {
                _view.OnFailedSwapAttempt(selectedCoords, targetCoords);
            }
        }

        public void BeginBoardUpdatePhase()
        {
            StartCoroutine(BoardUpdateRoutine());
        }

        IEnumerator BoardUpdateRoutine()
        {
            _view.PrepareBoardUpdate();
            int chainStep = 0;

            List<MatchInfo> matches = MatchFinder.FindMatches(_pieces);
            do
            {
                _currentPhase = BoardUpdatePhase.Matching;
                while (matches.Count > 0)
                {
                    _boardEvents.DispatchMatchesFound(matches, chainStep);
                    yield return ClearPieces(GetExplodingPieces(matches));
                    yield return ApplyGravityAndRegenerate();
                    matches = MatchFinder.FindMatches(_pieces);
                    chainStep++;
                }
                _boardEvents.DispatchDropStepCompleted();
                int possibleMatches = CountCoordsWithMatches();
                int reshuffleCount = 0;
                while (possibleMatches == 0 && reshuffleCount < _maxReshuffles)
                {
                    _currentPhase = BoardUpdatePhase.Reshuffling;
                    yield return Reshuffle();
                    possibleMatches = CountCoordsWithMatches();
                    reshuffleCount++;
                }
                yield return null;

                matches = MatchFinder.FindMatches(_pieces);
                if (matches.Count == 0)
                {
                    _currentPhase = BoardUpdatePhase.Stable;
                }
            } while(_currentPhase != BoardUpdatePhase.Stable);            
            _view.CompleteBoardUpdate();
        }

        private IEnumerator Reshuffle()
        {
            int totalPieces = _pieces.Length;

            for (int idx = totalPieces - 1; idx >= 0; idx--)
            {
                int swapIdx = URandom.Range(0, idx + 1);
                int srcRow = idx / _rows;
                int srcCol = idx % _rows;
                int swapRow = swapIdx / _rows;
                int swapCol = swapIdx % _rows;

                Piece srcPiece = _pieces[srcRow, srcCol];
                Piece swapPiece = _pieces[swapRow, swapCol];
                PieceType auxType = srcPiece.PieceType;
                PieceColour auxColour = srcPiece.Colour;
                srcPiece.UpdateData(swapPiece.PieceType, swapPiece.Colour);                
                swapPiece.UpdateData(auxType, auxColour);              
            }         
            yield return _view.Reshuffle(_pieces);
        }

        private IEnumerator ApplyGravityAndRegenerate()
        {
            _currentPhase = BoardUpdatePhase.Gravity;
            bool gapFound = false;
            HashSet<Vector2Int> droppingPieces = new HashSet<Vector2Int>();
            List<(Piece, Vector2Int)> newPieces = new List<(Piece, Vector2Int)>();
            do
            {
                // TODO: Account for varying drop heights + refill so the dropping animations will be smoother
                gapFound = false;
                for (int row = 0; row < _rows; ++row)
                {
                    for (int col = 0; col < _cols; ++col)
                    {
                        Vector2Int coords = new Vector2Int(row, col);
                        if(_pieces[row,col] == null)
                        {
                            gapFound = true;
                        }
                        else if (row > 0 && _pieces[row-1, col] == null)
                        {
                            droppingPieces.Add(coords);
                            _pieces[row - 1, col] = _pieces[row, col];
                            _pieces[row, col] = null;
                        }
                    }
                }
                yield return _view.Drop(droppingPieces);
                droppingPieces.Clear();
                for (int col = 0; col < _cols; ++col)
                {
                    if (_pieces[_rows - 1, col] == null)
                    {
                        _pieces[_rows - 1, col] = Piece.GenerateRandom();
                        newPieces.Add((_pieces[_rows - 1, col], new Vector2Int(_rows - 1, col)));
                    }
                }
                yield return _view.Refill(newPieces);
                newPieces.Clear();
            } while (gapFound);
        }

        private IEnumerator ClearPieces(HashSet<Vector2Int> matchingCoordinates)
        {
            _currentPhase = BoardUpdatePhase.Exploding;
            foreach(var coords in matchingCoordinates)
            {
                (int row, int col) = (coords.x, coords.y);
                _pieces[row, col] = null;
            }
            yield return _view.ExplodePieces(matchingCoordinates);
        }

        private HashSet<Vector2Int> GetExplodingPieces(List<MatchInfo> matches)
        {
            var set = new HashSet<Vector2Int>();
            foreach(var match in matches)
            {
                foreach(var pieceCoords in match.MatchCoords)
                {
                    set.Add(pieceCoords);
                }
            }
            return set;
        }

        private void SwapPiecesAtCoords(Vector2Int selectedCoords, Vector2Int targetCoords)
        {
            Piece src = _pieces[selectedCoords.x, selectedCoords.y];
            Piece tgt = _pieces[targetCoords.x, targetCoords.y];
            PieceType auxType = src.PieceType;
            PieceColour auxColour = src.Colour;
            src.UpdateData(tgt.PieceType, tgt.Colour);
            tgt.UpdateData(auxType, auxColour);
            _view.ConfirmSwapAttempt(selectedCoords, targetCoords);
        }

        private bool CanSwapPiecesAtCoords(Vector2Int selectedCoords, Vector2Int targetCoords)
        {
            Piece selectedPiece = _pieces[selectedCoords.x, selectedCoords.y];
            Piece targetPiece = _pieces[targetCoords.x, targetCoords.y];
            Vector2Int excludeOffsetA = selectedCoords - targetCoords;
            Vector2Int excludeOffsetB = -excludeOffsetA;
            return FindMatch(selectedPiece.PieceType, selectedPiece.Colour, targetCoords, excludeOffsetA)
                || FindMatch(targetPiece.PieceType, targetPiece.Colour, selectedCoords, excludeOffsetB);
        }

        int CountCoordsWithMatches()
        {
            List<Vector2Int> targets = new List<Vector2Int>();
            for(int row = 0; row < _rows; ++row)
            {
                for (int col = 0; col < _cols; ++col)
                {
                    Vector2Int testCoords = new Vector2Int(row, col);
                    if (FindPotentialMatch(testCoords))
                    {
                        targets.Add(testCoords);
                    }
                }
            }
            return targets.Count;
        }

        bool FindPotentialMatch(Vector2Int refCoords)
        {
            if(!AreValidCoords(refCoords.x, refCoords.y) || _pieces[refCoords.x, refCoords.y] == null)
            {
                return false;
            }

            Vector2Int[] primaryNeighbourOffsets =
            {
                new Vector2Int(0,2), new Vector2Int(0,-2), new Vector2Int(-2,0), new Vector2Int(2,0), new Vector2Int(-1,-1), new Vector2Int(-1,1), new Vector2Int(1,-1), new Vector2Int(1,1)
            };

            Vector2Int[][] secondaryOffsets =
            {
                new Vector2Int[]{ new Vector2Int(0,3)},
                new Vector2Int[]{ new Vector2Int(0,-3)},
                new Vector2Int[]{ new Vector2Int(-3,0)},
                new Vector2Int[]{ new Vector2Int(3,0)},
                new Vector2Int[]{ new Vector2Int(-1,-2), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-2,-1)},
                new Vector2Int[]{ new Vector2Int(-1,2), new Vector2Int(1,1), new Vector2Int(-1,-1), new Vector2Int(-2,1)},
                new Vector2Int[]{ new Vector2Int(1,-2), new Vector2Int(1,1), new Vector2Int(-1,-1), new Vector2Int(2,-1)},
                new Vector2Int[]{ new Vector2Int(1,2), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(2,1)},
            };

            PieceType type = _pieces[refCoords.x, refCoords.y].PieceType;
            PieceColour colour = _pieces[refCoords.x, refCoords.y].Colour;
            return FindMatch(type, colour, refCoords, primaryNeighbourOffsets, secondaryOffsets, false, Vector2Int.zero);
        }

        bool FindMatch(PieceType type, PieceColour colour, Vector2Int refCoords, Vector2Int excludedOffset)
        {
            Vector2Int[] primaryNeighbourOffsets =
            {
                new Vector2Int(0,1), new Vector2Int(0,-1), new Vector2Int(-1,0), new Vector2Int(1,0)
            };

            Vector2Int[][] secondaryOffsets =
            {
                new Vector2Int[]{ new Vector2Int(0, 2), new Vector2Int(0,-1)},
                new Vector2Int[]{ new Vector2Int(0,-2), new Vector2Int(0,1)},
                new Vector2Int[]{ new Vector2Int(-2,0), new Vector2Int(1,0)},
                new Vector2Int[]{ new Vector2Int(2,0), new Vector2Int(-1,0)},
            };

            return FindMatch(type, colour, refCoords, primaryNeighbourOffsets, secondaryOffsets, true, excludedOffset);
        }

        bool FindMatch(PieceType type, PieceColour colour, Vector2Int refCoords, Vector2Int[] primaryNeighbourOffsets, Vector2Int[][] secondaryOffsets, bool useExcludedOffset, Vector2Int excludedOffset)
        {
            for (int i = 0; i < primaryNeighbourOffsets.Length; ++i)
            {
                Vector2Int primaryOffset = primaryNeighbourOffsets[i];
                Vector2Int neighbourCoords = refCoords + primaryOffset;

                if ((useExcludedOffset && primaryOffset == excludedOffset) || !AreValidCoords(neighbourCoords))
                {
                    continue;
                }
                Piece neighbour = _pieces[neighbourCoords.x, neighbourCoords.y];
                if (neighbour == null)
                {
                    continue;
                }

                if (neighbour.IsMatchingData(type, colour))
                {
                    // Evaluate secondary offsets
                    Vector2Int[] testSecOffsets = secondaryOffsets[i];
                    for (int j = 0; j < testSecOffsets.Length; ++j)
                    {
                        Vector2Int secondaryOffset = testSecOffsets[j];
                        Vector2Int secondaryNeighbour = refCoords + secondaryOffset;
                        if ((useExcludedOffset && secondaryOffset == excludedOffset) || !AreValidCoords(secondaryNeighbour))
                        {
                            continue;
                        }
                        Piece secondaryNeighbourPiece = _pieces[secondaryNeighbour.x, secondaryNeighbour.y];
                        if (secondaryNeighbourPiece.IsMatchingData(type, colour))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        void InitPieces(BaseLevelData levelData)
        {
            _pieces = levelData.Generate();
            _rows = _pieces.GetLength(0);
            _cols = _pieces.GetLength(1);
        }

        bool AreValidCoords(int row, int col)
        {
            return row >= 0 && row < _rows && col >= 0 && col < _cols;
        }

        bool AreValidCoords(Vector2Int coords)
        {
            (int row, int col) = (coords.x, coords.y);
            return AreValidCoords(row, col);
        }
    }
}
