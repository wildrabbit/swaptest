using UnityEngine;
using System.Collections;
using System;

namespace Game.Events
{
    public class GameFlowEvents
    {
        public event Action GameStarted;
        public event Action<int> GameFinished;
        public event Action<int> ScoreChanged;
        public event Action<float> TimerChanged;

        public void DispatchGameStarted()
        {
            GameStarted?.Invoke();
        }
        public void DispatchGameFinished(int finalScore)
        {
            GameFinished?.Invoke(finalScore);
        }
        public void DispatchScoreChanged(int score)
        {
            ScoreChanged?.Invoke(score);
        }
        public void DispatchTimerChanged(float secondsLeft)
        {
            TimerChanged?.Invoke(secondsLeft);
        }
    }
    public class UIEvents
    {
        public event Action RestartRequested;
        public event Action PauseToggled;

        public void DispatchRestartRequested()
        {
            RestartRequested?.Invoke();
        }

        public void DispatchPauseToggled()
        {
            PauseToggled?.Invoke();
        }
    }

    public class GameEvents
    {
        public GameFlowEvents GameFlow = new GameFlowEvents();
        public UIEvents UI = new UIEvents();
    }
}
