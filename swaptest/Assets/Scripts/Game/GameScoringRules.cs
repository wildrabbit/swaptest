using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

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
        [SerializeField] List<ChainPair> _chains = new List<ChainPair>();
        bool _sorted = false;

        public int Match3Score => _match3BaseScore;
        public int Match4Score => _match4BaseScore;
        public int Match5Score => _match5BaseScore;

        public int GetMultiplierForStep(int step)
        {
            if(!_sorted)
            {
                _chains.Sort((pair1, pair2) => pair1.ChainStep.CompareTo(pair2.ChainStep));
                _sorted = true;
            }

            for(int i = 0; i < _chains.Count; ++i)
            {
                if(step == _chains[i].ChainStep)
                {
                    return _chains[i].Multiplier;
                }
            }
            return _chains[_chains.Count - 1].Multiplier;
        }
    }
}
