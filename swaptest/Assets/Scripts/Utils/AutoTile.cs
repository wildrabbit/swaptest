using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    [Serializable]
    public class AutoTileRule
    {
        public enum ConstraintType
        {
            DontCare,
            RequireValid,
            RequireInvalid
        }

        public class NeighbourConstraint
        {            
            public int X;
            public int Y;
            public ConstraintType ConstraintType;
        }

        [SerializeField] List<NeighbourConstraint> _neighbourRequirement;
        [SerializeField] Sprite _replacementSprite;
    }
    
    // Automatically replace tile sprites depending on adjacency. This is convenient to avoid using multiple tile types for corners or borders.
    public class AutoTile: TileBase
    {
        [SerializeField] Sprite _baseSprite;
        [SerializeField] List<AutoTileRule> _replacementRules;
    }
}
