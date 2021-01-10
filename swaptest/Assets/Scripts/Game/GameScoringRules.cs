using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Game
{
    [Serializable]
    public class GameScoringRules
    {
        [Serializable]
        public class ChainPair
        {
            public int ChainStep = 0;
            public int Multiplier = 1;
        }

        [SerializeField] int _match3BaseScore = 50;
        [SerializeField] int _match4BaseScore = 100;
        [SerializeField] int _match5BaseScore = 200;
        [SerializeField] List<ChainPair> _matchChainMultipliers = new List<ChainPair>();

        public int Match3Score => _match3BaseScore;
        public int Match4Score => _match4BaseScore;
        public int Match5Score => _match5BaseScore;

        bool _sortedMultipliers = false;

        public int GetMultiplierForStep(int step)
        {
            int numMultipliers = _matchChainMultipliers?.Count ?? 0;
            if (numMultipliers == 0)
            {
                return 1;
            }

            if (!_sortedMultipliers)
            {
                // Ensure the list will always be sorted regardless of the order values are set in the inspector.
                _matchChainMultipliers.Sort((pair1, pair2) => pair1.ChainStep.CompareTo(pair2.ChainStep));
                _sortedMultipliers = true;
            }

            for(int i = 0; i < numMultipliers; ++i)
            {
                if(step == _matchChainMultipliers[i].ChainStep)
                {
                    return _matchChainMultipliers[i].Multiplier;
                }
            }
            return _matchChainMultipliers[numMultipliers - 1].Multiplier;
        }
    }
}
