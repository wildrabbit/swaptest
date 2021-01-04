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

        bool[,] _boardLayout;
        Piece[,] _pieces;
        string _lastSeed;

        void Awake()
        {
            InitBoard();
            InitPieces();
        }

        void Start()
        {
            _view.LoadView(_pieces);    
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
                        _pieces[i, j] = GeneratePiece();
                    }
                    else
                    {
                        _pieces[i, j] = null;
                    }
                }
            }

            // Purge matches
            PurgeMatches();
        }

        void PurgeMatches()
        {
            int numAttempts = 0;
            List<MatchInfo> matches = new List<MatchInfo>();

            do
            {
                matches = MatchFinder.FindMatches(_pieces);
                //ClearMatches();
                //ApplyGravity();
                //Refill();
                numAttempts++;
            }
            while (matches.Count > 0 && numAttempts < 10);
            
            
        }

        Piece GeneratePiece()
        {
            // TODO: Account for weights, adjacency,etc.For now, go pure random
            return new Piece(PieceType.Normal, (Colour)URandom.Range((int)Colour.Red, (int)Colour.NumColours));
        }

        bool IsValidSlot(int row, int col)
        {
            Debug.Assert(row >= 0 && row < _rows && col >= 0 && col < _cols, "Invalid coordinates");
            return _boardLayout[row, col];
        }

    }
}
