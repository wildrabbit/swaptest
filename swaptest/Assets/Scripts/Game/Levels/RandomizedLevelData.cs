using UnityEngine;
using System.Collections;
using Game.Board;

namespace Game.Levels
{
    [CreateAssetMenu(fileName = "RandomizedLevel", menuName = "Match3/Create Randomized Level Data")]
    public class RandomizedLevelData : BaseLevelData
    {
        [Header("Board dimensions")]
        [SerializeField] int _rows;
        [SerializeField] int _cols;

        public int Rows => _rows;
        public int Cols => _cols;

        public override Piece[,] Generate()
        {
            var pieces = new Piece[_rows, _cols];
            for (int i = 0; i < _rows; ++i)
            {
                for (int j = 0; j < _cols; ++j)
                {
                    Piece[] horz = { };
                    Piece[] vert = { };
                    if (j >= 2)
                    {
                        horz = new Piece[]
                        {
                            pieces[i, j - 1], pieces[i, j - 2]
                        };
                    }
                    if (i >= 2)
                    {
                        vert = new Piece[]
                        {
                            pieces[i - 1, j], pieces[i - 2, j]
                        };
                    }
                    pieces[i,j] = Piece.GenerateRandom(horz, vert);
                }
            }
            return pieces;
        }
    }
}
