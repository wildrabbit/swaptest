using UnityEngine;
using Game.Input;
using Game.Levels;
using Game.Events;
using URandom = UnityEngine.Random;
using System.Timers;

namespace Game
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] BaseLevelData _levelData;
        [SerializeField] Game.Board.BoardController _boardController;
        [SerializeField] InputController _inputController;

        static GameEvents _gameEvents = new GameEvents();
        bool _running;
        bool _finished;
        int _score;
        float _elapsed;
        float _totalTime;
        string _lastSeed;

        Timer _debugTimer;

        public static GameEvents GameEvents => _gameEvents;
        public float RemainingTime => _totalTime - _elapsed;

        void Start()
        {
            StartGame();
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
                    _gameEvents.GameFlow.DispatchGameFinished(_score);
                    _finished = true;
                }
            }

            _elapsed += Time.deltaTime;
            if (_elapsed >= _totalTime)
            {
                StopTimer();
                _running = false;
            }
        }

        void OnDestroy()
        {
            StopTimer();
        }

        public void StartGame()
        {
            StartTimer();
            if (_levelData.IsSeeded)
            {
                URandom.state = JsonUtility.FromJson<URandom.State>(_levelData.RandomSeed);
                _lastSeed = _levelData.RandomSeed;
            }
            else
            {
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
            _gameEvents.GameFlow.DispatchGameStarted();
            _boardController.BeginBoardUpdatePhase();

        }

        void StartTimer()
        {
            _debugTimer = new Timer(1000);
            _debugTimer.Elapsed += OnTimer;
            _debugTimer.AutoReset = true;
            _debugTimer.Enabled = true;
        }

        void StopTimer()
        {
            _debugTimer.Elapsed -= OnTimer;
            _debugTimer.Stop();
            _debugTimer.Enabled = false;
        }

        void OnTimer(object sender, ElapsedEventArgs eventArgs)
        {
            Debug.Log($"Remaining: {RemainingTime}");
        }
    }

}
