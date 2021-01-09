using UnityEngine;
using Game.Input;
using Game.Levels;
using Game.Events;
using URandom = UnityEngine.Random;
using Game.Board;
using System.Collections.Generic;

namespace Game
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] BaseLevelData _levelData;
        [SerializeField] GameScoringRules _scoringRules;
        [SerializeField] Game.Board.BoardController _boardController;
        [SerializeField] InputController _inputController;
        [SerializeField] float _runOutElapsedRemaining = 5f;

        bool _running;
        bool _finished;
        int _score;
        float _elapsed;
        float _totalTime;
        string _lastSeed;
        bool _notifiedRunningOut = false;
        private GameEvents _gameEvents;
        public float RemainingTime => _totalTime - _elapsed;
        public bool Running => _running;
        public bool Finished => _finished;


        void Start()
        {
            _gameEvents = GameEvents.Instance;
            _gameEvents.Board.MatchesFound += OnMatchesFound;
            _gameEvents.UI.StartGameRequested += OnStartNewGame;
            StartGame();
        }

        void OnStartNewGame(bool isRestart)
        {
            StartGame(useLastSeed: isRestart);
        }

        void Update()
        {
            if (_finished)
            {
                return;
            }

            if(!_running)
            {
                WaitForStableBoardAndFinish();
                return;                
            }

            _elapsed += Time.deltaTime;
            _gameEvents.Gameplay.DispatchTimerChanged(_elapsed, _totalTime);
            if (_elapsed >= _totalTime)
            {
                _gameEvents.Gameplay.DispatchTimerExpired();
                _running = false;
                WaitForStableBoardAndFinish();
            }
            else if (!_notifiedRunningOut && RemainingTime < _runOutElapsedRemaining)
            {
                _gameEvents.Gameplay.DispatchTimerRunningOut();
                _notifiedRunningOut = true;
            }
        }

        bool WaitForStableBoardAndFinish()
        {
            if (_boardController.IsStable)
            {
                _finished = true;
                _gameEvents.Gameplay.DispatchGameFinished(_score);
                return true;
            }
            return false;
        }

        void OnDestroy()
        {
            _gameEvents.Board.MatchesFound -= OnMatchesFound;
            _gameEvents.UI.StartGameRequested -= OnStartNewGame;
        }

        void OnMatchesFound(List<MatchInfo> matches, int chainStep)
        {
            int currentStepScore = 0;
            int chainMultiplier = _scoringRules.GetMultiplierForStep(chainStep);
            foreach (var match in matches)
            {
                int matchScore = 0;
                switch(match.MatchType)
                {
                    case MatchType.Match3:
                    {
                        matchScore = _scoringRules.Match3Score;
                        break;
                    }
                    case MatchType.Match4:
                    {
                        matchScore = _scoringRules.Match4Score;
                        break;
                    }
                    case MatchType.Match5:
                    {
                        matchScore = _scoringRules.Match5Score;
                        break;
                    }
                }
                matchScore *= chainMultiplier;
                _gameEvents.Gameplay.DispatchMatchProcessed(match, matchScore, chainMultiplier);
                currentStepScore += matchScore;
            }            
            _score += currentStepScore;
            _gameEvents.Gameplay.DispatchScoreChanged(currentStepScore, _score);
        }

        public void StartGame(bool useLastSeed = false)
        {
            if (_levelData.IsSeeded)
            {
                URandom.state = JsonUtility.FromJson<URandom.State>(_levelData.RandomSeed);
                _lastSeed = _levelData.RandomSeed;
            }
            else
            {
                if(useLastSeed)
                {
                    URandom.state = JsonUtility.FromJson<URandom.State>(_lastSeed);
                }
                else
                {
                    URandom.InitState(System.Environment.TickCount);
                }

                _lastSeed = JsonUtility.ToJson(URandom.state);
                Debug.Log($"Current random seed: {_lastSeed}");
            }

            _boardController.StartNewGame(_levelData);
            _elapsed = 0;
            _totalTime = _levelData.PlayTime;
            _running = true;
            _finished = false;
            _score = 0;
            _notifiedRunningOut = false;
            _gameEvents.Gameplay.DispatchGameStarted(_score, _elapsed, _totalTime);
            _boardController.BeginBoardUpdatePhase();
        }
    }

}
