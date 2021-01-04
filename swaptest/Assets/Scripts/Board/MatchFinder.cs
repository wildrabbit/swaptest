using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Board
{
    public enum MatchType
    {
        Match3,
        Match4,
        Match5,
        Cross,
        T,
        Square
    }
    public class MatchInfo
    {
        public MatchType MatchType { get; private set; }
        public List<Vector2Int> MatchCoords { get; private set; }
    }

    public static class MatchFinder
    {
        public static List<MatchInfo> FindMatches(Piece[,] pieces)
        {
            int rows = pieces.GetLength(0);
            int cols = pieces.GetLength(1);
            List<List<Vector2Int>> blobs = new List<List<Vector2Int>>();

            for(int i = 0; i < rows; ++i)
            {
                for(int j = 0; j < cols; ++j)
                {
                    bool inBlob = blobs.Exists(
                        testBlob => testBlob.Exists(coords => coords.x == i && coords.y == j)
                    );
                    if (inBlob) continue;

                    List<Vector2Int> blob = new List<Vector2Int>();
                    blob.Add(new Vector2Int(i, j));

                    Piece reference = pieces[i, j];
                    if (reference == null) continue;

                    if(i + 1 < rows)
                    {
                        TestBlob(pieces, i + 1, j, rows, cols, reference, blob, blobs);
                    }

                    if (j + 1 < cols)
                    {
                        TestBlob(pieces, i, j + 1, rows, cols, reference, blob, blobs);
                    }
                    
                    // TODO: Analyze blob to get the match type
                    if(blob.Count >= 3)
                    {
                        blobs.Add(blob);
                    }
                }
            }
            return new List<MatchInfo>();
        }

        static void TestBlob(Piece[,] pieces, int i, int j, int rows, int cols, Piece pieceToMatch, List<Vector2Int> blob, List<List<Vector2Int>> blobs)
        {
            bool inBlob = blobs.Exists(testBlob => testBlob.Exists(coords => coords.x == i && coords.y == j)) || blob.Exists(coords => coords.x == i && coords.y == j);
            if (inBlob) return;
            Piece referencePiece = pieces[i, j];
            if (referencePiece == null || referencePiece.PieceType != pieceToMatch.PieceType || referencePiece.Colour != pieceToMatch.Colour) return;

            blob.Add(new Vector2Int(i, j));

            if (i + 1 < rows)
            {
                TestBlob(pieces, i + 1, j, rows, cols, referencePiece, blob, blobs);
            }

            if (j + 1 < cols)
            {
                TestBlob(pieces, i, j + 1, rows, cols, referencePiece, blob, blobs);
            }
        }

        public static List<MatchInfo> FindPotentialMatches(Piece[,] pieces)
        {
            return new List<MatchInfo>();
        }
    }
}
