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

        void Awake()
        {
            InitBoard();
            InitPieces();
            LoadView();
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

        private void BeginBoardUpdatePhase()
        {
            throw new NotImplementedException();
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
            Piece[,] pieceCopy = new Piece[_rows, _cols];
            System.Array.Copy(_pieces, pieceCopy, _rows * _cols);
            pieceCopy[selectedCoords.x, selectedCoords.y] = _pieces[targetCoords.x, targetCoords.y];
            pieceCopy[targetCoords.x, targetCoords.y] = _pieces[selectedCoords.x, selectedCoords.y];
            // TODO: Find match
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


        bool IsValidSlot(int row, int col)
        {
            Debug.Assert(row >= 0 && row < _rows && col >= 0 && col < _cols, "Invalid coordinates");
            return _boardLayout[row, col];
        }

    }
}
