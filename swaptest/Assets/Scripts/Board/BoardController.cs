using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using View;
using URandom = UnityEngine.Random;

namespace Board
{
    public class BoardController : MonoBehaviour
    {

        enum BoardUpdatePhase
        {
            Stable,
            Matching,
            Exploding,
            Gravity
        }

        [SerializeField] int _cols;
        [SerializeField] int _rows;
        [SerializeField] bool _isSeeded;
        [SerializeField] string _seedJson;

        [SerializeField] BoardView _view;

        
        public int Cols => _cols;
        public int Rows => _rows;
        public BoardView View => _view;

        bool[,] _boardLayout;
        Piece[,] _pieces;
        string _lastSeed;
        BoardUpdatePhase _currentPhase;

        void Awake()
        {
            InitBoard();
            InitPieces();
            LoadView();
            _currentPhase = BoardUpdatePhase.Stable;
        }

        void OnDestroy()
        {
            _view.SwapAnimationCompleted -= OnSwapAnimationCompleted;
        }

        private void LoadView()
        {
            _view.SwapAnimationCompleted -= OnSwapAnimationCompleted;
            _view.LoadView(_pieces);
            _view.SwapAnimationCompleted += OnSwapAnimationCompleted;
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

        void BeginBoardUpdatePhase()
        {
            StartCoroutine(BoardUpdateRoutine());
        }

        IEnumerator BoardUpdateRoutine()
        {
            _currentPhase = BoardUpdatePhase.Matching;
            _view.PrepareBoardUpdate();
            var matches = MatchFinder.FindMatches(_pieces);
            while(matches.Count > 0)
            {
                // TODO: Scoring, special generation, etc
                yield return ClearPieces(GetExplodingPieces(matches));
                matches = MatchFinder.FindMatches(_pieces);
            }
            yield return null;
            _currentPhase = BoardUpdatePhase.Stable;
            _view.CompleteBoardUpdate();
        }

        private IEnumerator ClearPieces(HashSet<Vector2Int> matchingCoordinates)
        {
            foreach(var coords in matchingCoordinates)
            {
                (int row, int col) = (coords.x, coords.y);
                _pieces[row, col] = null;
            }
            return _view.ExplodePieces(matchingCoordinates);
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
            src.Update(tgt.PieceType, tgt.Colour);
            tgt.Update(auxType, auxColour);
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

            (int row, int col) = (refCoords.x, refCoords.y);
            for(int i = 0; i < primaryNeighbourOffsets.Length; ++i)
            {
                (int rowOffset, int colOffset) = primaryNeighbourOffsets[i];

                if (!AreValidCoords(row + rowOffset, col + colOffset) || (rowOffset == excludedOffset.x && colOffset == excludedOffset.y))
                {
                    continue;
                }
                Piece neighbour = _pieces[row + rowOffset, col + colOffset];
                if(neighbour == null )
                {
                    continue;
                }
                if(neighbour.PieceType == type && neighbour.Colour == colour)
                {
                    // Evaluate secondary offsets
                    (int, int)[] testSecOffsets = secondaryOffsets[i];
                    for(int j = 0; j < testSecOffsets.Length; ++j)
                    {
                        (int secRowOffset, int secColOffset) = testSecOffsets[j];
                        if (!AreValidCoords(row + secRowOffset, col + secColOffset) || (secRowOffset == excludedOffset.x && secColOffset == excludedOffset.y))
                        {
                            continue;
                        }
                        Piece secondaryNeighbour = _pieces[row + secRowOffset, col + secColOffset];
                        if(secondaryNeighbour.PieceType == type && secondaryNeighbour.Colour == colour)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        void InitBoard()
        {
            _boardLayout = new bool[_rows, _cols];
            _boardLayout.Fill(true);
        }

        void InitPieces()
        {
            _pieces = new Piece[_rows, _cols];

            if (_isSeeded)
            {
                URandom.state = JsonUtility.FromJson<URandom.State>(_seedJson);
                _lastSeed = _seedJson;
            }
            else
            {
                _lastSeed = JsonUtility.ToJson(URandom.state);
                Debug.Log($"Current state: {_lastSeed}");
            }

            for (int i = 0; i < _rows; ++i)
            {
                for(int j = 0; j < _cols; ++j)
                {
                    if(_boardLayout[i, j])
                    {
                        Piece[] horz = {};
                        Piece[] vert = {};
                        if (j >= 2)
                        {
                            horz = new Piece[]
                            {
                                _pieces[i, j - 1], _pieces[i, j - 2]
                            };
                        }
                        if(i >= 2)
                        {
                            vert = new Piece[]
                            {
                                _pieces[i - 1, j], _pieces[i - 2, j]
                            };
                        }
                        _pieces[i, j] = GeneratePiece(horz, vert);
                    }
                    else
                    {
                        _pieces[i, j] = null;
                    }
                }
            }
        }

        Piece GeneratePiece(Piece[] previousHorz, Piece[] previousVert)
        {
            // Ensure there will be no matches beforehand
            List<Colour> validColours = new List<Colour>();
            validColours.AddRange((Colour[])Enum.GetValues(typeof(Colour)));

            if (previousHorz.Length == 2 && previousHorz[1].PieceType == previousHorz[0].PieceType && previousHorz[1].Colour == previousHorz[0].Colour)
            {
                validColours.Remove(previousHorz[0].Colour);
            }
            if (previousVert.Length == 2 && previousVert[1].PieceType == previousVert[0].PieceType && previousVert[1].Colour == previousVert[0].Colour)
            {
                validColours.Remove(previousVert[0].Colour);
            }

            return new Piece(PieceType.Normal, validColours[URandom.Range(0, validColours.Count)]);
        }

        private void ClearMatches(List<MatchInfo> matches)
        {
            throw new NotImplementedException();
        }

        bool AreValidCoords(int row, int col)
        {
            return row >= 0 && row < _rows && col >= 0 && col < _cols;
        }

        bool IsValidSlot(int row, int col)
        {
            Debug.Assert(AreValidCoords(row, col), "Invalid coordinates");
            return _boardLayout[row, col];
        }

    }
}
