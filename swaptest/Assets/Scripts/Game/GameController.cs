using UnityEngine;
using Game.Input;
using Game.Levels;
using Game.Events;
using URandom = UnityEngine.Random;
using Game.Board;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Entry point to the gameplay logic. 
    /// This class will deal with level data load, time and score tracking,
    /// and govern the flow.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        const string kGameStateFilename = "/gameState.sav";

        [SerializeField] BaseLevelData _levelData;
        [SerializeField] GameScoringRules _scoringRules;
        [SerializeField] Game.Board.BoardController _boardController;
        [SerializeField] InputController _inputController;
        [SerializeField] float _runningOutWarningTime = 5f;

        bool _running;
        bool _finished;
        int _score;
        bool _isNewHighScore;
        int _highScore;
        float _elapsed;
        float _totalTime;
        string _lastSeed;
        bool _notifiedRunningOut = false;
        private GameEvents _gameEvents;
        public float RemainingTime => _totalTime - _elapsed;
        public bool Running => _running;
        public bool Finished => _finished;

        GameState _gameState;

        void Awake()
        {
            _gameEvents = GameEvents.Instance;
            _gameEvents.Board.MatchesFound += OnMatchesFound;
            _gameEvents.UI.StartGameRequested += OnStartNewGame;
            _gameEvents.UI.ResetSaveRequested += OnRequestSaveReset;
            _gameState = new GameState(Application.persistentDataPath + kGameStateFilename);
        }

        void Start()
        {
            _gameState.Load();
            StartGame();
        }

        void OnDestroy()
        {
            _gameEvents.Board.MatchesFound -= OnMatchesFound;
            _gameEvents.UI.StartGameRequested -= OnStartNewGame;
            _gameEvents.UI.ResetSaveRequested -= OnRequestSaveReset;
        }


        void OnStartNewGame(bool isRestart)
        {
            StartGame(useLastSeed: isRestart);
        }

        void OnRequestSaveReset()
        {
            _gameState.Delete();
            _highScore = _levelData.HighScore;
            _isNewHighScore = _score > _highScore;
            _gameEvents.Gameplay.DispatchHighScoreChanged(_highScore);
        }

        void Update()
        {
            if (_finished)
            {
                return;
            }

            if(!_running)
            {
                CheckIfStableBoardAndFinish();
                return;                
            }

            _elapsed += Time.deltaTime;
            _gameEvents.Gameplay.DispatchTimerChanged(_elapsed, _totalTime);
            if (_elapsed >= _totalTime)
            {
                _gameEvents.Gameplay.DispatchTimerExpired();
                _running = false;
                CheckIfStableBoardAndFinish();
            }
            else if (!_notifiedRunningOut && RemainingTime <= _runningOutWarningTime)
            {
                _gameEvents.Gameplay.DispatchTimerRunningOut();
                _notifiedRunningOut = true;
            }
        }

        bool CheckIfStableBoardAndFinish()
        {
            if (_boardController.IsStable)
            {
                _finished = true;
                if(_isNewHighScore)
                {
                    _gameState.AddHighScoreEntry(_levelData.name, _highScore);
                }
                _gameState.Save();
                _gameEvents.Gameplay.DispatchGameFinished(_score, _isNewHighScore, _highScore);
                return true;
            }
            return false;
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
            if(_score > _highScore)
            {
                _highScore = _score;
                _isNewHighScore = true;
                _gameEvents.Gameplay.DispatchHighScoreChanged(_highScore);
            }
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

            var savedHighScore = _gameState.GetHighScoreForLevel(_levelData.name);
            if (savedHighScore >= 0)
            {
                _highScore = savedHighScore;
            }
            else
            {
                _highScore = _levelData.HighScore;
            }

            _isNewHighScore = false;
            _gameEvents.Gameplay.DispatchGameStarted(_score, _highScore, _elapsed, _totalTime);
            _boardController.BeginBoardUpdatePhase();           
        }
    }

}
