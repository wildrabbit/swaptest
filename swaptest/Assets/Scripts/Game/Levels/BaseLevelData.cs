using Game.Board;
using UnityEngine;

namespace Game.Levels
{
   public abstract class BaseLevelData: ScriptableObject
   {
        [Header("Common level data")]
        [SerializeField] float _playTime;
        [SerializeField] bool _isSeeded;
        [SerializeField] string _randomSeed;

        public float PlayTime => _playTime;
        public bool IsSeeded => _isSeeded;
        public string RandomSeed => _randomSeed;

        public abstract Piece[,] Generate();
    }
}