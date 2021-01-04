using System;
using System.Collections.Generic;
using UnityEngine;

namespace Board
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
        Orange,
        NumColours
    }

    public class Piece
    {
        public PieceType PieceType { get; private set; }
        public Colour Colour { get; private set; }
        
        public Piece(PieceType type, Colour colour)
        {
            PieceType = type;
            Colour = colour;
        }
    }
}
