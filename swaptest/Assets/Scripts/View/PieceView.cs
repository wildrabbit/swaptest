using UnityEngine;
using System.Collections;
using Board;

namespace View
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] PieceType _pieceType;
        [SerializeField] Colour _colour;

        public PieceType PieceType => _pieceType;
        public Colour Colour => _colour;

        Vector2Int _coords;

        public void Init(Vector2Int coords, Vector3 position)
        {
            _coords = coords;
            transform.localPosition = position;
        }
    }
}
