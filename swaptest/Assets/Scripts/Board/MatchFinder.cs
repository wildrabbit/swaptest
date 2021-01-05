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
        //Cross,
        //T,
        //Square
    }
    public class MatchInfo
    {
        public MatchType MatchType { get; private set; }
        public List<Vector2Int> MatchCoords { get; private set; }
        public PieceType RefType { get; private set; }
        public Colour RefColour { get; private set; }

        public static MatchInfo Create(Piece referencePiece, int rows, List<int> indexes)
        {
            MatchType type = MatchType.Match3;
            if(indexes.Count == 4)
            {
                type = MatchType.Match4;
            }
            else if (indexes.Count == 5)
            {
                type = MatchType.Match5;
            }

            return new MatchInfo
            {
                MatchType = type,
                MatchCoords = indexes.ConvertAll(index => new Vector2Int(index / rows, index % rows)),
                RefType = referencePiece.PieceType,
                RefColour = referencePiece.Colour
            };
        }
    }

    public static class MatchFinder
    {
        public static List<MatchInfo> FindMatches(Piece[,] pieces)
        {
            int rows = pieces.GetLength(0);
            int cols = pieces.GetLength(1);
            List<MatchInfo> totalMatches = new List<MatchInfo>();
            FindHorizontalMatches(pieces, rows, cols, totalMatches);
            FindVerticalMatches(pieces, rows, cols, totalMatches);
            return totalMatches;
            //for (int i = 0; i < rows; ++i)
            //{
            //    int j = 0;
            //    while (j < cols - 2)
            //    {

            //    }
            //    // TODO: 
            //    for (int j = 0; j < cols; ++j)
            //    {
            //        bool inBlob = blobs.Exists(
            //            testBlob => testBlob.Exists(coords => coords.x == i && coords.y == j)
            //        );
            //        if (inBlob) continue;

            //        List<Vector2Int> blob = new List<Vector2Int>();
            //        blob.Add(new Vector2Int(i, j));

            //        Piece reference = pieces[i, j];
            //        if (reference == null) continue;

            //        if (i + 1 < rows)
            //        {
            //            TestBlob(pieces, i + 1, j, rows, cols, reference, blob, blobs);
            //        }

            //        if (j + 1 < cols)
            //        {
            //            TestBlob(pieces, i, j + 1, rows, cols, reference, blob, blobs);
            //        }

            //        // TODO: Analyze blob to get the match type
            //        if (blob.Count >= 3)
            //        {
            //            blobs.Add(blob);
            //        }
            //    }
            //}
            //return new List<MatchInfo>();
        }

        private static void FindVerticalMatches(Piece[,] pieces, int rows, int cols, List<MatchInfo> totalMatches)
        {
            for (int i = 0; i < cols; ++i)
            {
                int j = 0;
                while (j < rows- 2)
                {
                    List<int> candidateMatchIndices = new List<int>();
                    candidateMatchIndices.Add(j * rows + i);
                    Piece refPiece = pieces[j, i];
                    int k = j + 1;
                    while (k < rows)
                    {
                        Piece testPiece = pieces[j, k];
                        if (testPiece.PieceType == refPiece.PieceType && testPiece.Colour == refPiece.Colour)
                        {
                            candidateMatchIndices.Add(k * rows + i);
                            k++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    j += candidateMatchIndices.Count;
                    if (candidateMatchIndices.Count >= 3)
                    {
                        totalMatches.Add(MatchInfo.Create(refPiece, rows, candidateMatchIndices));
                    }
                }
            }
        }

        private static void FindHorizontalMatches(Piece[,] pieces, int rows, int cols, List<MatchInfo> totalMatches)
        {
            for (int i = 0; i < rows; ++i)
            {
                int j = 0;
                while (j < cols - 2)
                {
                    List<int> candidateMatchIndices = new List<int>();
                    candidateMatchIndices.Add(i * rows + j);
                    Piece refPiece = pieces[i, j];
                    int k = j + 1;
                    while (k < cols)
                    {
                        Piece testPiece = pieces[i, k];
                        if (testPiece.PieceType == refPiece.PieceType && testPiece.Colour == refPiece.Colour)
                        {
                            candidateMatchIndices.Add(i * rows + k);
                            k++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    j += candidateMatchIndices.Count;
                    if(candidateMatchIndices.Count >= 3)
                    {
                        totalMatches.Add(MatchInfo.Create(refPiece, rows, candidateMatchIndices));
                    }
                }
            }
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
