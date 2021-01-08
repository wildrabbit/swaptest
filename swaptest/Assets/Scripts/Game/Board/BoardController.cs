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

        void Start()
        {
            _viewEvents = GameController.GameEvents.View;
            _viewEvents.SwapAnimationCompleted += OnSwapAnimationCompleted;
            _boardEvents = GameController.GameEvents.Board;
        }

        public void Init(BaseLevelData level)
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
            if(CanMatch(selectedCoords, targetCoords))
            {
                SwapPositions(selectedCoords, targetCoords);
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
                int possibleMatches = CountCoordsWithMatches();
                int reshuffleCount = 0;
                while (possibleMatches == 0 && reshuffleCount < _maxReshuffles)
                {
                    _currentPhase = BoardUpdatePhase.Reshuffling;
                    yield return Reshuffle();
                    possibleMatches = CountCoordsWithMatches();
                    reshuffleCount++;
                }

                matches = MatchFinder.FindMatches(_pieces);
                if(matches.Count == 0)
                {
                    _currentPhase = BoardUpdatePhase.Stable;
                }
                yield return null;
            } while(_currentPhase != BoardUpdatePhase.Stable);            
            _view.CompleteBoardUpdate();
        }

        private IEnumerator Reshuffle()
        {
            Dictionary<Vector2Int, Vector2Int> swaps = new Dictionary<Vector2Int, Vector2Int>();
            int totalPieces = _pieces.Length;
            for(int i = 0; i < totalPieces; ++i)
            {
                Vector2Int startCoords = new Vector2Int(i / _rows, i % _rows);
                swaps.Add(startCoords, startCoords);
            }
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
                Colour auxColour = srcPiece.Colour;
                srcPiece.UpdateData(swapPiece.PieceType, swapPiece.Colour);
                var srcCoords = new Vector2Int(srcRow, srcCol);
                var swapCoords = new Vector2Int(swapRow, swapCol);
                swaps[srcCoords] = swapCoords;
                swaps[swapCoords] = srcCoords;
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

        private void SwapPositions(Vector2Int selectedCoords, Vector2Int targetCoords)
        {
            Piece src = _pieces[selectedCoords.x, selectedCoords.y];
            Piece tgt = _pieces[targetCoords.x, targetCoords.y];
            PieceType auxType = src.PieceType;
            Colour auxColour = src.Colour;
            src.UpdateData(tgt.PieceType, tgt.Colour);
            tgt.UpdateData(auxType, auxColour);
            _view.ConfirmSwapAttempt(selectedCoords, targetCoords);
        }

        private bool CanMatch(Vector2Int selectedCoords, Vector2Int targetCoords)
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

            (int, int)[] primaryNeighbourOffsets =
            {
                (0,2), (0,-2), (-2,0), (2,0), (-1,-1), (-1,1), (1,-1), (1,1)
            };

            (int, int)[][] secondaryOffsets =
            {
                new (int, int)[]{(0,3)},
                new (int, int)[]{(0,-3)},
                new (int, int)[]{(-3,0)},
                new (int, int)[]{(3,0)},
                new (int, int)[]{(-1,-2), (1,-1), (-1,1), (-2,-1)},
                new (int, int)[]{(-1,2), (1,1), (-1,-1), (-2,1)},
                new (int, int)[]{(1,-2), (1,1), (-1,-1), (2,-1)},
                new (int, int)[]{(1,2), (1,-1), (-1,1), (2,1)},
            };

            PieceType type = _pieces[refCoords.x, refCoords.y].PieceType;
            Colour colour = _pieces[refCoords.x, refCoords.y].Colour;
            return FindMatch(type, colour, refCoords, primaryNeighbourOffsets, secondaryOffsets, false, Vector2Int.zero);
        }

        bool FindMatch(PieceType type, Colour colour, Vector2Int refCoords, Vector2Int excludedOffset)
        {
            (int, int)[] primaryNeighbourOffsets =
            {
                (0,1), (0,-1), (-1,0), (1,0)
            };

            (int, int)[][] secondaryOffsets =
            {
                new (int, int)[]{(0, 2), (0,-1)},
                new (int, int)[]{(0,-2), (0,1)},
                new (int, int)[]{(-2,0), (1,0)},
                new (int, int)[]{(2,0), (-1,0)},
            };

            return FindMatch(type, colour, refCoords, primaryNeighbourOffsets, secondaryOffsets, true, excludedOffset);
        }

        bool FindMatch(PieceType type, Colour colour, Vector2Int refCoords, (int, int)[] primaryNeighbourOffsets, (int, int)[][] secondaryOffsets, bool useExcludedOffset, Vector2Int excludedOffset)
        {
            (int row, int col) = (refCoords.x, refCoords.y);
            for (int i = 0; i < primaryNeighbourOffsets.Length; ++i)
            {
                (int rowOffset, int colOffset) = primaryNeighbourOffsets[i];

                if (!AreValidCoords(row + rowOffset, col + colOffset) || (useExcludedOffset && rowOffset == excludedOffset.x && colOffset == excludedOffset.y))
                {
                    continue;
                }
                Piece neighbour = _pieces[row + rowOffset, col + colOffset];
                if (neighbour == null)
                {
                    continue;
                }
                if (neighbour.PieceType == type && neighbour.Colour == colour)
                {
                    // Evaluate secondary offsets
                    (int, int)[] testSecOffsets = secondaryOffsets[i];
                    for (int j = 0; j < testSecOffsets.Length; ++j)
                    {
                        (int secRowOffset, int secColOffset) = testSecOffsets[j];
                        if (!AreValidCoords(row + secRowOffset, col + secColOffset) || (useExcludedOffset && secRowOffset == excludedOffset.x && secColOffset == excludedOffset.y))
                        {
                            continue;
                        }
                        Piece secondaryNeighbour = _pieces[row + secRowOffset, col + secColOffset];
                        if (secondaryNeighbour.PieceType == type && secondaryNeighbour.Colour == colour)
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

        private void ClearMatches(List<MatchInfo> matches)
        {
            throw new NotImplementedException();
        }

        bool AreValidCoords(int row, int col)
        {
            return row >= 0 && row < _rows && col >= 0 && col < _cols;
        }
    }
}
