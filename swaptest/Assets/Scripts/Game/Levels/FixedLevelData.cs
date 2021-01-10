using Game.Board;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>
    /// Generate a level given a text-based layout
    /// </summary>
    [CreateAssetMenu(fileName = "FixedLevel", menuName ="Match3/Create Fixed Level Data")]
    public class FixedLevelData: BaseLevelData
    {
        [Header("Fixed level data")]
        [SerializeField, Tooltip("If checked the list of rows will be processed in reverse order to ensure the first row will match the top row in-game")] bool _firstRowAtTop = true;
        [SerializeField, TextArea(10,10)] string _levelLayout;
        public string LevelLayout => _levelLayout;

        public override Piece[,] Generate()
        {
            var lines = new List<string>(_levelLayout.Split('\n'));
            lines.RemoveAll(line => string.IsNullOrEmpty(line.Trim())); // Remove potentially empty lines

            int numLines = lines.Count;

            Debug.Assert(numLines > 0, "Empty file");

            var dimensions = lines[0].Split(',');
            Debug.Assert(dimensions.Length == 2, "First line must start with the dimensions of the board in rows,cols format. Ex: 9,9");
            if (!Int32.TryParse(dimensions[0], out int rows) || !Int32.TryParse(dimensions[1], out int cols))
            {
                Debug.LogError("Rows or columns don't match to valid integers. Will skip");
                return new Piece[,] { };
            }

            Piece[,] pieces = new Piece[rows, cols];
            Debug.Assert(rows == numLines - 1, $"Invalid number of rows. Must be {rows}, got {numLines - 1}");
            for(int i = 0; i < rows; ++i)
            {
                int rowIdx = _firstRowAtTop ? numLines - 1 - i : i + 1;
                var columns = lines[rowIdx].Split(',');
                int numColumns = columns.Length;
                Debug.Assert(numColumns == cols, $"Invalid number of columns @ row {i}. Must be {cols}, got {numColumns}");
                for (int j = 0; j < cols; ++j)
                {
                    //var pieceData = columns[j].Split(':');
                    //Debug.Assert(pieceData.Length == 2, "Piece data must have the following format: <b>\"PieceTypeID:ColourID\"</b>. Ex: 0:3");
                    //int typeInt = 0;
                    int colourInt = 0;
                    if (!Int32.TryParse(columns[j], out colourInt))
                    {
                        Debug.LogError($"Invalid colour for data @ coords {i},{j}");
                        pieces[i, j] = null;
                    }
                    pieces[i, j] = new Piece(PieceType.Normal, (PieceColour)colourInt);
                }
            }
            return pieces;
        }
    }
}
