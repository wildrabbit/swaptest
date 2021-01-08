using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace Game.UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] Text _score;
        [SerializeField] Text _timeLeft;

        [SerializeField] Button _toggleSfx;
        [SerializeField] Button _toggleMusic;

        private void Awake()
        {
            var gameFlowEvents = GameController.GameEvents.Gameplay;
            gameFlowEvents.GameStarted += OnGameStarted;
            gameFlowEvents.GameFinished += OnGameFinished;
            gameFlowEvents.ScoreChanged += OnScoreChanged;
            gameFlowEvents.TimerChanged += OnTimerChanged;
        }

        private void OnDestroy()
        {
            var gameFlowEvents = GameController.GameEvents.Gameplay;
            gameFlowEvents.GameStarted -= OnGameStarted;
            gameFlowEvents.GameFinished -= OnGameFinished;
            gameFlowEvents.ScoreChanged -= OnScoreChanged;
            gameFlowEvents.TimerChanged -= OnTimerChanged;
        }

        private void OnTimerChanged(float elapsed, float totalTime)
        {
            UpdateTime(totalTime - elapsed);
        }

        private void OnScoreChanged(int delta, int total)
        {
            UpdateScore(total);
        }

        private void OnGameFinished(int score)
        {
            UpdateScore(score);
        }

        void OnGameStarted(int score, float elapsed, float totalTime)
        {
            UpdateScore(score);
            UpdateTime(totalTime - elapsed);
        }

        void UpdateScore(int score)
        {
            _score.text = score.ToString("000000");
        }

        void UpdateTime(float secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);
            _timeLeft.text = t.ToString(@"mm\:ss");
        }

        
    }
}
