using System;
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

        [SerializeField] Transform _piecesRoot; // Bottom left!!

        // TODO: Add grid view too (tilemap-based)

        List<PieceView> _pieceInstances = new List<PieceView>();

        public void LoadView(Piece[,] pieces)
        {
            int rows = pieces.GetLength(0);
            int cols = pieces.GetLength(1);
            Cleanup();

            float yOffset = _cellHeight * (rows / 2) - (1 - (rows % 2)) * _cellHeight * 0.5f;
            float xOffset = _cellWidth * (cols / 2) - (1 - (cols % 2)) * _cellWidth* 0.5f;

            for (int i = 0; i < rows; ++i)
            {
                for(int j = 0; j < cols; ++j)
                {
                    PieceView instance = Instantiate(GetPrefabForPiece(pieces[i, j]), _piecesRoot);
                    Vector3 position = new Vector3
                    {
                        x = j * _cellWidth - xOffset,
                        y = i * _cellHeight - yOffset,
                        z = 0.0f
                    };
                    instance.Init(new Vector2Int(i,j), position);
                    _pieceInstances.Add(instance);
                }
            }
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
