using UnityEngine;
using Game.Input;
using Game.Levels;
using Game.Events;
using URandom = UnityEngine.Random;
using System.Timers;
using Game.Board;
using System;
using System.Collections.Generic;

namespace Game
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] BaseLevelData _levelData;
        [SerializeField] GameScoringRules _scoringRules;
        [SerializeField] Game.Board.BoardController _boardController;
        [SerializeField] InputController _inputController;

        static GameEvents _gameEvents = new GameEvents();
        bool _running;
        bool _finished;
        int _score;
        float _elapsed;
        float _totalTime;
        string _lastSeed;

        //Timer _debugTimer;

        public static GameEvents GameEvents => _gameEvents;
        public float RemainingTime => _totalTime - _elapsed;

        void Start()
        {
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
                if (_boardController.IsStable)
                {
                    _gameEvents.Gameplay.DispatchGameFinished(_score);
                    _finished = true;
                }
            }

            _elapsed += Time.deltaTime;
            _gameEvents.Gameplay.DispatchTimerChanged(_elapsed, _totalTime);
            if (_elapsed >= _totalTime)
            {
                //StopTimer();
                _running = false;
            }
        }

        void OnDestroy()
        {
           // StopTimer();
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

            _boardController.Init(_levelData);
            _inputController.Init();
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
