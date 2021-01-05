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
        Orange
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

        public void Update(PieceType pieceType, Colour colour)
        {
            PieceType = pieceType;
            Colour = colour;
        }
    }
}
