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

        bool _running;
        bool _finished;
        int _score;
        float _elapsed;
        float _totalTime;
        string _lastSeed;
        private GameEvents _gameEvents;

        //Timer _debugTimer;

        public float RemainingTime => _totalTime - _elapsed;


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
                _running = false;
                WaitForStableBoardAndFinish();
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
            int delta = 0;
            foreach(var match in matches)
            {
                switch(match.MatchType)
                {
                    case MatchType.Match3:
                    {
                        delta += _scoringRules.Match3Score;
                        break;
                    }
                    case MatchType.Match4:
                    {
                        delta += _scoringRules.Match4Score;
                        break;
                    }
                    case MatchType.Match5:
                    {
                        delta += _scoringRules.Match5Score;
                        break;
                    }
                }
            }
            delta *= _scoringRules.GetMultiplierForStep(chainStep);
            _score += delta;
            _gameEvents.Gameplay.DispatchScoreChanged(delta, _score);
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
            _gameEvents.Gameplay.DispatchGameStarted(_score, _elapsed, _totalTime);
            _boardController.BeginBoardUpdatePhase();
            //StartTimer();
        }

        //void StartTimer()
        //{
        //    _debugTimer = new Timer(1000);
        //    _debugTimer.Elapsed += OnTimer;
        //    _debugTimer.AutoReset = true;
        //    _debugTimer.Enabled = true;
        //}

        //void StopTimer()
        //{
        //    _debugTimer.Elapsed -= OnTimer;
        //    _debugTimer.Stop();
        //    _debugTimer.Enabled = false;
        //}

        //void OnTimer(object sender, ElapsedEventArgs eventArgs)
        //{
        //    Debug.Log($"Remaining: {RemainingTime}");
        //}
    }

}
