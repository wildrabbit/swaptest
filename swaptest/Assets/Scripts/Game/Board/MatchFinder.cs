using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Game.Board
{
    public enum MatchType
    {
        Match3,
        Match4,
        Match5,
        //Cross,
        //T,
        //Square,
        //L
    }
    public class MatchInfo
    {
        public MatchType MatchType { get; private set; }
        public List<Vector2Int> MatchCoords { get; private set; }
        public PieceType RefType { get; private set; }
        public PieceColour RefColour { get; private set; }

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
                    if (refPiece == null)
                    {
                        j++;
                        continue;
                    }
                    int k = j + 1;
                    while (k < rows)
                    {
                        Piece testPiece = pieces[k, i];
                        if (Piece.CanPiecesMatch(testPiece, refPiece))
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
                    if(refPiece == null)
                    {
                        j++;
                        continue;
                    }
                    int k = j + 1;
                    while (k < cols)
                    {
                        Piece testPiece = pieces[i, k];
                        if (Piece.CanPiecesMatch(testPiece, refPiece))
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
    }
}
