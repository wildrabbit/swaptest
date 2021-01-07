using System;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace Game.Board
{
    public enum PieceType
    {
        Normal
    }

    public enum Colour
    {
        Red = 0,
        Yellow,
        Blue,
        Green,
        Orange
    }

    public class Piece
    {
        static Colour[] _allColours;

        public PieceType PieceType { get; private set; }
        public Colour Colour { get; private set; }

        public static Colour[] AllColours
        {
            get
            {
                if (_allColours == null)
                {
                    _allColours = (Colour[])Enum.GetValues(typeof(Colour));
                }
                return _allColours;
            }
        }

        public Piece(PieceType type, Colour colour)
        {
            UpdateData(type, colour);
        }

        public void UpdateData(PieceType pieceType, Colour colour)
        {
            PieceType = pieceType;
            Colour = colour;
        }

        public static Piece GenerateRandom(Piece[] previousHorz, Piece[] previousVert)
        {
            // Ensure there will be no matches beforehand
            List<Colour> validColours = new List<Colour>(AllColours);

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

        public static Piece GenerateRandom()
        {
            return new Piece(PieceType.Normal, AllColours[URandom.Range(0, AllColours.Length)]);
        }
    }
}
