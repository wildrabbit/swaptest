using System;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace Game.Board
{
    // Using enums for simplicity. For full extensibility it would probably be better
    // to use a list of identifiable data structures (PieceTypeDefinition, PieceColourDefinition,...)

    public enum PieceType
    {
        Normal
        //,HLineBooster,
        //,VLineBooster,
        //,Bomb,
        //,Rainbow
    }

    public enum PieceColour
    {
        Red = 0,
        Yellow,
        Blue,
        Green,
        Orange
        //....
    }

    public class Piece
    {
        public PieceType PieceType { get; private set; }
        public PieceColour Colour { get; private set; }

        public Piece(PieceType type, PieceColour colour)
        {
            UpdateData(type, colour);
        }

        public void UpdateData(PieceType pieceType, PieceColour colour)
        {
            PieceType = pieceType;
            Colour = colour;
        }

        public bool CanMatch(Piece other)
        {
            return other != null && IsMatchingData(other.PieceType, other.Colour);
        }

        public bool IsMatchingData(PieceType type, PieceColour colour)
        {
            return PieceType == type && Colour == colour;
        }

 #region static block
        static PieceColour[] _allColours;
        public static PieceColour[] AllColours
        {
            get
            {
                if (_allColours == null)
                {
                    _allColours = (PieceColour[])Enum.GetValues(typeof(PieceColour));
                }
                return _allColours;
            }
        }

        public static bool CanPiecesMatch(Piece p1, Piece p2)
        {
            return p1 != null && p1.CanMatch(p2);
        }

        // Generate a randomised normal piece
        public static Piece GenerateRandom()
        {
            return new Piece(PieceType.Normal, AllColours[URandom.Range(0, AllColours.Length)]);
        }

        // This override generates a random normal piece accounting for a couple of lists of preexisting pieces in order to
        // guarantee we won't generate a match
        public static Piece GenerateRandom(Piece[] previousHorz, Piece[] previousVert)
        {
            List<PieceColour> validColours = new List<PieceColour>(AllColours);

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
#endregion
    }
}
